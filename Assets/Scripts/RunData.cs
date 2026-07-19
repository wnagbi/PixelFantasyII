using UnityEngine;

/// <summary>
/// 当前局运行时数据。
/// 这些数据只在一局游戏内有效，不写入 JSON，也不再通过 PlayerPrefs 实时传递。
/// </summary>
public static class RunData
{
    public static int KillCount { get; private set; }
    public static float RunTime { get; private set; }

    /// <summary>
    /// 清空本局击杀数和生存时间，并广播对应的初始值。
    /// </summary>
    /// <remarks>
    /// 使用注意：只在开始新一局时调用；不要在暂停、切换普通 UI 时重置。
    /// </remarks>
    public static void Reset()
    {
        // 新开一局时清空运行时状态，并通知已经打开的 HUD/任务 UI 刷新。
        KillCount = 0;
        RunTime = 0f;
        GameEvents.RaiseKillCountChanged(KillCount);
        GameEvents.RaiseRunTimeChanged(RunTime);
    }

    /// <summary>
    /// 增加一次本局击杀计数并立即广播给 HUD 和任务系统。
    /// </summary>
    /// <remarks>
    /// 使用注意：只能在一次敌人死亡结算中调用一次，避免对象池重复回收造成重复计数。
    /// </remarks>
    public static void AddKill()
    {
        // 敌人死亡时只调用这里，避免多个系统各自维护击杀数。
        KillCount = Mathf.Max(0, KillCount + 1);
        GameEvents.RaiseKillCountChanged(KillCount);
    }

    /// <summary>
    /// 保存精确本局时间，并在整数秒变化时广播显示更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：允许逐帧调用；事件经过秒级降频，但 RunTime 始终保留最新精度。
    /// </remarks>
    public static void SetRunTime(float time)
    {
        // 保留最新的浮点时间供结算读取，但 UI 通知只在整数秒变化时发送。
        int previousSecond = Mathf.FloorToInt(RunTime);
        RunTime = Mathf.Max(0f, time);
        int currentSecond = Mathf.FloorToInt(RunTime);
        if (previousSecond != currentSecond)
        {
            GameEvents.RaiseRunTimeChanged(RunTime);
        }
    }
}
