using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// 编辑器专用的 Lua 同步工具。
// 运行时 Lua 加载优先级仍然是 persistentDataPath/LuaHotfix，然后才回退到 StreamingAssets/Lua。
public static class HotfixLuaBuildUtility
{
    // Unity 工程中维护 Lua 源文件的目录。
    public const string SourcePath = "Assets/Lua";

    // StreamingAssets 会被 Unity 打进包体，因此这里作为包内基础 Lua 回退目录。
    public const string TargetPath = "Assets/StreamingAssets/Lua";

    [MenuItem("Hotfix/Sync Lua To StreamingAssets")]
    public static void SyncLuaToStreamingAssets()
    {
        // 保留旧菜单入口，实际成功/失败逻辑交给 Try 方法，方便面板复用。
        TrySyncLuaToStreamingAssets();
    }

    public static bool TrySyncLuaToStreamingAssets()
    {
        try
        {
            // 文件操作前先转成绝对路径，避免 Unity 当前工作目录变化带来路径问题。
            string sourceFullPath = Path.GetFullPath(SourcePath);
            string targetFullPath = Path.GetFullPath(TargetPath);

            if (!Directory.Exists(sourceFullPath))
            {
                // Lua 源目录不存在时没有可同步内容，返回 false 给构建面板停止流程。
                Debug.LogWarning($"[Hotfix] Lua source path not found: {sourceFullPath}");
                return false;
            }

            if (Directory.Exists(targetFullPath))
            {
                // 先清理旧目录，避免已经删除的 Lua 文件继续残留在 StreamingAssets。
                Directory.Delete(targetFullPath, true);
            }

            // 只复制运行时需要的 Lua 文件，然后刷新 Unity 资源数据库。
            CopyLuaDirectory(sourceFullPath, targetFullPath);
            AssetDatabase.Refresh();
            Debug.Log($"[Hotfix] Synced Lua files to {targetFullPath}");
            return true;
        }
        catch (Exception ex)
        {
            // 文件被占用、权限不足、路径异常都可能导致失败，这里统一报错并交给调用方停止流程。
            Debug.LogError($"[Hotfix] Sync Lua failed: {ex.Message}");
            return false;
        }
    }

    [MenuItem("Hotfix/Print Persistent LuaHotfix Path")]
    public static void PrintPersistentLuaHotfixPath()
    {
        // 打印运行时下载 Lua 热更后实际落盘的位置。
        LuaLoader.EnsureHotfixRoot();
        Debug.Log($"[Hotfix] LuaHotfix path: {LuaLoader.HotfixRoot}");
    }

    private static void CopyLuaDirectory(string source, string target)
    {
        // 先创建目标根目录，再按源目录结构复制子目录。
        Directory.CreateDirectory(target);

        foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, directory);
            Directory.CreateDirectory(Path.Combine(target, relative));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            {
                // .meta 是 Unity 编辑器元数据，运行时 Lua 加载不需要。
                continue;
            }

            // 保留相对路径，例如 Assets/Lua/hotfix/weapon/missile.lua -> hotfix/weapon/missile.lua。
            string relative = Path.GetRelativePath(source, file);
            string targetFile = Path.Combine(target, relative);
            string targetDirectory = Path.GetDirectoryName(targetFile);

            if (!string.IsNullOrEmpty(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(file, targetFile, true);
        }
    }
}

// Unity 打包前自动同步 Lua，确保包内基础 Lua 始终是最新版本。
public class HotfixLuaBuildPreprocessor : IPreprocessBuildWithReport
{
    // callbackOrder 越小越早执行，这里提前同步，保证后续构建步骤看到最新 StreamingAssets。
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        // 每次 Build Player 时都自动同步一次包内基础 Lua。
        HotfixLuaBuildUtility.SyncLuaToStreamingAssets();
    }
}
