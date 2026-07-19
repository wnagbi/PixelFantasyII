using System.Collections.Generic;
using UnityEngine;

// 场景内存活敌人注册表和统一寻敌服务。
// 敌人从对象池启用时注册，禁用/回收时注销，武器只通过这里查询有效敌人。
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Spatial Query")]
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private float defaultNearestSearchRange = 30f;
    [SerializeField] private bool drawDebugGrid;

    [SerializeField]
    private List<Enemy> aliveEnemies = new List<Enemy>();

    private readonly Dictionary<Vector2Int, List<Enemy>> spatialGrid = new Dictionary<Vector2Int, List<Enemy>>();
    private readonly Stack<List<Enemy>> gridListPool = new Stack<List<Enemy>>();
    private readonly List<Enemy> queryBuffer = new List<Enemy>();
    private int lastGridBuildFrame = -1;

    public int AliveCount
    {
        get
        {
            // 读取数量前先清理一次，避免统计到已经回收到对象池的敌人。
            CleanupInvalidEnemies();
            return aliveEnemies.Count;
        }
    }

    private void Awake()
    {
        // 当前项目场景里只需要一个 EnemyManager。
        Instance = this;
        if (aliveEnemies == null)
        {
            aliveEnemies = new List<Enemy>();
        }
    }

    /// <summary>
    /// 将启用且有效的敌人加入统一寻敌注册表。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Enemy.OnEnable 调用；方法会忽略 null 和重复注册。
    /// </remarks>
    public void AddEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        // 对象池重复启用或脚本重复调用时，避免同一个敌人被加入多次。
        if (!aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Add(enemy);
            lastGridBuildFrame = -1;
        }
    }

    /// <summary>
    /// 从寻敌注册表移除禁用、死亡回收或销毁的敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Enemy.OnDisable 调用；移除后会让空间网格在下次查询时重建。
    /// </remarks>
    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (aliveEnemies.Remove(enemy))
        {
            lastGridBuildFrame = -1;
        }
    }

    /// <summary>
    /// 清空敌人注册表、空间网格和列表复用池。
    /// </summary>
    /// <remarks>
    /// 使用注意：只在场景重置或管理器销毁时调用，战斗中调用会让现有敌人暂时无法被寻敌。
    /// </remarks>
    public void Clear()
    {
        // 场景切换、重新开始或调试时可以清空注册表。
        aliveEnemies.Clear();
        ReleaseGridLists();
        queryBuffer.Clear();
        lastGridBuildFrame = -1;
    }

    /// <summary>
    /// 获取指定世界坐标附近最近的有效敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：先查询默认范围，找不到时回退全表扫描；无目标返回 null。
    /// </remarks>
    public Enemy GetNearestEnemy(Vector3 origin)
    {
        // 优先在默认范围内通过空间网格查询，通常武器只关心屏幕附近的目标。
        Enemy nearest = FindNearestInGrid(origin, defaultNearestSearchRange);
        if (nearest != null)
        {
            return nearest;
        }

        // 默认范围内找不到时回退全列表扫描，保证“最近敌人”接口语义不变。
        return FindNearestByFullScan(origin, float.MaxValue);
    }

    /// <summary>
    /// 获取指定圆形范围内最近的有效敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：range 小于等于 0 时返回 null；查询使用网格粗筛和距离平方精筛。
    /// </remarks>
    public Enemy GetNearestEnemyInRange(Vector3 origin, float range)
    {
        // 只在指定范围内寻找最近敌人，适合有攻击距离限制的武器。
        return FindNearestInGrid(origin, Mathf.Max(0f, range));
    }

    /// <summary>
    /// 从当前全部有效敌人中随机返回一个目标。
    /// </summary>
    /// <remarks>
    /// 使用注意：随机查询不使用空间网格；没有有效敌人时返回 null。
    /// </remarks>
    public Enemy GetRandomEnemy()
    {
        // 随机寻敌前先清掉无效对象，避免随机到已死亡/已回收敌人。
        CleanupInvalidEnemies();
        if (aliveEnemies.Count == 0)
        {
            return null;
        }

        return aliveEnemies[Random.Range(0, aliveEnemies.Count)];
    }

    /// <summary>
    /// 返回清理无效对象后的敌人只读列表。
    /// </summary>
    /// <remarks>
    /// 使用注意：返回的是内部列表的只读接口，外部不得长期缓存其中可能被对象池回收的 Enemy。
    /// </remarks>
    public IReadOnlyList<Enemy> GetAliveEnemies()
    {
        // 返回只读接口，表达“外部可以遍历查看，但不要修改注册表”。
        // 如果以后需要绝对防止外部强转修改，可以改为 return new List<Enemy>(aliveEnemies)。
        CleanupInvalidEnemies();
        return aliveEnemies;
    }

    /// <summary>
    /// 全表扫描并计算指定范围内最近的有效敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅作为无范围查询的正确性回退，不应替代常规网格查询。
    /// </remarks>
    private Enemy FindNearestByFullScan(Vector3 origin, float range)
    {
        Enemy nearest = null;
        float safeRange = Mathf.Max(0f, range);
        float nearestSqrDistance = safeRange >= float.MaxValue ? float.MaxValue : safeRange * safeRange;

        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = aliveEnemies[i];
            if (!IsValidEnemy(enemy))
            {
                aliveEnemies.RemoveAt(i);
                continue;
            }

            // 使用 sqrMagnitude 避免 Vector3.Distance 的开方开销。
            float sqrDistance = (enemy.transform.position - origin).sqrMagnitude;
            if (sqrDistance <= nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 使用空间网格收集范围候选并返回最近敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前会确保网格为当前帧数据；queryBuffer 是复用缓存，不得返回给外部。
    /// </remarks>
    private Enemy FindNearestInGrid(Vector3 origin, float range)
    {
        if (range <= 0f)
        {
            return null;
        }

        EnsureGridCurrent();
        QueryCellsInRange(origin, range, queryBuffer);

        Enemy nearest = null;
        float nearestSqrDistance = range * range;

        for (int i = 0; i < queryBuffer.Count; i++)
        {
            Enemy enemy = queryBuffer[i];
            if (!IsValidEnemy(enemy))
            {
                continue;
            }

            float sqrDistance = (enemy.transform.position - origin).sqrMagnitude;
            if (sqrDistance <= nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 确保当前帧首次查询前已经完成一次网格重建。
    /// </summary>
    /// <remarks>
    /// 使用注意：同一帧多次寻敌只能重建一次，避免多武器查询重复产生开销。
    /// </remarks>
    private void EnsureGridCurrent()
    {
        if (lastGridBuildFrame == Time.frameCount)
        {
            return;
        }

        RebuildGrid();
    }

    /// <summary>
    /// 清理无效敌人并按当前位置重新填充空间网格。
    /// </summary>
    /// <remarks>
    /// 使用注意：敌人移动不做增量更新，因此查询依赖每帧懒重建获得最新格子。
    /// </remarks>
    private void RebuildGrid()
    {
        ReleaseGridLists();

        float safeCellSize = Mathf.Max(0.01f, cellSize);
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = aliveEnemies[i];
            if (!IsValidEnemy(enemy))
            {
                aliveEnemies.RemoveAt(i);
                continue;
            }

            Vector2Int cell = WorldToCell(enemy.transform.position, safeCellSize);
            if (!spatialGrid.TryGetValue(cell, out List<Enemy> enemiesInCell))
            {
                enemiesInCell = GetGridList();
                spatialGrid.Add(cell, enemiesInCell);
            }

            enemiesInCell.Add(enemy);
        }

        lastGridBuildFrame = Time.frameCount;
    }

    /// <summary>
    /// 从列表池取得一个已清空的格子敌人列表。
    /// </summary>
    /// <remarks>
    /// 使用注意：取得的列表必须在下次重建前通过 ReleaseGridLists 归还。
    /// </remarks>
    private List<Enemy> GetGridList()
    {
        if (gridListPool.Count > 0)
        {
            return gridListPool.Pop();
        }

        return new List<Enemy>();
    }

    /// <summary>
    /// 清空所有网格列表并归还到列表对象池。
    /// </summary>
    /// <remarks>
    /// 使用注意：归还后 spatialGrid 不再保留这些列表引用，避免同一列表被多个格子复用。
    /// </remarks>
    private void ReleaseGridLists()
    {
        foreach (List<Enemy> enemiesInCell in spatialGrid.Values)
        {
            enemiesInCell.Clear();
            gridListPool.Push(enemiesInCell);
        }

        spatialGrid.Clear();
    }

    /// <summary>
    /// 使用当前 cellSize 将世界坐标转换为网格坐标。
    /// </summary>
    /// <remarks>
    /// 使用注意：cellSize 会经过最小值保护，防止 Inspector 配置为 0 时除零。
    /// </remarks>
    private Vector2Int WorldToCell(Vector3 position)
    {
        return WorldToCell(position, Mathf.Max(0.01f, cellSize));
    }

    /// <summary>
    /// 使用已经校验过的格子尺寸将世界坐标转换为网格坐标。
    /// </summary>
    /// <remarks>
    /// 使用注意：safeCellSize 必须大于 0，仅供网格内部循环减少重复校验。
    /// </remarks>
    private Vector2Int WorldToCell(Vector3 position, float safeCellSize)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / safeCellSize),
            Mathf.FloorToInt(position.y / safeCellSize)
        );
    }

    /// <summary>
    /// 把覆盖查询圆形包围盒的格子敌人收集到复用结果列表。
    /// </summary>
    /// <remarks>
    /// 使用注意：该方法只做格子粗筛，调用方仍需用距离平方执行真实圆形范围判断。
    /// </remarks>
    private void QueryCellsInRange(Vector3 origin, float range, List<Enemy> results)
    {
        results.Clear();

        float safeCellSize = Mathf.Max(0.01f, cellSize);
        Vector2Int minCell = WorldToCell(new Vector3(origin.x - range, origin.y - range, origin.z), safeCellSize);
        Vector2Int maxCell = WorldToCell(new Vector3(origin.x + range, origin.y + range, origin.z), safeCellSize);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!spatialGrid.TryGetValue(cell, out List<Enemy> enemiesInCell))
                {
                    continue;
                }

                for (int i = 0; i < enemiesInCell.Count; i++)
                {
                    results.Add(enemiesInCell[i]);
                }
            }
        }
    }

    /// <summary>
    /// 从注册表移除死亡、禁用、回收、空引用或生命值无效的敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：清理会改变列表顺序；外部不能持有索引作为稳定敌人标识。
    /// </remarks>
    private void CleanupInvalidEnemies()
    {
        // 从后往前删，避免 RemoveAt 后影响还没遍历到的索引。
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (!IsValidEnemy(aliveEnemies[i]))
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 判断敌人是否仍可作为武器和 Lua 规则的合法目标。
    /// </summary>
    /// <remarks>
    /// 使用注意：isDie 为 true 表示已死亡，同时还要检查 active、live 和 Health。
    /// </remarks>
    private bool IsValidEnemy(Enemy enemy)
    {
        // 对象池项目里“还在列表里”不代表“还能被锁定”。
        // 注意：isDie == true 表示敌人已经死亡，所以有效目标必须满足 !enemy.isDie。
        // 这里统一过滤禁用、已死亡、回收中或血量归零的敌人。
        return enemy != null
            && enemy.gameObject.activeInHierarchy
            && enemy.live
            && !enemy.isDie
            && enemy.Health > 0f;
    }

    /// <summary>
    /// 在 Scene 视图绘制 EnemyManager 的调试辅助信息。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Unity 按生命周期或消息规则自动调用，不要从普通业务代码直接调用。
    /// </remarks>
    private void OnDrawGizmos()
    {
        if (!drawDebugGrid)
        {
            return;
        }

        float safeCellSize = Mathf.Max(0.01f, cellSize);
        Vector3 debugCenter = transform.position;
        if (Application.isPlaying && PlayerRuntimeRegistry.TryGetPlayerTransform(out Transform playerTransform))
        {
            debugCenter = playerTransform.position;
        }

        // 黄色圆圈表示 GetNearestEnemy 默认会优先扫描的范围。
        Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(debugCenter, Mathf.Max(0f, defaultNearestSearchRange));

        if (spatialGrid == null || spatialGrid.Count == 0)
        {
            return;
        }

        // 半透明方框表示当前帧空间网格中有敌人的格子。
        Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.25f);
        foreach (Vector2Int cell in spatialGrid.Keys)
        {
            Vector3 center = new Vector3(
                (cell.x + 0.5f) * safeCellSize,
                (cell.y + 0.5f) * safeCellSize,
                0f
            );
            Gizmos.DrawWireCube(center, new Vector3(safeCellSize, safeCellSize, 0f));
        }
    }
}
