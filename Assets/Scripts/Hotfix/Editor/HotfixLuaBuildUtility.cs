using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// 编辑器专用的 Lua 热更构建辅助。
// 作用是把开发目录 Assets/Lua 同步到包内目录 Assets/StreamingAssets/Lua，
// 这样打包后的游戏即使没有 LuaHotfix 覆盖文件，也能读取到一份基础 Lua。
public static class HotfixLuaBuildUtility
{
    // 开发期维护 Lua 的目录。
    private const string SourcePath = "Assets/Lua";

    // 打包时会被 Unity 原样带进包体的目录。
    private const string TargetPath = "Assets/StreamingAssets/Lua";

    [MenuItem("Hotfix/Sync Lua To StreamingAssets")]
    public static void SyncLuaToStreamingAssets()
    {
        // 菜单手动同步入口：Hotfix/Sync Lua To StreamingAssets。
        string sourceFullPath = Path.GetFullPath(SourcePath);
        string targetFullPath = Path.GetFullPath(TargetPath);

        if (!Directory.Exists(sourceFullPath))
        {
            Debug.LogWarning($"[Hotfix] Lua source path not found: {sourceFullPath}");
            return;
        }

        if (Directory.Exists(targetFullPath))
        {
            // 先清空旧目录，避免已经删除的 Lua 文件继续残留在 StreamingAssets。
            Directory.Delete(targetFullPath, true);
        }

        CopyLuaDirectory(sourceFullPath, targetFullPath);
        AssetDatabase.Refresh();
        Debug.Log($"[Hotfix] Synced Lua files to {targetFullPath}");
    }

    [MenuItem("Hotfix/Print Persistent LuaHotfix Path")]
    public static void PrintPersistentLuaHotfixPath()
    {
        // 菜单打印运行时热更目录，方便你知道打包后要把 LuaHotfix 文件放到哪里。
        LuaLoader.EnsureHotfixRoot();
        Debug.Log($"[Hotfix] LuaHotfix path: {LuaLoader.HotfixRoot}");
    }

    private static void CopyLuaDirectory(string source, string target)
    {
        // 保持 Assets/Lua 下的相对路径不变复制到 StreamingAssets/Lua。
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
                // .meta 是 Unity 编辑器资源管理文件，运行时 Lua 加载不需要它。
                continue;
            }

            string relative = Path.GetRelativePath(source, file);
            string targetFile = Path.Combine(target, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
            File.Copy(file, targetFile, true);
        }
    }
}

// Unity 构建前回调。
// 只要执行 Build，都会自动同步 Lua，避免忘记手动点菜单导致包内 Lua 不是最新版。
public class HotfixLuaBuildPreprocessor : IPreprocessBuildWithReport
{
    // 越小越早执行。这里提前执行，确保其它构建步骤看到的 StreamingAssets 已经是最新 Lua。
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        // 打包前自动执行一次菜单同步逻辑。
        HotfixLuaBuildUtility.SyncLuaToStreamingAssets();
    }
}
