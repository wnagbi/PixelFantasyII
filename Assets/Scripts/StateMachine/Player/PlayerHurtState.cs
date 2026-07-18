using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家受伤状态。
// 当前受伤表现主要由 Player.PlayerHurt 事件处理，这个状态类先作为扩展占位。
public class PlayerHurtState : IState
{
    private Player player;
    public PlayerHurtState(Player player)
    {
        // 保存状态所属玩家，供后续受击硬直和动画控制使用。
        this.player = player;
    }

    public void OnEnter()
    {
        // 预留：当前受击事件由 Player.PlayerHurt 直接触发。
    }


    public void OnUpData()
    {
        // 预留：受击硬直结束后切回 Idle/Move。
    }
    public void OnFixUpData()
    {
        // 预留：击退等物理行为应放在固定帧。
    }
    public void OnExit()
    {
        // 预留：离开受伤状态时清理硬直标记。
    }

}
