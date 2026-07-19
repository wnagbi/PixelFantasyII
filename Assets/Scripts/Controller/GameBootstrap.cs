using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 挂在 start 场景中的启动流程控制器。
// 它只负责驱动热更新流程和更新已有加载 UI，不会在运行时创建 UI 对象。
public sealed class GameBootstrap : MonoBehaviour
{
    public StartLoadingView loadingView;
    public string titleSceneName = "title";
    public string gameplayLabel = "gameplay";

    private IEnumerator Start()
    {
        if (loadingView == null)
        {
            Debug.LogWarning("[GameBootstrap] StartLoadingView is missing. Bootstrap will continue without loading UI.");
        }

        // 启动顺序必须固定：先应用 Lua 文件，再初始化 LuaEnv，最后检查资源 Catalog。
        yield return SmoothTo(0.05f, "准备启动...");

        yield return LuaHotfixRemoteUpdater.CheckAndApply(
            CreateProgressCallback(0.05f, 0.15f, "检查脚本更新..."));
        yield return SmoothTo(0.15f, "检查脚本更新...");

        LuaManager.Instance.Init();
        yield return SmoothTo(0.30f, "初始化脚本环境...");

        yield return AddressableResourceManager.CheckAndUpdateCatalogs(
            CreateProgressCallback(0.30f, 0.45f, "检查资源版本..."));
        yield return SmoothTo(0.45f, "检查资源版本...");

        yield return AddressableResourceManager.DownloadDependencies(
            gameplayLabel,
            CreateProgressCallback(0.45f, 0.85f, "下载玩法资源..."));
        yield return SmoothTo(0.85f, "下载玩法资源...");

        yield return SmoothTo(1f, "进入游戏...");
        yield return new WaitForSecondsRealtime(0.2f);

        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// loadingView 缺失时跳过表现，不阻塞真正的热更新流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameBootstrap 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private IEnumerator SmoothTo(float target, string status)
    {
        // loadingView 缺失时跳过表现，不阻塞真正的热更新流程。
        if (loadingView == null)
        {
            yield break;
        }

        yield return loadingView.SmoothTo(target, status);
    }

    /// <summary>
    /// 把子系统自己的 0~1 进度映射到启动总进度的指定区间。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameBootstrap 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private Action<float, string> CreateProgressCallback(float start, float end, string fallbackStatus)
    {
        // 把子系统自己的 0~1 进度映射到启动总进度的指定区间。
        if (loadingView == null)
        {
            return null;
        }

        return (progress, status) =>
        {
            float mappedProgress = Mathf.Lerp(start, end, Mathf.Clamp01(progress));
            string displayStatus = string.IsNullOrEmpty(status) ? fallbackStatus : status;
            loadingView.SetProgress(mappedProgress, displayStatus);
        };
    }
}
