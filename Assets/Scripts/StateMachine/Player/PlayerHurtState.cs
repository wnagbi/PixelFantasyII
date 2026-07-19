using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家受伤状态。
// 当前受伤表现主要由 Player.PlayerHurt 事件处理，这个状态类先作为扩展占位。
public class PlayerHurtState : IState
{
    private Player player;
    /// <summary>
    /// 保存状态所属玩家，供后续受击硬直和动画控制使用。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 PlayerHurtState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public PlayerHurtState(Player player)
    {
        // 保存状态所属玩家，供后续受击硬直和动画控制使用。
        this.player = player;
    }

    /// <summary>
    /// 进入当前状态并应用该状态的初始表现。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnEnter()
    {
        // 预留：当前受击事件由 Player.PlayerHurt 直接触发。
    }


    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        // 预留：受击硬直结束后切回 Idle/Move。
    }
    /// <summary>
    /// 在固定时间步执行当前状态的物理更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnFixUpData()
    {
        // 预留：击退等物理行为应放在固定帧。
    }
    /// <summary>
    /// 退出当前状态并清理该状态留下的临时效果。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnExit()
    {
        // 预留：离开受伤状态时清理硬直标记。
    }

}
