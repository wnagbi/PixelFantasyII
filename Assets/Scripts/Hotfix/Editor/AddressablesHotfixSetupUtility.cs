using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

// 编辑器专用的 Addressables HTTP 热更配置和构建工具。
public static class AddressablesHotfixSetupUtility
{
    // 所有玩法热更资源统一放入这个组，共用同一套远端 Build/Load Path。
    public const string GroupName = "RemoteGameplayAssets";

    // 运行时启动阶段会通过 AddressableResourceManager.DownloadDependencies 下载这个 label。
    public const string GameplayLabel = "gameplay";

    // 本地服务器根目录，用来模拟真实 CDN 或静态资源服务器。
    public const string ServerRoot = "D:/AddressablesServerRoot";

    // BuildPath 是 Unity 构建 Addressables 时写入 bundle/catalog 的目录。
    public const string RemoteBuildPath = ServerRoot + "/AddressablesRemote/[BuildTarget]";

    // LoadPath 是打包后的游戏运行时请求远端 Addressables 内容的地址。
    public const string RemoteLoadPath = "http://127.0.0.1:18080/AddressablesRemote/[BuildTarget]";

    // 快速测试地址，可以用浏览器确认本地 HTTP 服务器是否能访问 catalog hash。
    public const string TestUrl = "http://127.0.0.1:18080/AddressablesRemote/StandaloneWindows64/catalog_1.0.hash";

    // 资源路径 -> Addressable key。
    // 运行时 C# 和 Lua 使用右侧 key，不直接依赖工程资源路径。
    private static readonly Dictionary<string, string> GameplayAssets = new Dictionary<string, string>
    {
        { "Assets/Materials/items/i_magnet.png", "ui/skill/icon_magnet" },
        { "Assets/Materials/items/i_atkUP.png", "ui/skill/icon_rage" },
        { "Assets/Materials/items/i_slash.png", "ui/skill/icon_dimension_slash" },
        { "Assets/Prefabs/Particle/Slash Dimension.prefab", "vfx/skill/dimension_slash" },
        { "Assets/Prefabs/Weapon/MissileRange.prefab", "weapon/projectile/missile" },
        { "Assets/Prefabs/Weapon/Knife.prefab", "weapon/projectile/knife" },
        { "Assets/Prefabs/Weapon/Scyth.prefab", "weapon/projectile/scythe" },
        { "Assets/Prefabs/Weapon/i_sword.prefab", "weapon/projectile/sword" },
        { "Assets/Prefabs/Weapon/Funnel.prefab", "weapon/projectile/funnel" },
        { "Assets/Prefabs/Weapon/Laser.prefab", "weapon/projectile/funnel_laser" },
        { "Assets/Prefabs/Weapon/Tornado.prefab", "weapon/projectile/tornado" },
        { "Assets/Prefabs/Silm.prefab", "enemy/slim" },
        { "Assets/Prefabs/Exp.prefab", "pickup/exp" },
        { "Assets/Prefabs/Potion.prefab", "pickup/blood" },
    };

    [MenuItem("Hotfix/Addressables/Setup HTTP Remote Gameplay Assets")]
    public static void SetupHttpRemoteGameplayAssets()
    {
        // 保留旧菜单入口，同时把成功/失败逻辑交给 Try 方法，方便面板复用。
        TrySetupHttpRemoteGameplayAssets();
    }

