using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人待机状态。
// 当前默认敌人基本直接追踪玩家，因此 Idle 暂时没有具体行为。
public class EnemyIdleState : IState
{
    private Enemy enemy;
    public EnemyIdleState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        
    }


    public void OnUpData()
    {


    }
    public void OnFixUpData()
    {
       
    }
    public void OnExit()
    {

    }

}
