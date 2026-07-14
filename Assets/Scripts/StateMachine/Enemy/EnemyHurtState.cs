using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人受伤状态。
// 进入时闪红，等待闪红结束后回到移动；如果受伤时已经死亡，则转死亡状态。
public class EnemyHurtState : IState
{
    private Enemy enemy;
    public EnemyHurtState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        // 闪红结束后 ResetColor 会把 isHurt 置回 false。
        enemy.FlashColor(0.1f);
        
    }
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
    public void OnFixUpData()
    {
       
    }
    public void OnExit()
    {
        
    }

}
