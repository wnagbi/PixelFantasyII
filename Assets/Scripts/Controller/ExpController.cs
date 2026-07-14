using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 经验与升级控制器。
// 负责读取 PlayerData.Exp、刷新经验 UI、判断升级，并给 Lua 一个接管升级规则的机会。
public class ExpController : MonoBehaviour
{
    public int currentExperience;
    public List<int> expLevels;
    public int currentLevel = 1, levelCount;
    public UnityEvent levelUpEvent;

    private UIController UIController;

    private void Start()
    {
        PlayerData.getInstance().Exp = 0;
        PlayerData.getInstance().Level = currentLevel;
        UIController = GetComponent<UIController>();

        // 经验曲线长度和成长倍率优先从 Lua 配置读取，方便热更升级节奏。
        levelCount = LuaConfig.GetInt("config.exp_config", "level_count", levelCount);
        while (expLevels.Count < levelCount) 
        {
            float multiplier = LuaConfig.GetFloat("config.exp_config", "growth_multiplier", 1.1f);
            expLevels.Add(Mathf.CeilToInt(expLevels[expLevels.Count-1]*multiplier));
        }
    }
    private void Update()
    {
        currentExperience = PlayerData.getInstance().Exp;
        if (currentExperience >= expLevels[currentLevel]) 
        {

            LevelUp();
            levelUpEvent?.Invoke();
        }
        UIController.UpdateExp(currentExperience, expLevels[currentLevel],currentLevel);
    }
    private void LevelUp() 
    {
        // 升级事件先交给 Lua 规则。
        // Lua 可以处理扣经验、加等级、奖励等逻辑；返回成功时不再执行 C# 默认升级。
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
        // Lua 未接管时的默认升级逻辑。
        currentExperience -= expLevels[currentLevel];
        PlayerData.getInstance().Exp = currentExperience;
        currentLevel++;
        if (currentLevel > expLevels.Count)
        {
            currentLevel = expLevels.Count - 1;
        }
        PlayerData.getInstance().Level = currentLevel;
    }
}
