using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人死亡状态。
// 进入时执行 EnemyDestroy，默认会计数、掉落并回收到对象池。
public class EnemyDieState : IState
{
    private Enemy enemy;
    /// <summary>
    /// 创建并绑定敌人死亡状态实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 EnemyDieState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public EnemyDieState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    /// <summary>
    /// 进入当前状态并应用该状态的初始表现。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnEnter()
    {
        // 执行死亡收尾。当前这里也会给 Lua 一个接管死亡规则的机会。
        enemy.EnemyDestroy();
    }


    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        if (!enemy.isDie) 
        {
            // 对象池重新启用或死亡标记清除后，回到移动状态。
            enemy.TransitionState(EnemyStateType.Move);
        }
    }
    /// <summary>
    /// 在固定时间步执行当前状态的物理更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnFixUpData()
    {
       
    }
    /// <summary>
    /// 退出当前状态并清理该状态留下的临时效果。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnExit()
    {
        // 离开死亡状态时清除死亡标记。
        enemy.isDie = false;
    }

}
