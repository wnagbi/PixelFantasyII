using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家移动状态。
// 进入时播放 Move 动画，停止移动后切回 Idle。
public class PlayerMoveState : IState
{
    private Player player;
    public PlayerMoveState(Player player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        // 播放移动动画。
        player.ani.Play("Move");
    }


    public void OnUpData()
    {
        if (!player.isRuning)
        {
            // 没有移动输入时回到待机状态。
            player.TransitionState(PlayerStateType.Idle);
        }
    }
    public void OnFixUpData()
    {
       
    }
    public void OnExit()
    {

    }

}