    public static bool TrySetupHttpRemoteGameplayAssets()
    {
        try
        {
            // 写入 Profile 路径和构建内容前，先保证服务器根目录存在。
            Directory.CreateDirectory(ServerRoot);

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                // Addressables 包或配置不存在时，无法继续配置远端资源。
                Debug.LogError("[AddressablesHotfixSetup] Addressable settings not found.");
                return false;
            }

            // 必须开启远端 catalog，否则客户端无法检查远端资源是否更新。
            settings.BuildRemoteCatalog = true;

            // 让 catalog 路径引用当前 Profile 中的 Remote.BuildPath / Remote.LoadPath 变量。
            SetProfileReference(settings, settings.RemoteCatalogBuildPath, AddressableAssetSettings.kRemoteBuildPath);
            SetProfileReference(settings, settings.RemoteCatalogLoadPath, AddressableAssetSettings.kRemoteLoadPath);

            // 设置 Profile 变量的真实值。
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.BuildPath", RemoteBuildPath);
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", RemoteLoadPath);
            settings.AddLabel(GameplayLabel, false);

            // 如果组已经存在就复用，保证重复点击 Setup 时结果稳定。
            AddressableAssetGroup group = settings.FindGroup(GroupName);
            if (group == null)
            {
                group = settings.CreateGroup(
                    GroupName,
                    false,
                    false,
                    true,
                    null,
                    typeof(ContentUpdateGroupSchema),
                    typeof(BundledAssetGroupSchema)
                );
            }

            // StaticContent=false 表示这组资源允许后续做内容更新。
            ContentUpdateGroupSchema contentSchema = group.GetSchema<ContentUpdateGroupSchema>() ?? group.AddSchema<ContentUpdateGroupSchema>();
            contentSchema.StaticContent = false;

            // Bundled schema 控制 bundle 的构建路径和运行时加载路径。
            BundledAssetGroupSchema bundledSchema = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
            SetProfileReference(settings, bundledSchema.BuildPath, AddressableAssetSettings.kRemoteBuildPath);
            SetProfileReference(settings, bundledSchema.LoadPath, AddressableAssetSettings.kRemoteLoadPath);

            // PackTogether 让第一版演示更简单：玩法资源打成一组，而不是很多小 bundle。
            bundledSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;

            foreach (KeyValuePair<string, string> asset in GameplayAssets)
            {
                // 为目标资源创建或移动 Addressable entry，并设置运行时 key 和 gameplay label。
                AddOrMoveEntry(settings, group, asset.Key, asset.Value);
            }

            // 标记配置已修改，确保 Unity 会把 Addressables 设置写回磁盘。
            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(group);
            EditorUtility.SetDirty(contentSchema);
            EditorUtility.SetDirty(bundledSchema);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AddressablesHotfixSetup] Setup HTTP remote complete. Build path: {RemoteBuildPath}");
            Debug.Log($"[AddressablesHotfixSetup] Load path: {RemoteLoadPath}");
            Debug.Log($"[AddressablesHotfixSetup] Start server in {ServerRoot}: python D:/AddressablesServerRoot/start_addressables_server.py");
            Debug.Log($"[AddressablesHotfixSetup] Test URL: {TestUrl}");
            return true;
        }
        catch (Exception ex)
        {
            // 返回 false，让构建面板可以停止一键构建流程。
            Debug.LogError($"[AddressablesHotfixSetup] Setup failed: {ex.Message}");
            return false;
        }
    }

    [MenuItem("Hotfix/Addressables/Build HTTP Remote Content")]
    public static void BuildHttpRemoteContent()
    {
        // 菜单模式下构建前仍然先执行 Setup，避免 Profile 路径是旧配置。
        TryBuildHttpRemoteContent();
    }

    public static bool TryBuildHttpRemoteContent()
    {
        // 单步构建默认先重新应用 HTTP 远端配置，更安全。
        return TryBuildHttpRemoteContent(true);
    }

    public static bool TryBuildHttpRemoteContent(bool setupBeforeBuild)
    {
        try
        {
            // 一键构建已经在前一步执行过 Setup，所以可以传 false 避免重复配置。
            if (setupBeforeBuild && !TrySetupHttpRemoteGameplayAssets())
            {
                return false;
            }

            // Unity 会根据当前 Addressables 设置生成 catalog、hash 和 bundle。
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrEmpty(result.Error))
            {
                // Addressables 构建多数错误会通过 result.Error 返回，而不是抛异常。
                Debug.LogError($"[AddressablesHotfixSetup] Addressables build failed: {result.Error}");
                return false;
            }

            Debug.Log($"[AddressablesHotfixSetup] Addressables build complete. Output: {ServerRoot}/AddressablesRemote");
            Debug.Log($"[AddressablesHotfixSetup] Test URL: {TestUrl}");
            return true;
        }
        catch (Exception ex)
        {
            // 捕获意外的 Addressables 或 Editor 异常，让构建面板可以恢复。
            Debug.LogError($"[AddressablesHotfixSetup] Addressables build failed: {ex.Message}");
            return false;
        }
    }

    private static void AddOrMoveEntry(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath, string address)
    {
        // Addressables 通过 GUID 管理资源，因此先把工程路径解析成 GUID。
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid))
        {
            // 资源缺失只输出 Warning，不直接中断，方便部分演示资源不存在时仍可构建。
            Debug.LogWarning($"[AddressablesHotfixSetup] Asset not found: {assetPath}");
            return;
        }

        // CreateOrMoveEntry 是幂等操作：不存在就创建，已存在就移动到目标组。
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
        entry.address = address;
        entry.SetLabel(GameplayLabel, true, true, true);
    }

    private static void SetProfileReference(AddressableAssetSettings settings, ProfileValueReference reference, string variableName)
    {
        // Addressables schema 字段应引用 Profile 变量，而不是直接写死字符串。
        AddressableAssetProfileSettings.ProfileIdData profileData = settings.profileSettings.GetProfileDataByName(variableName);
        if (profileData == null)
        {
            // 正常 Addressables 项目一般不会缺这个变量；这里输出 Warning 方便定位配置问题。
            Debug.LogWarning($"[AddressablesHotfixSetup] Profile variable not found: {variableName}");
            return;
        }

        // 设置后，这个字段会跟随 Profile 变量值变化。
        reference.SetVariableById(settings, profileData.Id);
    }
}
