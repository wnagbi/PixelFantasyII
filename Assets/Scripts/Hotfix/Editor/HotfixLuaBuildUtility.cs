using System;
using System.IO;
using System.IO.Compression;
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

    // Android 不能使用 File API 直接读取 APK 内的散文件，因此同时生成一个可通过 UnityWebRequest 读取的 zip。
    public const string BuiltinPackagePath = "Assets/StreamingAssets/LuaBuiltin.zip";

    /// <summary>
    /// 尝试执行 TrySyncLuaToStreamingAssets，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
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
            CreateBuiltinPackage(sourceFullPath, Path.GetFullPath(BuiltinPackagePath));
            AssetDatabase.Refresh();
            Debug.Log($"[Hotfix] Synced Lua files to {targetFullPath}");
            Debug.Log($"[Hotfix] Built offline Lua package: {Path.GetFullPath(BuiltinPackagePath)}");
            return true;
        }
        catch (Exception ex)
        {
            // 文件被占用、权限不足、路径异常都可能导致失败，这里统一报错并交给调用方停止流程。
            Debug.LogError($"[Hotfix] Sync Lua failed: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// 先创建目标根目录，再按源目录结构复制子目录。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixLuaBuildUtility 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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

    /// <summary>
    /// 把全部基础 Lua 打成供 Android 首包读取的离线 zip。
    /// </summary>
    /// <remarks>
    /// 使用注意：压缩包根目录必须直接包含 main.lua；不要把 Assets/Lua 目录本身再包一层。
    /// </remarks>
    private static void CreateBuiltinPackage(string sourceRoot, string packagePath)
    {
        string packageDirectory = Path.GetDirectoryName(packagePath);
        if (!string.IsNullOrEmpty(packageDirectory))
        {
            Directory.CreateDirectory(packageDirectory);
        }

        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }

        using (FileStream zipStream = File.Create(packagePath))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string relativePath = Path.GetRelativePath(sourceRoot, file).Replace('\\', '/');
                ZipArchiveEntry entry = archive.CreateEntry(
                    relativePath,
                    System.IO.Compression.CompressionLevel.Optimal);
                using (Stream entryStream = entry.Open())
                using (FileStream sourceStream = File.OpenRead(file))
                {
                    sourceStream.CopyTo(entryStream);
                }
            }
        }
    }
}

// Unity 打包前自动同步 Lua，确保包内基础 Lua 始终是最新版本。
public class HotfixLuaBuildPreprocessor : IPreprocessBuildWithReport
{
    // callbackOrder 越小越早执行，这里提前同步，保证后续构建步骤看到最新 StreamingAssets。
    public int callbackOrder => -1000;

    /// <summary>
    /// 每次 Build Player 时都自动同步一次包内基础 Lua。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixLuaBuildPreprocessor 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void OnPreprocessBuild(BuildReport report)
    {
        // 每次 Build Player 时都自动同步一次包内基础 Lua。
        HotfixLuaBuildUtility.TrySyncLuaToStreamingAssets();
    }
}
