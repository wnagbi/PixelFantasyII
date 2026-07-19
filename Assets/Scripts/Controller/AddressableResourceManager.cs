using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Addressables 运行时统一入口。
/// 业务层通过本类检查远端 Catalog、预下载 label、加载/实例化资源并释放 Handle，
/// 从而把 Addressables API、失败回退和资源生命周期管理集中在一个位置。
/// </summary>
public static class AddressableResourceManager
{
    // LoadAsset 加载的资源 Handle 按 Address 缓存；资源仍被使用时不能提前 Release。
    private static readonly Dictionary<string, AsyncOperationHandle> cachedHandles = new Dictionary<string, AsyncOperationHandle>();
    // initialized 表示初始化成功；initializing 防止多个协程同时启动 InitializeAsync。
    private static bool initialized;
    private static bool initializing;
    // InternalIdTransformFunc 是全局回调，只允许注册一次。
    private static bool internalIdTransformRegistered;

    /// <summary>
    /// 初始化 Addressables。并发调用会等待正在执行的初始化，失败时由业务层使用 Inspector fallback。
    /// </summary>
    /// <remarks>
    /// 使用注意：应在检查 Catalog 或加载远端资源前 yield 等待该协程完成。
    /// </remarks>
    public static IEnumerator Initialize()
    {
        if (initialized)
        {
            yield break;
        }

        while (initializing)
        {
            // 其它调用已经在初始化时，本协程只等待结果，不重复创建 Handle。
            yield return null;
            if (initialized)
            {
                yield break;
            }
        }

        initializing = true;
        // 初始化前注册路径转换，保证旧 file 模式 Catalog 也能在打包目录中找到资源。
        RegisterInternalIdTransform();

        AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync(false);
        yield return handle;

        if (!handle.IsValid())
        {
            Debug.LogWarning("[Addressables] Initialize handle became invalid. Gameplay will use Inspector fallback assets.");
            initializing = false;
            yield break;
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            initialized = true;
            Debug.Log("[Addressables] Initialized.");
        }
        else
        {
            Debug.LogWarning("[Addressables] Initialize failed. Gameplay will use Inspector fallback assets.");
        }

        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }

