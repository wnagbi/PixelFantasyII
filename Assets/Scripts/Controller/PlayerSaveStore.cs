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

    /// <summary>
    /// 加载 PlayerSaveStore 中与 Load 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
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

    /// <summary>
    /// 保存 PlayerSaveStore 当前维护的数据。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
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

    /// <summary>
    /// 重建默认数据、规范化字段并覆盖保存本地 JSON。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static void ResetToDefault()
    {
        current = CreateDefault();
        Save();
    }

    /// <summary>
    /// 判断 PlayerSaveStore 当前是否满足 IsSkillUnlocked 对应的状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool IsSkillUnlocked(int skillId)
    {
        return Current.unlockedSkills.Contains(skillId);
    }

    /// <summary>
    /// 尝试执行 TrySpendScore，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
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

    /// <summary>
    /// 向 PlayerSaveStore 添加 AddScore 对应的对象或数据。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
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

    /// <summary>
    /// 把技能 ID 加入存档、保存 JSON 并广播解锁事件。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
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

    /// <summary>
    /// 创建 PlayerSaveStore 中与 CreateDefault 对应的数据或对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 PlayerSaveStore 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static PlayerSaveData CreateDefault()
    {
        return new PlayerSaveData
        {
            version = 1,
            score = 0,
            unlockedSkills = new List<int>()
        };
    }

    /// <summary>
    /// 对外部手动改 JSON、缺字段、重复技能 ID 做一次宽容修正。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 PlayerSaveStore 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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
