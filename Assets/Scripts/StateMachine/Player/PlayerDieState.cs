using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家死亡状态。
// 当前死亡流程主要由 onDie UnityEvent 处理，这个状态类先作为扩展占位。
public class PlayerDieState : IState
{
    private Player player;
    /// <summary>
    /// 保存状态所属玩家，供后续死亡动画和输入禁用使用。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 PlayerDieState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public PlayerDieState(Player player)
    {
        // 保存状态所属玩家，供后续死亡动画和输入禁用使用。
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
        // 预留：当前死亡表现由 Player.onDie UnityEvent 负责。
    }


    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        // 死亡状态默认不接受移动状态切换。
    }
    /// <summary>
    /// 在固定时间步执行当前状态的物理更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnFixUpData()
    {
        // 死亡状态没有物理更新。
    }
    /// <summary>
    /// 退出当前状态并清理该状态留下的临时效果。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnExit()
    {
        // 预留：复活流程加入后在这里恢复状态。
    }

}
