using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

// 编辑器专用的 Lua HTTP 热更包构建工具。
// 它会把 Assets/Lua 打成 zip，输出到本地服务器目录，并生成 lua_manifest.json。
public static class HotfixLuaServerBuildUtility
{
    // 会被打进可下载 zip 的 Lua 源目录。
    public const string SourcePath = "Assets/Lua";

    // 本地 HTTP 服务器根目录，Python 服务器会从这个目录提供静态文件。
    public const string ServerRoot = "D:/AddressablesServerRoot";

    // LuaRemote 目录对应运行时访问地址 http://127.0.0.1:18080/LuaRemote/。
    public const string LuaRemotePath = ServerRoot + "/LuaRemote";

    // 客户端启动时会先下载这个 manifest，用它判断是否需要更新 Lua。
    public const string LuaManifestName = "lua_manifest.json";
    public const string LuaManifestUrl = "http://127.0.0.1:18080/LuaRemote/lua_manifest.json";

    /// <summary>
    /// 构建 HotfixLuaServerBuildUtility 中与 BuildHttpLuaHotfixPackage 对应的输出内容。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    [MenuItem("Hotfix/Lua/Build HTTP Lua Hotfix Package")]
    public static void BuildHttpLuaHotfixPackage()
    {
        // 菜单入口保持简单，实际逻辑放在 Try 方法中，方便构建面板复用。
        TryBuildHttpLuaHotfixPackage();
    }

    /// <summary>
    /// 尝试执行 TryBuildHttpLuaHotfixPackage，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool TryBuildHttpLuaHotfixPackage()
    {
        try
        {
            // 使用绝对路径，避免 Unity 进程工作目录变化影响打包。
            string sourceFullPath = Path.GetFullPath(SourcePath);
            if (!Directory.Exists(sourceFullPath))
            {
                // 源目录不存在时返回 false，让一键构建在生成错误 manifest 前停止。
                Debug.LogWarning($"[LuaHotfixBuild] Lua source path not found: {sourceFullPath}");
                return false;
            }

            // 确保输出目录存在，这样 HTTP 服务器才能访问 zip 和 manifest。
            Directory.CreateDirectory(LuaRemotePath);

            // 第一版用时间戳作为版本号，足够用于本地热更新测试。
            // 正式项目通常会从发布工具或版本管理系统传入版本号。
            DateTime now = DateTime.Now;
            string version = now.ToString("yyyy.MM.dd.HHmmss");
            string packageName = $"lua_hotfix_{now:yyyyMMdd_HHmmss}.zip";
            string packagePath = Path.Combine(LuaRemotePath, packageName);

            if (File.Exists(packagePath))
            {
                // 文件名包含秒，一般不会重复；这里保留删除逻辑，让重复调用行为更确定。
                File.Delete(packagePath);
            }

            // 先生成 zip，再读取最终文件字节，确保 size 和 sha256 对应真实落盘文件。
            CreateLuaZip(sourceFullPath, packagePath);
            byte[] zipBytes = File.ReadAllBytes(packagePath);

            // manifest 是和运行时 LuaHotfixRemoteUpdater 的约定。
            // 客户端会检查 version、size、sha256，然后才替换本地 LuaHotfix。
            LuaHotfixManifest manifest = new LuaHotfixManifest
            {
                version = version,
                package = packageName,
                sha256 = ComputeSha256(zipBytes),
                size = zipBytes.Length,
            };

            // 使用 JsonUtility，保持 manifest 格式和运行时解析方式一致。
            string manifestPath = Path.Combine(LuaRemotePath, LuaManifestName);
            File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));

            Debug.Log($"[LuaHotfixBuild] Lua hotfix package built: {packagePath}");
            Debug.Log($"[LuaHotfixBuild] Version: {manifest.version}");
            Debug.Log($"[LuaHotfixBuild] Manifest URL: {LuaManifestUrl}");
            return true;
        }
        catch (Exception ex)
        {
            // 常见失败原因包括文件被占用、路径无效、权限不足等。
            Debug.LogError($"[LuaHotfixBuild] Build failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 手动创建 zip，方便跳过 Unity .meta 文件，并保持 Lua 相对路径干净。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixLuaServerBuildUtility 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static void CreateLuaZip(string sourceRoot, string packagePath)
    {
        // 手动创建 zip，方便跳过 Unity .meta 文件，并保持 Lua 相对路径干净。
        using (FileStream zipStream = File.Create(packagePath))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    // .meta 不能作为运行时热更内容下载。
                    continue;
                }

                // zip 根目录就是 Assets/Lua，因此 main.lua 在压缩包内路径仍然是 main.lua。
                string relativePath = GetRelativePath(sourceRoot, file).Replace('\\', '/');
                ZipArchiveEntry entry = archive.CreateEntry(relativePath, System.IO.Compression.CompressionLevel.Optimal);
                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = File.OpenRead(file))
                {
                    // 使用流复制，避免额外把每个 Lua 文件单独完整读入内存。
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }

    /// <summary>
    /// 使用 Uri 计算相对路径，兼容一些旧 Unity/.NET API 环境。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixLuaServerBuildUtility 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static string GetRelativePath(string root, string file)
    {
        // 使用 Uri 计算相对路径，兼容一些旧 Unity/.NET API 环境。
        Uri rootUri = new Uri(AppendDirectorySeparatorChar(root));
        Uri fileUri = new Uri(file);
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString());
    }

    /// <summary>
    /// Uri.MakeRelativeUri 要求目录路径以分隔符结尾，否则可能按文件路径处理。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixLuaServerBuildUtility 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static string AppendDirectorySeparatorChar(string path)
    {
        // Uri.MakeRelativeUri 要求目录路径以分隔符结尾，否则可能按文件路径处理。
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
            path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// 运行时更新器会重新计算这个 hash，用来拒绝损坏或被篡改的 zip 包。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixLuaServerBuildUtility 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static string ComputeSha256(byte[] bytes)
    {
        // 运行时更新器会重新计算这个 hash，用来拒绝损坏或被篡改的 zip 包。
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
