using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

// 服务器上的 lua_manifest.json 会反序列化成这个结构。
// 客户端根据它判断版本是否变化、要下载哪个 zip、下载后是否完整。
public sealed class LuaHotfixManifest
{
    public string version;
    public string package;
    public string sha256;
    public long size;
}

// 运行时 Lua 热更新入口。
// 它只负责把服务器上的 Lua zip 安全落盘到 persistentDataPath/LuaHotfix。
public static class LuaHotfixRemoteUpdater
{
    // Manifest 只保存版本和包元数据，Zip 文件从同一远端目录下载。
    private const string ManifestUrl = "http://127.0.0.1:18080/LuaRemote/lua_manifest.json";
    private const string RemoteRootUrl = "http://127.0.0.1:18080/LuaRemote/";
    private const int RequestTimeoutSeconds = 10;

    // 下载、解压和备份使用独立目录，只有完整校验通过后才替换正式 LuaHotfix。
    private static readonly string DownloadRoot = Path.Combine(Application.persistentDataPath, "LuaHotfixDownload");
    private static readonly string StagingRoot = Path.Combine(Application.persistentDataPath, "LuaHotfixStaging");
    private static readonly string BackupRoot = Path.Combine(Application.persistentDataPath, "LuaHotfixBackup");

    /// <summary>
    /// 检查并处理 LuaHotfixRemoteUpdater 中与 CheckAndApply 对应的条件。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static IEnumerator CheckAndApply()
    {
        // 兼容不需要加载界面进度的调用方。
        yield return CheckAndApply(null);
    }

