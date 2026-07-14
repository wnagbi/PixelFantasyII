using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
// settings.json 对应的数据结构。
// 只保存本地用户设置，不保存玩家等级、金币、任务、技能解锁等存档数据。
public class GameSettingsData
{
    // 设置文件版本。以后字段结构变化时可以用它做兼容升级。
    public int version = 1;

    // 当前语言在 LocalizationSettings.AvailableLocales.Locales 中的索引。
    public int languageIndex = 0;

    // 分辨率保存宽高，不保存 Dropdown 索引，避免不同电脑分辨率列表顺序不同。
    public int resolutionWidth;
    public int resolutionHeight;
    public bool fullscreen = true;

    public bool vibration = false;

    public float masterVolume = 0f;
    public float musicVolume = 0f;
    public float vfVolume = 0f;
}

// 游戏设置 JSON 存储服务。
// 统一负责从 Application.persistentDataPath/settings.json 读取和保存设置。
public static class GameSettingsStore
{
    private const string SettingsFileName = "settings.json";
    private static GameSettingsData current;

    public static string FilePath
    {
        // 持久化目录。Windows 打包后通常在用户 AppData/LocalLow 下，不在游戏安装目录里。
        get { return Path.Combine(Application.persistentDataPath, SettingsFileName); }
    }

    public static GameSettingsData Current
    {
        get
        {
            // 懒加载：外部第一次访问 Current 时自动 Load，避免忘记初始化。
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
            // 没有 settings.json 时直接创建默认文件。
            if (!File.Exists(FilePath))
            {
                ResetToDefault();
                return;
            }

            // 使用 Newtonsoft.Json 把 JSON 文本反序列化成 GameSettingsData 对象。
            string json = File.ReadAllText(FilePath);
            current = JsonConvert.DeserializeObject<GameSettingsData>(json);
            if (current == null)
            {
                Debug.LogWarning("[GameSettingsStore] settings.json is empty or invalid. Resetting to default.");
                ResetToDefault();
                return;
            }

            // 对旧版本/缺字段/非法值做一次规范化，必要时立刻保存修正后的文件。
            if (Normalize())
            {
                Save();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GameSettingsStore] Failed to load settings. Resetting to default. {ex.Message}");
            ResetToDefault();
        }
    }

    public static void Save()
    {
        // 理论上 current 应该已经由 Load 创建；这里再兜底一次。
        if (current == null)
        {
            current = CreateDefault();
        }

        try
        {
            // 确保 settings.json 所在目录存在。
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Formatting.Indented 会输出带缩进的 JSON，方便你手动查看和测试。
            string json = JsonConvert.SerializeObject(current, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GameSettingsStore] Failed to save settings: {ex.Message}");
        }
    }

    public static void ResetToDefault()
    {
        // 重建默认设置并立刻写入 settings.json。
        current = CreateDefault();
        Save();
    }

    private static GameSettingsData CreateDefault()
    {
        // 默认分辨率使用当前屏幕分辨率。
        Resolution resolution = Screen.currentResolution;
        return new GameSettingsData
        {
            version = 1,
            languageIndex = 0,
            resolutionWidth = resolution.width,
            resolutionHeight = resolution.height,
            fullscreen = true,
            vibration = false,
            masterVolume = 0f,
            musicVolume = 0f,
            vfVolume = 0f
        };
    }

    private static bool Normalize()
    {
        // 修正损坏或旧版本字段，返回 true 表示需要重新保存。
        bool changed = false;

        if (current.version <= 0)
        {
            current.version = 1;
            changed = true;
        }

        if (current.resolutionWidth <= 0 || current.resolutionHeight <= 0)
        {
            Resolution resolution = Screen.currentResolution;
            current.resolutionWidth = resolution.width;
            current.resolutionHeight = resolution.height;
            changed = true;
        }

        return changed;
    }
}
