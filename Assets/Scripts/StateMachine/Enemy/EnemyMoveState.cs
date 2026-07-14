using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人移动状态。
// 每帧追踪玩家，并根据受伤/死亡标记切换到对应状态。
public class EnemyMoveState : IState
{
    private Enemy enemy;
    public EnemyMoveState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        
    }


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
    public void OnFixUpData()
    {
       
    }
    public void OnExit()
    {

    }

}
