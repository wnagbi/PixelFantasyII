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
        this.player = player;
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
