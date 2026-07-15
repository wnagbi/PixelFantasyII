using System.Collections.Generic;
using UnityEngine;

// 场景内存活敌人注册表和统一寻敌服务。
// 敌人从对象池启用时注册，禁用/回收时注销，武器只通过这里查询有效敌人。
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField]
    private List<Enemy> aliveEnemies = new List<Enemy>();

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
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        aliveEnemies.Remove(enemy);
    }

    public void Clear()
    {
        // 场景切换、重新开始或调试时可以清空注册表。
        aliveEnemies.Clear();
    }

    public Enemy GetNearestEnemy(Vector3 origin)
    {
        // 找离 origin 最近的有效敌人。武器寻最近目标时优先用这个接口。
        Enemy nearest = null;
        float nearestSqrDistance = float.MaxValue;

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
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    public Enemy GetNearestEnemyInRange(Vector3 origin, float range)
    {
        // 只在指定范围内寻找最近敌人，适合有攻击距离限制的武器。
        Enemy nearest = null;
        float nearestSqrDistance = Mathf.Max(0f, range) * Mathf.Max(0f, range);

        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = aliveEnemies[i];
            if (!IsValidEnemy(enemy))
            {
                aliveEnemies.RemoveAt(i);
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
        // 这里统一过滤禁用、死亡、回收中或血量归零的敌人。
        return enemy != null
            && enemy.gameObject.activeInHierarchy
            && enemy.live
            && !enemy.isDie
            && enemy.Health > 0f;
    }
}
