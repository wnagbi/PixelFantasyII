using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressablesHotfixSetupUtility
{
    private const string GroupName = "RemoteGameplayAssets";
    private const string GameplayLabel = "gameplay";
    // 本机静态服务器根目录。这个目录模拟真实商业项目里的 CDN/对象存储根目录。
    private const string ServerRoot = "D:/AddressablesServerRoot";
    // Addressables 构建输出目录：Build 后 catalog/hash/bundle 会写到这里。
    private const string RemoteBuildPath = ServerRoot + "/AddressablesRemote/[BuildTarget]";
    // 客户端运行时加载地址：打包后的游戏会通过 HTTP 请求这个地址。
    private const string RemoteLoadPath = "http://127.0.0.1:18080/AddressablesRemote/[BuildTarget]";
    private const string TestUrl = "http://127.0.0.1:18080/AddressablesRemote/StandaloneWindows64/catalog_1.0.hash";

    // 第一批接入热更的玩法资源。左边是项目资源路径，右边是运行时使用的 Addressable key。
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
        // 保证服务器根目录存在，避免第一次构建时输出目录找不到。
        Directory.CreateDirectory(ServerRoot);

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[AddressablesHotfixSetup] Addressable settings not found.");
            return;
        }

        // 开启远端 catalog。客户端启动时会用远端 hash 判断是否有资源更新。
        settings.BuildRemoteCatalog = true;
        SetProfileReference(settings, settings.RemoteCatalogBuildPath, AddressableAssetSettings.kRemoteBuildPath);
        SetProfileReference(settings, settings.RemoteCatalogLoadPath, AddressableAssetSettings.kRemoteLoadPath);
        settings.profileSettings.SetValue(settings.activeProfileId, "Remote.BuildPath", RemoteBuildPath);
        settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", RemoteLoadPath);
        settings.AddLabel(GameplayLabel, false);

        // 远端玩法资源统一放入 RemoteGameplayAssets 组，方便后续扩展武器、敌人、掉落物。
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

        // 这组资源使用远端 Build/Load Path，生成后由本地 HTTP 服务器提供下载。
        BundledAssetGroupSchema bundledSchema = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
        SetProfileReference(settings, bundledSchema.BuildPath, AddressableAssetSettings.kRemoteBuildPath);
        SetProfileReference(settings, bundledSchema.LoadPath, AddressableAssetSettings.kRemoteLoadPath);
        bundledSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;

        // 把目标资源移动/创建为 Addressable entry，并统一打上 gameplay label。
        foreach (KeyValuePair<string, string> asset in GameplayAssets)
        {
            AddOrMoveEntry(settings, group, asset.Key, asset.Value);
        }

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
    }

    [MenuItem("Hotfix/Addressables/Print HTTP Remote Path")]
    public static void PrintHttpRemotePath()
    {
        Debug.Log($"[AddressablesHotfixSetup] Server root: {ServerRoot}");
        Debug.Log($"[AddressablesHotfixSetup] Remote build path: {RemoteBuildPath}");
        Debug.Log($"[AddressablesHotfixSetup] Remote load path: {RemoteLoadPath}");
        Debug.Log($"[AddressablesHotfixSetup] Test URL: {TestUrl}");
    }

    [MenuItem("Hotfix/Addressables/Build HTTP Remote Content")]
    public static void BuildHttpRemoteContent()
    {
        // 构建前先重新应用配置，避免手动改 Profile 后输出到旧目录。
        SetupHttpRemoteGameplayAssets();

        // 使用 Addressables 默认构建脚本生成 catalog/hash/bundle。
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"[AddressablesHotfixSetup] Addressables build failed: {result.Error}");
            return;
        }

        Debug.Log($"[AddressablesHotfixSetup] Addressables build complete. Output: {ServerRoot}/AddressablesRemote");
        Debug.Log($"[AddressablesHotfixSetup] Test URL: {TestUrl}");
    }

    private static void AddOrMoveEntry(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath, string address)
    {
        // 通过 GUID 绑定资源，避免资源重命名时路径以外的信息丢失。
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning($"[AddressablesHotfixSetup] Asset not found: {assetPath}");
            return;
        }

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
        entry.address = address;
        entry.SetLabel(GameplayLabel, true, true, true);
    }

    private static void SetProfileReference(AddressableAssetSettings settings, ProfileValueReference reference, string variableName)
    {
        // Addressables 的 BuildPath/LoadPath 字段引用 Profile 变量，而不是直接写死字符串。
        AddressableAssetProfileSettings.ProfileIdData profileData = settings.profileSettings.GetProfileDataByName(variableName);
        if (profileData == null)
        {
            Debug.LogWarning($"[AddressablesHotfixSetup] Profile variable not found: {variableName}");
            return;
        }

        reference.SetVariableById(settings, profileData.Id);
    }
}
