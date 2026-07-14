using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人攻击状态。
// 当前敌人主要通过触发器接触玩家造成伤害，这个状态类暂时作为后续攻击行为扩展点。
public class EnemyAttackState : IState
{
    private Enemy enemy;
    public EnemyAttackState(Enemy enemy)
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
