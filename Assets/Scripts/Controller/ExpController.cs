using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 经验与升级控制器。
/// 它负责读取 PlayerData.Exp、判断升级，并通过 GameEvents 通知 UI 刷新。
/// </summary>
public class ExpController : MonoBehaviour
{
    public int currentExperience;
    public List<int> expLevels;
    public int currentLevel = 1, levelCount;
    public UnityEvent levelUpEvent;

    private int lastReportedExp = -1;
    private int lastReportedLevel = -1;
    private int lastReportedLevelExp = -1;
    private bool initialized;
    private bool isProcessingExperience;

    private void OnEnable()
    {
        // PlayerData 仍是经验数据源；这里只监听实际变化，不再每帧读取。
        PlayerData.getInstance().OnExperienceChanged += OnExperienceChanged;
    }

    private void OnDisable()
    {
        PlayerData.getInstance().OnExperienceChanged -= OnExperienceChanged;
    }

    private void Start()
    {
        // 经验曲线长度和成长倍率优先从 Lua 配置读取，便于热更新升级节奏。
        levelCount = LuaConfig.GetInt("config.exp_config", "level_count", levelCount);
        if (expLevels == null)
        {
            expLevels = new List<int>();
        }

        // 配置缺少首级经验时使用安全默认值，避免扩展经验曲线时访问空列表。
        if (expLevels.Count == 0)
        {
            expLevels.Add(1);
        }

        while (expLevels.Count < levelCount)
        {
            float multiplier = LuaConfig.GetFloat("config.exp_config", "growth_multiplier", 1.1f);
            expLevels.Add(Mathf.CeilToInt(expLevels[expLevels.Count - 1] * multiplier));
        }

        // 当前局从 0 经验和配置的初始等级开始。先完成曲线初始化，再允许事件处理。
        PlayerData data = PlayerData.getInstance();
        data.Level = currentLevel;
        data.Exp = 0;
        initialized = true;
        ProcessExperience(data.Exp, true);
    }

    /// <summary>
    /// 升级规则优先交给 Lua。Lua 返回成功时，不再执行 C# 默认升级逻辑。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ExpController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void LevelUp()
    {
        // 升级规则优先交给 Lua。Lua 返回成功时，不再执行 C# 默认升级逻辑。
        if (LuaConfig.TryCallInt("hotfix.player.player_rule", "OnLevelUp", this, currentExperience, out int luaExperience))
        {
            currentExperience = luaExperience;
            PlayerData.getInstance().Exp = currentExperience;
            currentLevel = PlayerData.getInstance().Level;
            return;
        }

        DefaultLevelUp();
    }

    /// <summary>
    /// Lua 未接管时的默认升级：扣除当前等级经验，等级 +1。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ExpController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void DefaultLevelUp()
    {
        // Lua 未接管时的默认升级：扣除当前等级经验，等级 +1。
        currentExperience -= expLevels[currentLevel];
        PlayerData.getInstance().Exp = currentExperience;
        currentLevel++;
        if (currentLevel >= expLevels.Count)
        {
            currentLevel = expLevels.Count - 1;
        }
        PlayerData.getInstance().Level = currentLevel;
    }

    /// <summary>
    /// 经验属性变化时进入连续升级和经验事件处理流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ExpController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void OnExperienceChanged(int experience)
    {
        if (!initialized)
        {
            currentExperience = experience;
            return;
        }

        ProcessExperience(experience, false);
    }

    /// <summary>
    /// LevelUp 会把剩余经验重新写回 PlayerData.Exp，因此用保护避免事件重入。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ExpController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void ProcessExperience(int experience, bool forceReport)
    {
        // LevelUp 会把剩余经验重新写回 PlayerData.Exp，因此用保护避免事件重入。
        if (isProcessingExperience)
        {
            return;
        }

        isProcessingExperience = true;
        try
        {
            currentExperience = experience;
            int processedLevels = 0;
            int maxLevelsPerChange = Mathf.Max(1, expLevels.Count);

            // 一次获得大量经验时允许连续升级，但最多处理经验曲线长度次。
            while (currentLevel >= 0 &&
                   currentLevel < expLevels.Count &&
                   currentExperience >= expLevels[currentLevel])
            {
                int previousExperience = currentExperience;
                int previousLevel = currentLevel;

                LevelUp();
                currentExperience = PlayerData.getInstance().Exp;
                currentLevel = PlayerData.getInstance().Level;
                levelUpEvent?.Invoke();
                processedLevels++;

                // Lua 规则如果没有改变等级或经验，继续循环会永远无法退出。
                if (previousExperience == currentExperience && previousLevel == currentLevel)
                {
                    Debug.LogWarning("[ExpController] Level-up rule made no progress. Stop processing experience.", this);
                    break;
                }

                if (processedLevels >= maxLevelsPerChange)
                {
                    Debug.LogWarning("[ExpController] Reached the level-up safety limit.", this);
                    break;
                }
            }

            ReportExpChanged(forceReport);
        }
        finally
        {
            isProcessingExperience = false;
        }
    }

    /// <summary>
    /// 向调用方报告 ExpController 当前流程的状态或进度。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ExpController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void ReportExpChanged(bool force)
    {
        if (expLevels == null || expLevels.Count == 0)
        {
            return;
        }

        int levelIndex = Mathf.Clamp(currentLevel, 0, expLevels.Count - 1);
        int levelExp = expLevels[levelIndex];
        // 避免每帧重复广播相同经验，只有数值真的变化时才刷新 UI。
        if (!force &&
            lastReportedExp == currentExperience &&
            lastReportedLevel == currentLevel &&
            lastReportedLevelExp == levelExp)
        {
            return;
        }

        lastReportedExp = currentExperience;
        lastReportedLevel = currentLevel;
        lastReportedLevelExp = levelExp;
        GameEvents.RaiseExpChanged(currentExperience, levelExp, currentLevel);
    }
}
