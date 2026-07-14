using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家死亡状态。
// 当前死亡流程主要由 onDie UnityEvent 处理，这个状态类先作为扩展占位。
public class PlayerDieState : IState
{
    private Player player;
    public PlayerDieState(Player player)
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
