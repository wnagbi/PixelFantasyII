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
        // 保存状态所属敌人，后续扩展攻击距离或动画时直接使用。
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        // 预留：进入主动攻击状态时播放攻击动画或锁定目标。
    }


    public void OnUpData()
    {
        // 预留：按攻击前摇、距离和冷却执行攻击。
    }
    public void OnFixUpData()
    {
        // 预留：需要物理位移时放在固定帧执行。
    }
    public void OnExit()
    {
        // 预留：退出时清理攻击标记或动画参数。
    }

}
