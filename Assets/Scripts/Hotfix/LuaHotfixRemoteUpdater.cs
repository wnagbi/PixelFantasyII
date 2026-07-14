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
    private const string ManifestUrl = "http://127.0.0.1:18080/LuaRemote/lua_manifest.json";
    private const string RemoteRootUrl = "http://127.0.0.1:18080/LuaRemote/";
    private const int RequestTimeoutSeconds = 10;

    private static readonly string DownloadRoot = Path.Combine(Application.persistentDataPath, "LuaHotfixDownload");
    private static readonly string StagingRoot = Path.Combine(Application.persistentDataPath, "LuaHotfixStaging");
    private static readonly string BackupRoot = Path.Combine(Application.persistentDataPath, "LuaHotfixBackup");

    public static IEnumerator CheckAndApply()
    {
        yield return CheckAndApply(null);
    }

    public static IEnumerator CheckAndApply(Action<float, string> onProgress)
    {
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
        if (localManifest != null && string.Equals(localManifest.version, remoteManifest.version, StringComparison.Ordinal))
        {
            Debug.Log($"[LuaHotfixRemote] Lua hotfix is up to date: {remoteManifest.version}");
            Report(onProgress, 1f, "脚本已是最新版本...");
            yield break;
        }

        Debug.Log($"[LuaHotfixRemote] Downloading Lua hotfix: {remoteManifest.version}");
        Report(onProgress, 0.60f, "下载脚本更新...");

        string packageUrl = RemoteRootUrl + remoteManifest.package;
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

    private static LuaHotfixManifest LoadLocalManifest()
    {
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

    private static bool IsValidManifest(LuaHotfixManifest manifest)
    {
        return manifest != null
            && !string.IsNullOrWhiteSpace(manifest.version)
            && !string.IsNullOrWhiteSpace(manifest.package)
            && !string.IsNullOrWhiteSpace(manifest.sha256)
            && manifest.size > 0;
    }

    private static bool VerifyPackage(byte[] zipBytes, LuaHotfixManifest manifest)
    {
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

    private static void ApplyPackage(byte[] zipBytes, LuaHotfixManifest manifest)
    {
        SafeDeleteDirectory(DownloadRoot);
        SafeDeleteDirectory(StagingRoot);
        Directory.CreateDirectory(DownloadRoot);
        Directory.CreateDirectory(StagingRoot);

        string zipPath = Path.Combine(DownloadRoot, manifest.package);
        File.WriteAllBytes(zipPath, zipBytes);
        ExtractZip(zipPath, StagingRoot);

        if (!File.Exists(Path.Combine(StagingRoot, "main.lua")))
        {
            throw new FileNotFoundException("main.lua not found in Lua hotfix package.");
        }

        string versionPath = Path.Combine(StagingRoot, "version.json");
        File.WriteAllText(versionPath, JsonUtility.ToJson(manifest, true));
        ReplaceHotfixDirectory();
        SafeDeleteDirectory(DownloadRoot);
    }

    private static void ExtractZip(string zipPath, string targetRoot)
    {
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

    private static bool IsPathInsideRoot(string path, string root)
    {
        string normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static void ReplaceHotfixDirectory()
    {
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
            if (hadOldHotfix && Directory.Exists(BackupRoot) && !Directory.Exists(LuaLoader.HotfixRoot))
            {
                Directory.Move(BackupRoot, LuaLoader.HotfixRoot);
            }

            throw;
        }
    }

    private static string ComputeSha256(byte[] bytes)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    private static void SafeDeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private static void Report(Action<float, string> onProgress, float progress, string status)
    {
        onProgress?.Invoke(Mathf.Clamp01(progress), status);
    }
}
