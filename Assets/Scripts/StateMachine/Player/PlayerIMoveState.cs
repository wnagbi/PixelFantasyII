using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家移动状态。
// 进入时播放 Move 动画，停止移动后切回 Idle。
public class PlayerMoveState : IState
{
    private Player player;
    /// <summary>
    /// 创建并绑定玩家移动状态实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 PlayerMoveState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public PlayerMoveState(Player player)
    {
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
        // 播放移动动画。
        player.GetAni().Play("Move");
    }


    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        if (!player.isRuning)
        {
            // 没有移动输入时回到待机状态。
            player.TransitionState(PlayerStateType.Idle);
        }
    }
    /// <summary>
    /// 在固定时间步执行当前状态的物理更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnFixUpData()
    {
       
    }
    /// <summary>
    /// 退出当前状态并清理该状态留下的临时效果。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnExit()
    {

    }

}
