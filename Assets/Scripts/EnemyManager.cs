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
        aliveEnemies.Clear();
    }

    public Enemy GetNearestEnemy(Vector3 origin)
    {
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
        CleanupInvalidEnemies();
        if (aliveEnemies.Count == 0)
        {
            return null;
        }

        return aliveEnemies[Random.Range(0, aliveEnemies.Count)];
    }

    public IReadOnlyList<Enemy> GetAliveEnemies()
    {
        CleanupInvalidEnemies();
        return aliveEnemies;
    }

    public List<Enemy> GetEnemiesList()
    {
        // 兼容旧代码。新代码优先使用 GetNearestEnemy/GetRandomEnemy 等寻敌接口。
        CleanupInvalidEnemies();
        return new List<Enemy>(aliveEnemies);
    }

    private void CleanupInvalidEnemies()
    {
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
        return enemy != null
            && enemy.gameObject.activeInHierarchy
            && enemy.live
            && !enemy.isDie
            && enemy.Health > 0f;
    }
}
