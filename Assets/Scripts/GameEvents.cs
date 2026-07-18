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

    public static void RaiseHealthChanged(float current, float max)
    {
        // 使用 ?.Invoke 可以在没有订阅者时安全跳过，不会空引用。
        HealthChanged?.Invoke(current, max);
    }

    public static void RaiseExpChanged(int currentExp, int levelExp, int level)
    {
        ExpChanged?.Invoke(currentExp, levelExp, level);
    }

    public static void RaiseLevelChanged(int level)
    {
        LevelChanged?.Invoke(level);
    }

    public static void RaiseKillCountChanged(int killCount)
    {
        KillCountChanged?.Invoke(killCount);
    }

    public static void RaiseRunTimeChanged(float runTime)
    {
        RunTimeChanged?.Invoke(runTime);
    }

    public static void RaiseScoreChanged(int score)
    {
        ScoreChanged?.Invoke(score);
    }

    public static void RaiseSkillUnlocked(int skillId)
    {
        SkillUnlocked?.Invoke(skillId);
    }

    public static void RaiseTaskProgressChanged(int taskId, int current, int goal)
    {
        TaskProgressChanged?.Invoke(taskId, current, goal);
    }
}
