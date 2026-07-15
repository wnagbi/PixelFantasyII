using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int version = 1;
    public int score = 0;
    public List<int> unlockedSkills = new List<int>();
}

/// <summary>
/// 玩家长期进度存档服务。
/// 当前只保存总分和技能解锁，不保存本局血量、经验、击杀数等运行时数据。
/// </summary>
public static class PlayerSaveStore
{
    private const string SaveFileName = "save_data.json";
    private static PlayerSaveData current;

    public static string FilePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
    }

    public static PlayerSaveData Current
    {
        get
        {
            // 延迟加载：第一次访问 Current 时再读取 JSON，避免启动顺序依赖。
            if (current == null)
            {
                Load();
            }

            return current;
        }
    }

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                // 没有存档时直接创建默认文件，不从旧 PlayerPrefs 迁移。
                ResetToDefault();
                return;
            }

            string json = File.ReadAllText(FilePath);
            current = JsonConvert.DeserializeObject<PlayerSaveData>(json);
            if (current == null)
            {
                Debug.LogWarning("[PlayerSaveStore] save_data.json is empty or invalid. Resetting to default.");
                ResetToDefault();
                return;
            }

            if (Normalize())
            {
                // 旧文件缺字段或数据异常时，修正后写回磁盘。
                Save();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PlayerSaveStore] Failed to load save data. Resetting to default. {ex.Message}");
            ResetToDefault();
        }
    }

    public static void Save()
    {
        if (current == null)
        {
            current = CreateDefault();
        }

        try
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(current, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PlayerSaveStore] Failed to save data: {ex.Message}");
        }
    }

    public static void ResetToDefault()
    {
        current = CreateDefault();
        Save();
    }

    public static bool IsSkillUnlocked(int skillId)
    {
        return Current.unlockedSkills.Contains(skillId);
    }

    public static bool TrySpendScore(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Current.score < amount)
        {
            return false;
        }

        Current.score -= amount;
        Save();

        // 存档变化后通知地图界面、商店等 UI 刷新。
        GameEvents.RaiseScoreChanged(Current.score);
        return true;
    }

    public static void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Current.score += amount;
        Save();

        // 结算加分后广播，已经打开的分数 UI 可以立即刷新。
        GameEvents.RaiseScoreChanged(Current.score);
    }

    public static void UnlockSkill(int skillId)
    {
        if (skillId <= 0 || Current.unlockedSkills.Contains(skillId))
        {
            return;
        }

        Current.unlockedSkills.Add(skillId);
        Save();

        // 技能解锁成功后通知商店图标和战斗技能按钮刷新。
        GameEvents.RaiseSkillUnlocked(skillId);
    }

    private static PlayerSaveData CreateDefault()
    {
        return new PlayerSaveData
        {
            version = 1,
            score = 0,
            unlockedSkills = new List<int>()
        };
    }

    private static bool Normalize()
    {
        // 对外部手动改 JSON、缺字段、重复技能 ID 做一次宽容修正。
        bool changed = false;

        if (current.version <= 0)
        {
            current.version = 1;
            changed = true;
        }

        if (current.score < 0)
        {
            current.score = 0;
            changed = true;
        }

        if (current.unlockedSkills == null)
        {
            current.unlockedSkills = new List<int>();
            changed = true;
        }

        for (int i = current.unlockedSkills.Count - 1; i >= 0; i--)
        {
            int skillId = current.unlockedSkills[i];
            if (skillId <= 0 || current.unlockedSkills.IndexOf(skillId) != i)
            {
                current.unlockedSkills.RemoveAt(i);
                changed = true;
            }
        }

        return changed;
    }
}
