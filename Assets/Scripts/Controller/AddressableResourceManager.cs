using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class AddressableResourceManager
{
    private static readonly Dictionary<string, AsyncOperationHandle> cachedHandles = new Dictionary<string, AsyncOperationHandle>();
    private static bool initialized;
    private static bool initializing;
    private static bool internalIdTransformRegistered;

    public static IEnumerator Initialize()
    {
        if (initialized)
        {
            yield break;
        }

        while (initializing)
        {
            yield return null;
            if (initialized)
            {
                yield break;
            }
        }

        initializing = true;
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

    public static IEnumerator CheckAndUpdateCatalogs()
    {
        yield return CheckAndUpdateCatalogs(null);
    }

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

    public static IEnumerator DownloadDependencies(string labelOrKey)
    {
        yield return DownloadDependencies(labelOrKey, null);
    }

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

    public static IEnumerator LoadPrefab(string key, Action<GameObject> onSuccess, Action onFail = null)
    {
        yield return LoadAsset<GameObject>(key, onSuccess, onFail);
    }

    public static IEnumerator PreloadPrefab(string key, Action<GameObject> onSuccess, Action onFail = null)
    {
        yield return LoadPrefab(key, onSuccess, onFail);
    }

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

    public static void ReleaseInstance(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        Addressables.ReleaseInstance(instance);
    }

    private static void RegisterInternalIdTransform()
    {
        if (internalIdTransformRegistered)
        {
            return;
        }

        Addressables.InternalIdTransformFunc = TransformAddressablesRemotePath;
        internalIdTransformRegistered = true;
    }

    private static string TransformAddressablesRemotePath(IResourceLocation location)
    {
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

        string relativeRemotePath = normalized.Substring(markerIndex + "AddressablesRemote/".Length);
        string runtimeRemoteRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "AddressablesRemote"));
        string runtimePath = Path.Combine(runtimeRemoteRoot, relativeRemotePath.Replace('/', Path.DirectorySeparatorChar));
        string transformed = "file:///" + runtimePath.Replace('\\', '/');

        Debug.Log($"[Addressables] Redirect remote path: {internalId} -> {transformed}");
        return transformed;
    }

    private static void Report(Action<float, string> onProgress, float progress, string status)
    {
        onProgress?.Invoke(Mathf.Clamp01(progress), status);
    }
}
