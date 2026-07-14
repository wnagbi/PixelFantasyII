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

// 玩家长期进度存档服务。
// 当前只保存 Score 和技能解锁状态，不保存当前局血量、经验、KillNum 等运行时数据。
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
    }

    public static void UnlockSkill(int skillId)
    {
        if (skillId <= 0 || Current.unlockedSkills.Contains(skillId))
        {
            return;
        }

        Current.unlockedSkills.Add(skillId);
        Save();
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
