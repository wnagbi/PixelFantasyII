using System;

/// <summary>
/// 全局事件中心，只负责“通知某个数据发生变化”，不保存任何数据。
/// 具体数据仍然放在 PlayerData、RunData、PlayerSaveStore 等数据源里。
/// </summary>
public static class GameEvents
{
    // current, max
    public static event Action<float, float> HealthChanged;

    // currentExp, levelExp, level
    public static event Action<int, int, int> ExpChanged;

    // level
    public static event Action<int> LevelChanged;

    // killCount
    public static event Action<int> KillCountChanged;

    // runTime
    public static event Action<float> RunTimeChanged;

    // score
    public static event Action<int> ScoreChanged;

    // skillId
    public static event Action<int> SkillUnlocked;

    // taskId, current, goal
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
