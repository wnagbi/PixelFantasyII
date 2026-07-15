using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家待机状态。
// 进入时播放 Idle 动画，检测到移动输入后切到 Move。
public class PlayerIdleState : IState
{
    private Player player;
    public PlayerIdleState(Player player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        // 播放待机动画。
        player.GetAni().Play("Idle");
    }


    public void OnUpData()
    {
        if (player.isRuning) 
        {
            // 有移动输入时切换到移动状态。
            player.TransitionState(PlayerStateType.Move);
        }

    }
    public void OnFixUpData()
    {
       
    }
    public void OnExit()
    {

    }

}
