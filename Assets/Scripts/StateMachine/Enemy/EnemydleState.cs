using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人待机状态。
// 当前默认敌人基本直接追踪玩家，因此 Idle 暂时没有具体行为。
public class EnemyIdleState : IState
{
    private Enemy enemy;
    public EnemyIdleState(Enemy enemy)
    {
        // 保存状态所属敌人，方便后续加入警戒范围判断。
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        // 预留：播放待机动画并清除移动速度。
    }


    public void OnUpData()
    {
        // 预留：发现玩家后切换到 Move。
    }
    public void OnFixUpData()
    {
        // Idle 当前没有物理逻辑。
    }
    public void OnExit()
    {
        // 预留：离开待机时恢复追踪参数。
    }

}
