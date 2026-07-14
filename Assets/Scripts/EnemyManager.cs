using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Mirror;
using UnityEngine;

// 场景内存活敌人注册表。
// 武器寻敌时不再依赖 FindObjects 或对象池子节点，而是从这里拿当前 active 的敌人列表。
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField]
    private List<Enemy> aliveEnemies;

    void Awake()
    {
        // 当前项目场景里只需要一个 EnemyManager。
        Instance = this;
    }
    public void AddEnemy(Enemy enemy)
    {
        // 敌人 OnEnable 时注册，表示它已从对象池取出并进入战斗。
        aliveEnemies.Add(enemy);
    }
    public void RemoveEnemy(Enemy enemy)
    {
        // 敌人 OnDisable 时注销，避免武器锁定已经回收到对象池的敌人。
        aliveEnemies.Remove(enemy);
    }
    public void Clear()
    {
        // 场景切换或重开时清空记录。
        aliveEnemies.Clear();
    }
    public List<Enemy> GetEnemiesList()
    {
        // 提供给飞剑等武器寻敌使用。
        return aliveEnemies;
    }
}
