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
        // 保存状态所属玩家，供后续死亡动画和输入禁用使用。
        this.player = player;
    }

    public void OnEnter()
    {
        // 预留：当前死亡表现由 Player.onDie UnityEvent 负责。
    }


    public void OnUpData()
    {
        // 死亡状态默认不接受移动状态切换。
    }
    public void OnFixUpData()
    {
        // 死亡状态没有物理更新。
    }
    public void OnExit()
    {
        // 预留：复活流程加入后在这里恢复状态。
    }

}
