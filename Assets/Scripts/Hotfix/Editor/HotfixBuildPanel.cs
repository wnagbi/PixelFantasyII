using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// 热更新构建面板的统一入口。
// 这个窗口只负责“编排流程”，真正的 Lua 打包和 Addressables 构建仍然交给对应工具类处理。
public sealed class HotfixBuildPanel : EditorWindow
{
    // 面板中展示、构建完成后打印到 Console 的测试地址。
    // 这些地址需要和本地 Python 热更新服务器保持一致。
    private const string LuaManifestUrl = HotfixLuaServerBuildUtility.LuaManifestUrl;
    private const string AddressablesCatalogUrl = AddressablesHotfixSetupUtility.TestUrl;
    private const string AddressablesRemotePath = AddressablesHotfixSetupUtility.ServerRoot + "/AddressablesRemote";

    // 构建过程中禁用按钮，避免同一个流程被重复触发。
    private bool isBuilding;

    // 这些状态只保存在当前 EditorWindow 内存中，不写入磁盘或项目设置。
    private string lastBuildStatus = "Idle";
    private string lastBuildTime = "-";
    private string lastError = "-";

    [MenuItem("Hotfix/Build Panel")]
    public static void Open()
    {
        // GetWindow 会在窗口不存在时创建窗口，已经存在时则聚焦到原窗口。
        HotfixBuildPanel window = GetWindow<HotfixBuildPanel>("Hotfix Build");
        window.minSize = new Vector2(460f, 360f);
        window.Show();
    }

    private void OnGUI()
    {
        // Unity 每次重绘 EditorWindow 时都会调用 OnGUI。
        // 这里只做布局分发，具体按钮逻辑拆到下面的方法里，方便阅读。
        EditorGUILayout.LabelField("Hotfix Build Panel", EditorStyles.boldLabel);
        EditorGUILayout.Space(6f);

        DrawConfig();
        EditorGUILayout.Space(8f);
        DrawSingleStepButtons();
        EditorGUILayout.Space(8f);
        DrawBuildAllButton();
        EditorGUILayout.Space(8f);
        DrawUtilityButtons();
        EditorGUILayout.Space(8f);


    }