        initializing = false;
    }

    /// <summary>
    /// 使用无进度回调的兼容入口。
    /// </summary>
    /// <remarks>
    /// 使用注意：需要显示启动进度时应调用带 onProgress 参数的重载。
    /// </remarks>
    public static IEnumerator CheckAndUpdateCatalogs()
    {
        yield return CheckAndUpdateCatalogs(null);
    }

    /// <summary>
    /// 检查并应用远端 Catalog 更新。进度值范围为 0~1，供 StartLoadingView 映射显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：服务器不可用时该流程会记录警告并回退，调用方仍应继续启动游戏。
    /// </remarks>
    public static IEnumerator CheckAndUpdateCatalogs(Action<float, string> onProgress)
    {
        Report(onProgress, 0f, "初始化资源系统...");

        yield return Initialize();
        if (!initialized)
        {
            Report(onProgress, 1f, "资源系统初始化失败，使用默认资源...");
            yield break;
        }

        Report(onProgress, 0.20f, "检查资源版本...");

        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
        // 不自动释放 Handle，确保读取 Status/Result 后由本方法统一释放。
        while (checkHandle.IsValid() && !checkHandle.IsDone)
        {
            Report(onProgress, Mathf.Lerp(0.20f, 0.55f, checkHandle.PercentComplete), "检查资源版本...");
            yield return null;
        }

        if (!checkHandle.IsValid() || checkHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning("[Addressables] Check catalog updates failed.");
            if (checkHandle.IsValid())
            {
                Addressables.Release(checkHandle);
            }

            Report(onProgress, 1f, "资源版本检查失败，使用默认资源...");
            yield break;
        }

        List<string> catalogs = checkHandle.Result;
        if (catalogs == null || catalogs.Count == 0)
        {
            // hash 没有变化时无需更新 Catalog，也不会重新下载已缓存 Bundle。
            Debug.Log("[Addressables] No catalog update.");
            Addressables.Release(checkHandle);
            Report(onProgress, 1f, "资源已是最新版本...");
            yield break;
        }

        Debug.Log($"[Addressables] Updating {catalogs.Count} catalog(s).");
        Report(onProgress, 0.60f, "更新资源目录...");

        AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogs, false);
        while (updateHandle.IsValid() && !updateHandle.IsDone)
        {
            Report(onProgress, Mathf.Lerp(0.60f, 0.95f, updateHandle.PercentComplete), "更新资源目录...");
            yield return null;
        }

        if (!updateHandle.IsValid() || updateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning("[Addressables] Update catalogs failed.");
            Report(onProgress, 1f, "资源目录更新失败，使用默认资源...");
        }
        else
        {
            Report(onProgress, 1f, "资源目录更新完成...");
        }

        if (updateHandle.IsValid())
        {
            Addressables.Release(updateHandle);
        }

        if (checkHandle.IsValid())
        {
            Addressables.Release(checkHandle);
        }
    }

    /// <summary>
    /// 预下载指定 label/key 的全部依赖，不接收进度回调。
    /// </summary>
    /// <remarks>
    /// 使用注意：labelOrKey 必须与 Addressables 配置一致；需要显示进度时使用带回调的重载。
    /// </remarks>
    public static IEnumerator DownloadDependencies(string labelOrKey)
    {
        yield return DownloadDependencies(labelOrKey, null);
    }

    /// <summary>
    /// 先计算尚未缓存的下载大小，再下载依赖；大小为 0 表示本地缓存已经是最新版本。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用方必须 yield 等待完成，失败时由后续资源加载使用 Inspector fallback。
    /// </remarks>
    public static IEnumerator DownloadDependencies(string labelOrKey, Action<float, string> onProgress)
    {
        if (string.IsNullOrWhiteSpace(labelOrKey))
        {
            Report(onProgress, 1f, "资源标签为空，跳过下载...");
            yield break;
        }

        Report(onProgress, 0f, "计算资源下载大小...");

        yield return Initialize();
        if (!initialized)
        {
            Report(onProgress, 1f, "资源系统不可用，使用默认资源...");
            yield break;
        }

        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(labelOrKey);
        while (sizeHandle.IsValid() && !sizeHandle.IsDone)
        {
            Report(onProgress, Mathf.Lerp(0f, 0.20f, sizeHandle.PercentComplete), "计算资源下载大小...");
            yield return null;
        }

        if (!sizeHandle.IsValid() || sizeHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning($"[Addressables] Get download size failed: {labelOrKey}");
            if (sizeHandle.IsValid())
            {
                Addressables.Release(sizeHandle);
            }

            Report(onProgress, 1f, "资源下载检查失败，使用默认资源...");
            yield break;
        }

        long downloadSize = sizeHandle.Result;
        // sizeHandle 只用于查询，不需要长期持有。
        Addressables.Release(sizeHandle);

        if (downloadSize <= 0)
        {
            Debug.Log($"[Addressables] No download needed: {labelOrKey}");
            Report(onProgress, 1f, "资源已是最新版本...");
            yield break;
        }

        Debug.Log($"[Addressables] Downloading {labelOrKey}, size: {downloadSize} bytes.");
        AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(labelOrKey);
        while (downloadHandle.IsValid() && !downloadHandle.IsDone)
        {
            Report(onProgress, Mathf.Lerp(0.20f, 0.98f, downloadHandle.PercentComplete), "下载玩法资源...");
            yield return null;
        }

        if (!downloadHandle.IsValid() || downloadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning($"[Addressables] Download dependencies failed: {labelOrKey}");
            Report(onProgress, 1f, "玩法资源下载失败，使用默认资源...");
        }
        else
        {
            Report(onProgress, 1f, "玩法资源下载完成...");
        }

        if (downloadHandle.IsValid())
        {
            Addressables.Release(downloadHandle);
        }
    }

    /// <summary>
    /// 按 Address 加载并缓存资源。相同 key 后续直接复用缓存 Handle 中的资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用方不应 Destroy 资源本体，使用结束后通过 Release(key) 释放缓存引用。
    /// </remarks>
    public static IEnumerator LoadAsset<T>(string key, Action<T> onSuccess, Action onFail = null) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            onFail?.Invoke();
            yield break;
        }

        yield return Initialize();
        if (!initialized)
        {
            onFail?.Invoke();
            yield break;
        }

        if (cachedHandles.TryGetValue(key, out AsyncOperationHandle cachedHandle))
        {
            // 缓存类型与本次请求一致时直接返回，不再发起异步加载。
            if (cachedHandle.IsValid() && cachedHandle.Result is T cachedAsset)
            {
                onSuccess?.Invoke(cachedAsset);
                yield break;
            }

            cachedHandles.Remove(key);
        }

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 成功 Handle 不能在这里 Release，否则返回的资源可能立即失效。
            cachedHandles[key] = handle;
            Debug.Log($"[Addressables] Loaded asset: {key} -> {handle.Result.name}");
            onSuccess?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogWarning($"[Addressables] Load asset failed: {key}");
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            onFail?.Invoke();
        }
    }

    /// <summary>
    /// GameObject 资源加载的语义化包装，返回 Prefab asset，不创建实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：成功回调得到的是 Prefab 资源；实例化和实例生命周期由调用方负责。
    /// </remarks>
    public static IEnumerator LoadPrefab(string key, Action<GameObject> onSuccess, Action onFail = null)
    {
        yield return LoadAsset<GameObject>(key, onSuccess, onFail);
    }

    /// <summary>
    /// 预加载 Prefab 到缓存，供对象池替换 prefab 后继续使用普通 Instantiate。
    /// </summary>
    /// <remarks>
    /// 使用注意：应在对象池预热前完成，并在 Addressables 加载失败时保留 Inspector 原 Prefab。
    /// </remarks>
    public static IEnumerator PreloadPrefab(string key, Action<GameObject> onSuccess, Action onFail = null)
    {
        yield return LoadPrefab(key, onSuccess, onFail);
    }

    /// <summary>
    /// 通过 Addressables 创建并返回一个 GameObject 实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：成功实例必须使用 ReleaseInstance 回收，不能只调用 Destroy。
    /// </remarks>
    public static IEnumerator InstantiateAsync(string key, Vector3 position, Quaternion rotation, Action<GameObject> onSuccess, Action onFail = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            onFail?.Invoke();
            yield break;
        }

        yield return Initialize();
        if (!initialized)
        {
            onFail?.Invoke();
            yield break;
        }

        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, position, rotation);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            onSuccess?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogWarning($"[Addressables] Instantiate failed: {key}");
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            onFail?.Invoke();
        }
    }

    /// <summary>
    /// 释放 LoadAsset 缓存的资源 Handle。不存在或已经无效时安全跳过。
    /// </summary>
    /// <remarks>
    /// 使用注意：只有确认所有使用方都不再依赖该 key 对应资源时才能释放。
    /// </remarks>
    public static void Release(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!cachedHandles.TryGetValue(key, out AsyncOperationHandle handle))
        {
            return;
        }

        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }

        cachedHandles.Remove(key);
    }

    /// <summary>
    /// 释放由 Addressables.InstantiateAsync 创建的实例及其引用计数。
    /// </summary>
    /// <remarks>
    /// 使用注意：该方法只适用于 Addressables 创建的实例，对象池普通 Instantiate 的对象应走对象池回收。
    /// </remarks>
    public static void ReleaseInstance(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        Addressables.ReleaseInstance(instance);
    }

    /// <summary>
    /// 注册一次 InternalId 转换回调，兼容旧版 file 地址并保留 HTTP 地址。
    /// </summary>
    /// <remarks>
    /// 使用注意：全局只能注册一次；重复追加回调会让资源地址被多次改写。
    /// </remarks>
    private static void RegisterInternalIdTransform()
    {
        if (internalIdTransformRegistered)
        {
            return;
        }

        Addressables.InternalIdTransformFunc = TransformAddressablesRemotePath;
        internalIdTransformRegistered = true;
    }

    /// <summary>
    /// 把旧 file 模式远端地址转换到客户端同级目录，同时让 HTTP/HTTPS 地址原样通过。
    /// </summary>
    /// <remarks>
    /// 使用注意：不能改写 HTTP Catalog 中的 internal id，否则打包客户端会错误访问本地文件。
    /// </remarks>
    private static string TransformAddressablesRemotePath(IResourceLocation location)
    {
        // HTTP/HTTPS 是当前正式测试模式，必须保持 Catalog 给出的服务器地址不变。
        string internalId = location.InternalId;
        if (string.IsNullOrEmpty(internalId) || internalId.IndexOf("AddressablesRemote", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return internalId;
        }

        string normalized = internalId.Replace('\\', '/');
        if (normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return internalId;
        }

        if (!normalized.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            return internalId;
        }

        int markerIndex = normalized.IndexOf("AddressablesRemote/", StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return internalId;
        }

        // 兼容旧 file:// Catalog：把构建机绝对路径改写为 exe 同级 AddressablesRemote。
        string relativeRemotePath = normalized.Substring(markerIndex + "AddressablesRemote/".Length);
        string runtimeRemoteRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "AddressablesRemote"));
        string runtimePath = Path.Combine(runtimeRemoteRoot, relativeRemotePath.Replace('/', Path.DirectorySeparatorChar));
        string transformed = "file:///" + runtimePath.Replace('\\', '/');

        Debug.Log($"[Addressables] Redirect remote path: {internalId} -> {transformed}");
        return transformed;
    }

    /// <summary>
    /// 安全上报 0 到 1 范围内的 Addressables 启动进度和状态文字。
    /// </summary>
    /// <remarks>
    /// 使用注意：回调允许为 null；该方法只更新显示，不改变下载和回退流程。
    /// </remarks>
    private static void Report(Action<float, string> onProgress, float progress, string status)
    {
        // 所有启动阶段都通过同一入口限制进度范围并更新加载界面。
        onProgress?.Invoke(Mathf.Clamp01(progress), status);
    }
}
