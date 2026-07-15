using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

// 编辑器工具：把 Assets/Lua 打包成服务器可下载的 zip，并生成 lua_manifest.json。
// 这个工具只在 Unity Editor 里使用，不会进入最终运行时代码。
public static class HotfixLuaServerBuildUtility
{
    // Lua 源目录。这里的相对路径会原样保留到 zip 里，例如 hotfix/weapon/missile.lua。
    private const string SourcePath = "Assets/Lua";

    // 本地 HTTP 服务器根目录，和 start_addressables_server.py 使用同一个根。
    private const string ServerRoot = "D:/AddressablesServerRoot";

    // Lua 热更包输出目录。服务器访问时对应 http://127.0.0.1:18080/LuaRemote/。
    private const string LuaRemotePath = ServerRoot + "/LuaRemote";
    private const string LuaManifestName = "lua_manifest.json";
    private const string LuaManifestUrl = "http://127.0.0.1:18080/LuaRemote/lua_manifest.json";

    [MenuItem("Hotfix/Lua/Build HTTP Lua Hotfix Package")]
    public static void BuildHttpLuaHotfixPackage()
    {
        // 转成绝对路径后再打包，避免 Unity 当前工作目录变化导致路径不一致。
        string sourceFullPath = Path.GetFullPath(SourcePath);
        if (!Directory.Exists(sourceFullPath))
        {
            Debug.LogWarning($"[LuaHotfixBuild] Lua source path not found: {sourceFullPath}");
            return;
        }

        Directory.CreateDirectory(LuaRemotePath);

        // 版本号和 zip 文件名都使用当前时间生成。
        // 后续如果接正式版本系统，可以把这里改成外部输入的版本号。
        string version = DateTime.Now.ToString("yyyy.MM.dd.HHmmss");
        string packageName = $"lua_hotfix_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        string packagePath = Path.Combine(LuaRemotePath, packageName);

        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }

        CreateLuaZip(sourceFullPath, packagePath);
        byte[] zipBytes = File.ReadAllBytes(packagePath);

        // manifest 是客户端判断是否下载、下载后如何校验的唯一依据。
        LuaHotfixManifest manifest = new LuaHotfixManifest
        {
            version = version,
            package = packageName,
            sha256 = ComputeSha256(zipBytes),
            size = zipBytes.Length,
        };

        string manifestPath = Path.Combine(LuaRemotePath, LuaManifestName);
        File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));

        Debug.Log($"[LuaHotfixBuild] Lua hotfix package built: {packagePath}");
        Debug.Log($"[LuaHotfixBuild] Version: {manifest.version}");
        Debug.Log($"[LuaHotfixBuild] Manifest URL: {LuaManifestUrl}");
    }


    // 创建 Lua zip。
    // 只打包实际 Lua 文件和配置文件，忽略 Unity 自动生成的 .meta。
    private static void CreateLuaZip(string sourceRoot, string packagePath)
    {
        using (FileStream zipStream = File.Create(packagePath))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string relativePath = GetRelativePath(sourceRoot, file).Replace('\\', '/');
                ZipArchiveEntry entry = archive.CreateEntry(relativePath, System.IO.Compression.CompressionLevel.Optimal);
                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = File.OpenRead(file))
                {
                    // 手动流复制，避免依赖 ZipFile 扩展 API。
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }

    // 计算 file 相对 sourceRoot 的路径。
    // 没直接用 Path.GetRelativePath，是为了兼容 Unity 某些旧 API 设置。
    private static string GetRelativePath(string root, string file)
    {
        Uri rootUri = new Uri(AppendDirectorySeparatorChar(root));
        Uri fileUri = new Uri(file);
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString());
    }

    // Uri 计算相对路径时，目录路径必须以分隔符结尾。
    // Path.DirectorySeparatorChar表示当前操作系统常用的目录分隔符 Windows是"\"
    private static string AppendDirectorySeparatorChar(string path) 
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
            path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }

    // 给 zip 生成 SHA256，写入 manifest 供客户端校验。
    private static string ComputeSha256(byte[] bytes)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
