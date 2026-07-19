using System;

/// <summary>
/// 全局事件中心，只负责“通知某个数据发生变化”，不保存任何数据。
/// 具体数据仍然放在 PlayerData、RunData、PlayerSaveStore 等数据源里。
/// </summary>
public static class GameEvents
{
    // 参数依次为当前生命值、最大生命值。
    public static event Action<float, float> HealthChanged;

    // 参数依次为当前经验、升级所需经验、当前等级。
    public static event Action<int, int, int> ExpChanged;

    // 参数为变化后的玩家等级。
    public static event Action<int> LevelChanged;

    // 参数为本局最新击杀数。
    public static event Action<int> KillCountChanged;

    // 参数为本局已经运行的秒数。
    public static event Action<float> RunTimeChanged;

    // 参数为 JSON 存档中的最新总分。
    public static event Action<int> ScoreChanged;

    // 参数为刚解锁的技能 ID。
    public static event Action<int> SkillUnlocked;

    // 参数依次为任务 ID、当前进度、目标进度。
    public static event Action<int, int, int> TaskProgressChanged;

    /// <summary>
    /// 广播玩家当前生命值和最大生命值变化。
    /// </summary>
    /// <remarks>
    /// 使用注意：只由权威血量入口调用；订阅方必须在 OnDisable 中取消订阅。
    /// </remarks>
    public static void RaiseHealthChanged(float current, float max)
    {
        // 使用 ?.Invoke 可以在没有订阅者时安全跳过，不会空引用。
        HealthChanged?.Invoke(current, max);
    }

    /// <summary>
    /// 广播当前经验、升级需求和等级变化后的完整快照。
    /// </summary>
    /// <remarks>
    /// 使用注意：经验结算完成后再广播，避免 UI 显示升级过程中的中间值。
    /// </remarks>
    public static void RaiseExpChanged(int currentExp, int levelExp, int level)
    {
        ExpChanged?.Invoke(currentExp, levelExp, level);
    }

    /// <summary>
    /// 广播玩家等级变化。
    /// </summary>
    /// <remarks>
    /// 使用注意：只有等级实际变化时调用，避免重复触发升级相关表现。
    /// </remarks>
    public static void RaiseLevelChanged(int level)
    {
        LevelChanged?.Invoke(level);
    }

    /// <summary>
    /// 广播本局击杀数变化。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 RunData 统一调用，Enemy 和 UI 不应直接广播。
    /// </remarks>
    public static void RaiseKillCountChanged(int killCount)
    {
        KillCountChanged?.Invoke(killCount);
    }

    /// <summary>
    /// 广播本局显示时间变化。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 RunData 控制广播频率，UI 不应自行逐帧读取时间。
    /// </remarks>
    public static void RaiseRunTimeChanged(float runTime)
    {
        RunTimeChanged?.Invoke(runTime);
    }

    /// <summary>
    /// 广播玩家长期总分变化。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 PlayerSaveStore 在成功修改并保存分数后调用。
    /// </remarks>
    public static void RaiseScoreChanged(int score)
    {
        ScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// 广播指定技能 ID 已成功解锁。
    /// </summary>
    /// <remarks>
    /// 使用注意：重复解锁不会广播；监听方应重新读取 PlayerSaveStore 作为权威状态。
    /// </remarks>
    public static void RaiseSkillUnlocked(int skillId)
    {
        SkillUnlocked?.Invoke(skillId);
    }

    /// <summary>
    /// 广播指定任务的当前进度和目标值。
    /// </summary>
    /// <remarks>
    /// 使用注意：任务控制器完成内部进度计算后调用，事件本身不保存任务数据。
    /// </remarks>
    public static void RaiseTaskProgressChanged(int taskId, int current, int goal)
    {
        TaskProgressChanged?.Invoke(taskId, current, goal);
    }
}