    /// <summary>
    /// 检查并处理 LuaHotfixRemoteUpdater 中与 CheckAndApply 对应的条件。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static IEnumerator CheckAndApply(Action<float, string> onProgress)
    {
        // 阶段一：请求远端 manifest 并验证基本字段。
        Report(onProgress, 0f, "检查脚本更新...");
        LuaLoader.EnsureHotfixRoot();

        UnityWebRequest manifestRequest = UnityWebRequest.Get(ManifestUrl);
        manifestRequest.timeout = RequestTimeoutSeconds;

        UnityWebRequestAsyncOperation manifestOperation = manifestRequest.SendWebRequest();
        while (!manifestOperation.isDone)
        {
            Report(onProgress, Mathf.Lerp(0f, 0.25f, manifestOperation.progress), "请求脚本版本...");
            yield return null;
        }

        if (manifestRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[LuaHotfixRemote] Manifest request failed: {manifestRequest.error}");
            manifestRequest.Dispose();
            Report(onProgress, 1f, "脚本服务器不可用，使用本地脚本...");
            yield break;
        }

        string manifestJson = manifestRequest.downloadHandler.text;
        manifestRequest.Dispose();

        Report(onProgress, 0.30f, "对比脚本版本...");

        LuaHotfixManifest remoteManifest = ParseManifest(manifestJson, "remote");
        if (!IsValidManifest(remoteManifest))
        {
            Debug.LogWarning("[LuaHotfixRemote] Remote manifest is invalid.");
            Report(onProgress, 1f, "脚本版本信息无效，使用本地脚本...");
            yield break;
        }

        LuaHotfixManifest localManifest = LoadLocalManifest();
        // 版本号相同则继续使用本地 LuaHotfix，不重复下载。
        if (localManifest != null && string.Equals(localManifest.version, remoteManifest.version, StringComparison.Ordinal))
        {
            Debug.Log($"[LuaHotfixRemote] Lua hotfix is up to date: {remoteManifest.version}");
            Report(onProgress, 1f, "脚本已是最新版本...");
            yield break;
        }

        Debug.Log($"[LuaHotfixRemote] Downloading Lua hotfix: {remoteManifest.version}");
        Report(onProgress, 0.60f, "下载脚本更新...");

        string packageUrl = RemoteRootUrl + remoteManifest.package;
        // 阶段二：下载完整 Zip；第一版不做差分包。
        UnityWebRequest packageRequest = UnityWebRequest.Get(packageUrl);
        packageRequest.timeout = RequestTimeoutSeconds;

        UnityWebRequestAsyncOperation packageOperation = packageRequest.SendWebRequest();
        while (!packageOperation.isDone)
        {
            Report(onProgress, Mathf.Lerp(0.60f, 0.80f, Mathf.Clamp01(packageRequest.downloadProgress)), "下载脚本更新...");
            yield return null;
        }

        if (packageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[LuaHotfixRemote] Package request failed: {packageRequest.error}");
            packageRequest.Dispose();
            Report(onProgress, 1f, "脚本下载失败，使用本地脚本...");
            yield break;
        }

        byte[] zipBytes = packageRequest.downloadHandler.data;
        packageRequest.Dispose();

        Report(onProgress, 0.80f, "校验并解压脚本...");

        if (!VerifyPackage(zipBytes, remoteManifest))
        {
            Report(onProgress, 1f, "脚本校验失败，使用本地脚本...");
            yield break;
        }

        try
        {
            // 阶段三：校验通过后解压到 staging，再安全替换正式目录。
            ApplyPackage(zipBytes, remoteManifest);
            Debug.Log($"[LuaHotfixRemote] Lua hotfix applied: {remoteManifest.version}");
            Report(onProgress, 1f, "脚本更新完成...");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaHotfixRemote] Apply package failed: {ex.Message}");
            Report(onProgress, 1f, "脚本应用失败，使用本地脚本...");
        }
    }

    /// <summary>
    /// 加载 LuaHotfixRemoteUpdater 中与 LoadLocalManifest 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static LuaHotfixManifest LoadLocalManifest()
    {
        // version.json 和实际生效 Lua 位于同一目录，用于下次启动版本对比。
        string path = Path.Combine(LuaLoader.HotfixRoot, "version.json");
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return ParseManifest(File.ReadAllText(path), "local");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaHotfixRemote] Local manifest read failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 反序列化并校验服务器返回的 Lua 热更新 manifest。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static LuaHotfixManifest ParseManifest(string json, string source)
    {
        try
        {
            return JsonUtility.FromJson<LuaHotfixManifest>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaHotfixRemote] Failed to parse {source} manifest: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 判断 LuaHotfixRemoteUpdater 当前是否满足 IsValidManifest 对应的状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static bool IsValidManifest(LuaHotfixManifest manifest)
    {
        // 缺少任意关键字段都不能继续下载，避免拼出非法 URL 或接受空包。
        return manifest != null
            && !string.IsNullOrWhiteSpace(manifest.version)
            && !string.IsNullOrWhiteSpace(manifest.package)
            && !string.IsNullOrWhiteSpace(manifest.sha256)
            && manifest.size > 0;
    }

    /// <summary>
    /// 先校验长度，再计算 SHA256；两项都匹配才允许解压。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static bool VerifyPackage(byte[] zipBytes, LuaHotfixManifest manifest)
    {
        // 先校验长度，再计算 SHA256；两项都匹配才允许解压。
        if (zipBytes == null || zipBytes.Length == 0)
        {
            Debug.LogWarning("[LuaHotfixRemote] Package is empty.");
            return false;
        }

        if (zipBytes.Length != manifest.size)
        {
            Debug.LogWarning($"[LuaHotfixRemote] Package size mismatch. remote={manifest.size}, local={zipBytes.Length}");
            return false;
        }

        string sha256 = ComputeSha256(zipBytes);
        if (!string.Equals(sha256, manifest.sha256, StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[LuaHotfixRemote] Package sha256 mismatch. remote={manifest.sha256}, local={sha256}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 每次更新先清理上次中断留下的临时目录。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static void ApplyPackage(byte[] zipBytes, LuaHotfixManifest manifest)
    {
        // 每次更新先清理上次中断留下的临时目录。
        SafeDeleteDirectory(DownloadRoot);
        SafeDeleteDirectory(StagingRoot);
        Directory.CreateDirectory(DownloadRoot);
        Directory.CreateDirectory(StagingRoot);

        string zipPath = Path.Combine(DownloadRoot, manifest.package);
        File.WriteAllBytes(zipPath, zipBytes);
        ExtractZip(zipPath, StagingRoot);

        if (!File.Exists(Path.Combine(StagingRoot, "main.lua")))
        {
            // main.lua 是 LuaManager 的固定入口，缺失说明包结构无效。
            throw new FileNotFoundException("main.lua not found in Lua hotfix package.");
        }

        string versionPath = Path.Combine(StagingRoot, "version.json");
        File.WriteAllText(versionPath, JsonUtility.ToJson(manifest, true));
        ReplaceHotfixDirectory();
        SafeDeleteDirectory(DownloadRoot);
    }

    /// <summary>
    /// 每个 Entry 都转换为绝对路径并验证仍在 staging 内，阻止 Zip Slip 路径穿越。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static void ExtractZip(string zipPath, string targetRoot)
    {
        // 每个 Entry 都转换为绝对路径并验证仍在 staging 内，阻止 Zip Slip 路径穿越。
        string targetFullRoot = Path.GetFullPath(targetRoot);
        using (FileStream zipStream = File.OpenRead(zipPath))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string destinationPath = Path.GetFullPath(Path.Combine(targetFullRoot, entry.FullName));
                if (!IsPathInsideRoot(destinationPath, targetFullRoot))
                {
                    throw new InvalidDataException($"Unsafe zip entry path: {entry.FullName}");
                }

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                string directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = File.Create(destinationPath))
                {
                    entryStream.CopyTo(fileStream);
                }
            }
        }
    }

    /// <summary>
    /// 判断 LuaHotfixRemoteUpdater 当前是否满足 IsPathInsideRoot 对应的状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static bool IsPathInsideRoot(string path, string root)
    {
        string normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 先备份旧版本；新目录移动失败时恢复备份，保证至少有一套可用 Lua。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static void ReplaceHotfixDirectory()
    {
        // 先备份旧版本；新目录移动失败时恢复备份，保证至少有一套可用 Lua。
        SafeDeleteDirectory(BackupRoot);

        bool hadOldHotfix = Directory.Exists(LuaLoader.HotfixRoot);
        if (hadOldHotfix)
        {
            Directory.Move(LuaLoader.HotfixRoot, BackupRoot);
        }

        try
        {
            Directory.Move(StagingRoot, LuaLoader.HotfixRoot);
        }
        catch
        {
            // 替换失败但旧版本仍存在时立即回滚。
            if (hadOldHotfix && Directory.Exists(BackupRoot) && !Directory.Exists(LuaLoader.HotfixRoot))
            {
                Directory.Move(BackupRoot, LuaLoader.HotfixRoot);
            }

            throw;
        }
    }

    /// <summary>
    /// 输出小写十六进制字符串，与 Editor 构建工具生成的 manifest 格式一致。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static string ComputeSha256(byte[] bytes)
    {
        // 输出小写十六进制字符串，与 Editor 构建工具生成的 manifest 格式一致。
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    /// <summary>
    /// 只接收本类预先计算的三个明确目录，不处理外部传入路径。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static void SafeDeleteDirectory(string path)
    {
        // 只接收本类预先计算的三个明确目录，不处理外部传入路径。
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    /// <summary>
    /// 向调用方报告 LuaHotfixRemoteUpdater 当前流程的状态或进度。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaHotfixRemoteUpdater 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static void Report(Action<float, string> onProgress, float progress, string status)
    {
        // 没有加载 UI 时 onProgress 为 null，更新流程仍可正常执行。
        onProgress?.Invoke(Mathf.Clamp01(progress), status);
    }
}
