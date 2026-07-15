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

    private void Start()
    {
        // 当前局从 0 经验和配置的初始等级开始。
        PlayerData.getInstance().Exp = 0;
        PlayerData.getInstance().Level = currentLevel;

        // 经验曲线长度和成长倍率优先从 Lua 配置读取，便于热更新升级节奏。
        levelCount = LuaConfig.GetInt("config.exp_config", "level_count", levelCount);
        while (expLevels.Count < levelCount)
        {
            float multiplier = LuaConfig.GetFloat("config.exp_config", "growth_multiplier", 1.1f);
            expLevels.Add(Mathf.CeilToInt(expLevels[expLevels.Count - 1] * multiplier));
        }

        ReportExpChanged(true);
    }

    private void Update()
    {
        // 经验值仍然以 PlayerData 为数据源；本脚本只做升级判断和事件通知。
        currentExperience = PlayerData.getInstance().Exp;
        if (currentLevel >= 0 && currentLevel < expLevels.Count && currentExperience >= expLevels[currentLevel])
        {
            LevelUp();
            levelUpEvent?.Invoke();
        }

        ReportExpChanged(false);
    }

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
