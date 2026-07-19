using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人待机状态。
// 当前默认敌人基本直接追踪玩家，因此 Idle 暂时没有具体行为。
public class EnemyIdleState : IState
{
    private Enemy enemy;
    /// <summary>
    /// 保存状态所属敌人，方便后续加入警戒范围判断。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 EnemyIdleState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public EnemyIdleState(Enemy enemy)
    {
        // 保存状态所属敌人，方便后续加入警戒范围判断。
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
        // 预留：播放待机动画并清除移动速度。
    }


    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        // 预留：发现玩家后切换到 Move。
    }
    /// <summary>
    /// 在固定时间步执行当前状态的物理更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnFixUpData()
    {
        // Idle 当前没有物理逻辑。
    }
    /// <summary>
    /// 退出当前状态并清理该状态留下的临时效果。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnExit()
    {
        // 预留：离开待机时恢复追踪参数。
    }

}
