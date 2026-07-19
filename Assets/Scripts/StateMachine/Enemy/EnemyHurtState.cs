using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人受伤状态。
// 进入时闪红，等待闪红结束后回到移动；如果受伤时已经死亡，则转死亡状态。
public class EnemyHurtState : IState
{
    private Enemy enemy;
    /// <summary>
    /// 创建并绑定敌人受伤状态实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 EnemyHurtState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public EnemyHurtState(Enemy enemy)
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
        // 闪红结束后 ResetColor 会把 isHurt 置回 false。
        enemy.FlashColor(0.1f);
        
    }
    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        if (enemy.isDie&& enemy.isHurt) 
        {
            // 受伤过程中血量归零，进入死亡流程。
            enemy.TransitionState(EnemyStateType.Die);
        }
        if (!enemy.isHurt) 
        {
            // 受伤表现结束后继续追踪玩家。
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
        
    }

}
