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

    public void Clear()
    {
        // 场景切换、重新开始或调试时可以清空注册表。
        aliveEnemies.Clear();
        ReleaseGridLists();
        queryBuffer.Clear();
        lastGridBuildFrame = -1;
    }

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

    public Enemy GetNearestEnemyInRange(Vector3 origin, float range)
    {
        // 只在指定范围内寻找最近敌人，适合有攻击距离限制的武器。
        return FindNearestInGrid(origin, Mathf.Max(0f, range));
    }

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

    public IReadOnlyList<Enemy> GetAliveEnemies()
    {
        // 返回只读接口，表达“外部可以遍历查看，但不要修改注册表”。
        // 如果以后需要绝对防止外部强转修改，可以改为 return new List<Enemy>(aliveEnemies)。
        CleanupInvalidEnemies();
        return aliveEnemies;
    }

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

    private void EnsureGridCurrent()
    {
        if (lastGridBuildFrame == Time.frameCount)
        {
            return;
        }

        RebuildGrid();
    }

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

    private List<Enemy> GetGridList()
    {
        if (gridListPool.Count > 0)
        {
            return gridListPool.Pop();
        }

        return new List<Enemy>();
    }

    private void ReleaseGridLists()
    {
        foreach (List<Enemy> enemiesInCell in spatialGrid.Values)
        {
            enemiesInCell.Clear();
            gridListPool.Push(enemiesInCell);
        }

        spatialGrid.Clear();
    }

    private Vector2Int WorldToCell(Vector3 position)
    {
        return WorldToCell(position, Mathf.Max(0.01f, cellSize));
    }

    private Vector2Int WorldToCell(Vector3 position, float safeCellSize)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / safeCellSize),
            Mathf.FloorToInt(position.y / safeCellSize)
        );
    }

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
