using UnityEngine;

/// <summary>
/// 当前局运行时数据。
/// 这些数据只在一局游戏内有效，不写入 JSON，也不再通过 PlayerPrefs 实时传递。
/// </summary>
public static class RunData
{
    public static int KillCount { get; private set; }
    public static float RunTime { get; private set; }

    public static void Reset()
    {
        // 新开一局时清空运行时状态，并通知已经打开的 HUD/任务 UI 刷新。
        KillCount = 0;
        RunTime = 0f;
        GameEvents.RaiseKillCountChanged(KillCount);
        GameEvents.RaiseRunTimeChanged(RunTime);
    }

    public static void AddKill()
    {
        // 敌人死亡时只调用这里，避免多个系统各自维护击杀数。
        KillCount = Mathf.Max(0, KillCount + 1);
        GameEvents.RaiseKillCountChanged(KillCount);
    }

    public static void SetRunTime(float time)
    {
        // TimeController 每帧更新这里，结算界面直接读取 RunData.RunTime。
        RunTime = Mathf.Max(0f, time);
        GameEvents.RaiseRunTimeChanged(RunTime);
    }
}
