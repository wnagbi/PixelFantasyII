using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人攻击状态。
// 当前敌人主要通过触发器接触玩家造成伤害，这个状态类暂时作为后续攻击行为扩展点。
public class EnemyAttackState : IState
{
    private Enemy enemy;
    /// <summary>
    /// 保存状态所属敌人，后续扩展攻击距离或动画时直接使用。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 EnemyAttackState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public EnemyAttackState(Enemy enemy)
    {
        // 保存状态所属敌人，后续扩展攻击距离或动画时直接使用。
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
        // 预留：进入主动攻击状态时播放攻击动画或锁定目标。
    }


    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        // 预留：按攻击前摇、距离和冷却执行攻击。
    }
    /// <summary>
    /// 在固定时间步执行当前状态的物理更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnFixUpData()
    {
        // 预留：需要物理位移时放在固定帧执行。
    }
    /// <summary>
    /// 退出当前状态并清理该状态留下的临时效果。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnExit()
    {
        // 预留：退出时清理攻击标记或动画参数。
    }

}
