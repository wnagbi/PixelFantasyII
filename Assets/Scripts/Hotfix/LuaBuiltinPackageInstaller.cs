using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

// Android 的 StreamingAssets 位于 APK 内部，不能通过 File API 直接读取。
// 本类在 xLua 初始化前读取包内 LuaBuiltin.zip，并解压到可同步读取的 persistentDataPath。
public static class LuaBuiltinPackageInstaller
{
    private const string PackageName = "LuaBuiltin.zip";
    private const string HashFileName = ".package_hash";

    public static readonly string BuiltinRoot = Path.Combine(Application.persistentDataPath, "LuaBuiltin");

    private static readonly string StagingRoot = Path.Combine(Application.persistentDataPath, "LuaBuiltinStaging");
    private static readonly string BackupRoot = Path.Combine(Application.persistentDataPath, "LuaBuiltinBackup");

    /// <summary>
    /// 准备当前平台可同步读取的包内基础 Lua。
    /// </summary>
    /// <remarks>
    /// 使用注意：必须在 LuaManager.Init() 前通过 yield return 执行；失败时只记录 Warning，由 C# fallback 继续启动。
    /// </remarks>
    public static IEnumerator Prepare()
    {
#if UNITY_ANDROID
        if (Application.isEditor)
        {
            yield break;
        }

        string packageUrl = CombineStreamingAssetsUrl(PackageName);
        using (UnityWebRequest request = UnityWebRequest.Get(packageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[LuaBuiltin] Failed to read package: {request.error}");
                yield break;
            }

            byte[] packageBytes = request.downloadHandler.data;
            if (packageBytes == null || packageBytes.Length == 0)
            {
                Debug.LogWarning("[LuaBuiltin] Package is empty.");
                yield break;
            }

            try
            {
                InstallIfChanged(packageBytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LuaBuiltin] Install failed: {ex.Message}");
            }
        }
#else
        // Windows 和 Unity Editor 可以直接读取 StreamingAssets/Lua，不需要额外提取。
        yield break;
#endif
    }

    /// <summary>
    /// 根据压缩包哈希判断是否需要重新安装，并以 staging 目录完成安全替换。
    /// </summary>
    /// <remarks>
    /// 使用注意：只有新包完整解压且包含 main.lua 时才会替换当前离线 Lua。
    /// </remarks>
    private static void InstallIfChanged(byte[] packageBytes)
    {
        string packageHash = ComputeSha256(packageBytes);
        string installedHashPath = Path.Combine(BuiltinRoot, HashFileName);

        if (File.Exists(Path.Combine(BuiltinRoot, "main.lua")) &&
            File.Exists(installedHashPath) &&
            string.Equals(File.ReadAllText(installedHashPath).Trim(), packageHash, StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[LuaBuiltin] Offline Lua package is ready.");
            return;
        }

        DeleteDirectoryIfExists(StagingRoot);
        Directory.CreateDirectory(StagingRoot);

        try
        {
            ExtractPackage(packageBytes, StagingRoot);

            if (!File.Exists(Path.Combine(StagingRoot, "main.lua")))
            {
                throw new InvalidDataException("LuaBuiltin.zip does not contain main.lua.");
            }

            File.WriteAllText(Path.Combine(StagingRoot, HashFileName), packageHash);
            ReplaceBuiltinRoot();
            Debug.Log($"[LuaBuiltin] Offline Lua installed: {BuiltinRoot}");
        }
        catch
        {
            DeleteDirectoryIfExists(StagingRoot);
            throw;
        }
    }

    /// <summary>
    /// 解压内置 Lua，并阻止压缩包条目写出目标目录。
    /// </summary>
    /// <remarks>
    /// 使用注意：所有条目都必须是相对路径；该检查用于避免 Zip Slip 覆盖任意本地文件。
    /// </remarks>
    private static void ExtractPackage(byte[] packageBytes, string destinationRoot)
    {
        string normalizedRoot = Path.GetFullPath(destinationRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        using (MemoryStream zipStream = new MemoryStream(packageBytes, false))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string destinationPath = Path.GetFullPath(Path.Combine(destinationRoot, entry.FullName));
                if (!destinationPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"Invalid Lua package entry: {entry.FullName}");
                }

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                using (Stream source = entry.Open())
                using (FileStream target = File.Create(destinationPath))
                {
                    source.CopyTo(target);
                }
            }
        }
    }

    /// <summary>
    /// 用已校验的 staging 目录替换当前离线 Lua，并在失败时恢复旧目录。
    /// </summary>
    /// <remarks>
    /// 使用注意：不要在 xLua 已经初始化后调用，否则已加载模块不会自动切换。
    /// </remarks>
    private static void ReplaceBuiltinRoot()
    {
        DeleteDirectoryIfExists(BackupRoot);

        bool hadCurrentRoot = Directory.Exists(BuiltinRoot);
        if (hadCurrentRoot)
        {
            Directory.Move(BuiltinRoot, BackupRoot);
        }

        try
        {
            Directory.Move(StagingRoot, BuiltinRoot);
            DeleteDirectoryIfExists(BackupRoot);
        }
        catch
        {
            DeleteDirectoryIfExists(BuiltinRoot);
            if (hadCurrentRoot && Directory.Exists(BackupRoot))
            {
                Directory.Move(BackupRoot, BuiltinRoot);
            }

            throw;
        }
    }

    /// <summary>
    /// 生成 StreamingAssets 中内置 Lua 压缩包的跨平台读取地址。
    /// </summary>
    /// <remarks>
    /// 使用注意：Android 返回 jar:file 地址，不能额外添加 file:// 前缀。
    /// </remarks>
    private static string CombineStreamingAssetsUrl(string fileName)
    {
        return $"{Application.streamingAssetsPath.TrimEnd('/', '\\')}/{fileName}";
    }

    /// <summary>
    /// 计算压缩包内容哈希，用于避免每次启动重复解压。
    /// </summary>
    /// <remarks>
    /// 使用注意：哈希只用于本地版本比较，不承担服务器签名校验。
    /// </remarks>
    private static string ComputeSha256(byte[] bytes)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return BitConverter.ToString(sha256.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    /// <summary>
    /// 在目录存在时递归删除，统一处理 staging 和 backup 清理。
    /// </summary>
    /// <remarks>
    /// 使用注意：只允许传入本类固定在 persistentDataPath 下的目录。
    /// </remarks>
    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