    private void DrawConfig()
    {
        // SelectableLabel 可以让你直接从面板复制路径和 URL。
        EditorGUILayout.LabelField("Fixed Config", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel($"Server Root: {AddressablesHotfixSetupUtility.ServerRoot}", EditorStyles.textField, GUILayout.Height(18f));
        EditorGUILayout.SelectableLabel($"Lua Manifest: {LuaManifestUrl}", EditorStyles.textField, GUILayout.Height(18f));
        EditorGUILayout.SelectableLabel($"Addressables Catalog: {AddressablesCatalogUrl}", EditorStyles.textField, GUILayout.Height(18f));
        EditorGUILayout.SelectableLabel($"Gameplay Label: {AddressablesHotfixSetupUtility.GameplayLabel}", EditorStyles.textField, GUILayout.Height(18f));
    }

    private void DrawSingleStepButtons()
    {
        EditorGUILayout.LabelField("Single Build", EditorStyles.boldLabel);

        // 构建步骤执行时禁用按钮，避免重复点击导致流程重入。
        EditorGUI.BeginDisabledGroup(isBuilding);


        if (GUILayout.Button("Build Lua"))
        {
            // 根据 Assets/Lua 生成 LuaRemote/lua_manifest.json 和 zip 热更包。
            RunStep("Build Lua", HotfixLuaServerBuildUtility.TryBuildHttpLuaHotfixPackage);
        }


        if (GUILayout.Button("Build Addressables"))
        {
            // 将 Addressables catalog、hash、bundle 构建到本地 HTTP 服务器目录。
            RunStep("Build Addressables", AddressablesHotfixSetupUtility.TryBuildHttpRemoteContent);
        }

        EditorGUI.EndDisabledGroup();
    }

    private void DrawBuildAllButton()
    {
        EditorGUILayout.LabelField("All Build", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(isBuilding);
        if (GUILayout.Button("Build All Hotfix"))
        {
            // 一键执行当前项目约定的完整热更新构建顺序。
            BuildAllHotfix();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(isBuilding);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open Server Root"))
            {
                // 打开 D:/AddressablesServerRoot，也就是本地 HTTP 服务器根目录。
                OpenFolder(AddressablesHotfixSetupUtility.ServerRoot);
            }

            if (GUILayout.Button("Open LuaRemote Folder"))
            {
                // 打开 Lua 热更目录，里面存放 lua_manifest.json 和 Lua zip 包。
                OpenFolder(HotfixLuaServerBuildUtility.LuaRemotePath);
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open AddressablesRemote Folder"))
            {
                // 打开 Addressables 热更目录，里面存放 catalog、hash 和 bundle。
                OpenFolder(AddressablesRemotePath);
            }

        }

        EditorGUI.EndDisabledGroup();
    }

    // private void DrawStatus()
    // {
    //     // 状态显示保持简单，只帮助你确认最近一次执行到哪一步，不额外增加配置文件。
    //     EditorGUILayout.LabelField("Last Build Status", EditorStyles.boldLabel);
    //     EditorGUILayout.LabelField("Status", lastBuildStatus);
    //     EditorGUILayout.LabelField("Time", lastBuildTime);
    //     EditorGUILayout.LabelField("Last Error", lastError);
    // }

    private void RunStep(string stepName, Func<bool> action)
    {
        // 单步按钮的统一执行包装。
        // Func<bool> 的返回值用于告诉面板这个工具步骤是否成功。
        isBuilding = true;
        lastBuildStatus = $"Running: {stepName}";
        lastError = "-";
        Repaint();

        try
        {
            bool success = action();
            SetResult(success, success ? $"{stepName} complete" : $"{stepName} failed");
        }
        catch (Exception ex)
        {
            // 捕获意外异常，避免 EditorWindow 卡在“正在构建”的状态。
            Debug.LogError($"[HotfixBuildPanel] {stepName} exception: {ex.Message}");
            SetResult(false, ex.Message);
        }
        finally
        {
            isBuilding = false;
            Repaint();
        }
    }

    private void BuildAllHotfix()
    {
        // 一键构建顺序：
        // 1. 检查 Lua 源目录是否存在。
        // 2. 同步 Lua 到 StreamingAssets，作为包内基础 Lua。
        // 3. 构建 Lua zip 和 manifest，用于 HTTP 脚本热更新。
        // 4. 应用 Addressables 远端 HTTP 配置。
        // 5. 构建 Addressables 远端资源内容。
        isBuilding = true;
        lastBuildStatus = "Running: Build All Hotfix";
        lastError = "-";
        Repaint();

        try
        {
            if (!Directory.Exists(HotfixLuaBuildUtility.SourcePath))
            {
                // 没有 Lua 源目录时直接停止，避免生成一个误导性的空热更包。
                SetResult(false, $"Lua source path not found: {HotfixLuaBuildUtility.SourcePath}");
                return;
            }

            if (!RunBuildAllStep("Sync Lua To StreamingAssets", HotfixLuaBuildUtility.TrySyncLuaToStreamingAssets))
            {
                return;
            }

            if (!RunBuildAllStep("Build Lua Hotfix Package", HotfixLuaServerBuildUtility.TryBuildHttpLuaHotfixPackage))
            {
                return;
            }

            if (!RunBuildAllStep("Setup Addressables HTTP Remote", AddressablesHotfixSetupUtility.TrySetupHttpRemoteGameplayAssets))
            {
                return;
            }

            if (!RunBuildAllStep("Build Addressables Remote Content", () => AddressablesHotfixSetupUtility.TryBuildHttpRemoteContent(false)))
            {
                // false 表示前面已经执行过 Setup，这里不需要重复配置 Addressables。
                return;
            }

            PrintTestUrls();
            Debug.Log("[HotfixBuildPanel] Build complete.");
            SetResult(true, "Build All Hotfix complete");
        }
        catch (Exception ex)
        {
            // 和 RunStep 一样兜底，确保异常后面板仍然可继续使用。
            Debug.LogError($"[HotfixBuildPanel] Build All Hotfix exception: {ex.Message}");
            SetResult(false, ex.Message);
        }
        finally
        {
            isBuilding = false;
            Repaint();
        }
    }

    private bool RunBuildAllStep(string stepName, Func<bool> action)
    {
        // 执行一键流程中的某一个步骤。
        // 如果返回 false，后续步骤会停止，避免基于失败状态继续产出资源。
        lastBuildStatus = $"Running: {stepName}";
        Repaint();

        bool success = action();
        if (!success)
        {
            SetResult(false, $"{stepName} failed");
            return false;
        }

        return true;
    }

    private void SetResult(bool success, string message)
    {
        // 统一更新结果状态，保证 Console 输出和面板显示是一致的。
        lastBuildTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        lastBuildStatus = success ? message : "Failed";
        lastError = success ? "-" : message;

        if (!success)
        {
            Debug.LogError($"[HotfixBuildPanel] {message}");
        }
    }

    private static void OpenFolder(string path)
    {
        // 先创建目录，再打开目录，保证 RevealInFinder 总是有真实路径可用。
        Directory.CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
    }

    private static void PrintTestUrls()
    {
        // 这些 URL 可以复制到浏览器里，用来确认本地 HTTP 服务器是否能访问热更文件。
        Debug.Log($"[HotfixBuildPanel] Lua Manifest: {LuaManifestUrl}");
        Debug.Log($"[HotfixBuildPanel] Addressables Catalog: {AddressablesCatalogUrl}");
        Debug.Log($"[HotfixBuildPanel] Start server: python D:/AddressablesServerRoot/start_addressables_server.py");
    }
}
