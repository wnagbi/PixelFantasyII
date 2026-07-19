using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人移动状态。
// 每帧追踪玩家，并根据受伤/死亡标记切换到对应状态。
public class EnemyMoveState : IState
{
    private Enemy enemy;
    /// <summary>
    /// 创建并绑定敌人移动状态实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 EnemyMoveState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public EnemyMoveState(Enemy enemy)
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
        
    }


    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        // 持续追踪玩家。
        enemy.ChasePlayer();
        if (enemy.isHurt) 
        {
            // 受伤标记由 Enemy.EnemeyHurt 或 Lua 状态逻辑设置。
            enemy.TransitionState(EnemyStateType.Hurt);
        }
        if (enemy.isDie && !enemy.isHurt) 
        {
            // 已死亡且不在受伤表现中时进入死亡状态。
            enemy.TransitionState(EnemyStateType.Die);
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

    }

}
