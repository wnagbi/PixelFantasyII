using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 全局对象池管理器。
// 其它系统通过池名 GetObj/ReturnObj，不需要直接 Instantiate/Destroy 常用对象。
public class ObjPoolManager : MonoBehaviour
{
    public static ObjPoolManager instance;

    // Inspector 中配置所有需要预热的对象池。
    public List<ObjPool> pools = new List<ObjPool>();
    private void Awake()
    {
        // 简单场景单例。重复出现时销毁新对象。
        if (instance == null)
        {
            instance = this;
            InitializeAllPools();
            StartCoroutine(PreloadConfiguredAddressablePools());
        }
        else 
        { 
            Destroy(gameObject);
         }
    }
    private void InitializeAllPools() 
    {
        // 场景启动时预热所有对象池。
        foreach (var pool in pools) 
        {
            pool.Initiliza();
        }
    }
    public GameObject GetObj(string name) 
    {
        // 按 poolName 查找对象池并取出一个对象。
        ObjPool pool = pools.Find(p => p.poolName == name);
        if(pool != null)
            return pool.GetObj();
        return null;
    }

    public void ReturnObj(GameObject obj) 
    {
        // 找到这个对象所属的对象池，然后交给该池回收。
        foreach (var pool in pools) 
        {
            if(pool.allObjects.Contains(obj))
            {
                pool.ReturnObj(obj);
                return;
            }
        }
    }

    public IEnumerator PreloadAddressablePrefab(string poolName, string prefabKey)
    {
        if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(prefabKey))
        {
            yield break;
        }

        yield return AddressableResourceManager.PreloadPrefab(
            prefabKey,
            prefab =>
            {
                if (ReplacePoolPrefab(poolName, prefab))
                {
                    Debug.Log($"[ObjPoolManager] Loaded Addressable prefab for pool {poolName}: {prefabKey}", this);
                }
            },
            () => Debug.LogWarning($"[ObjPoolManager] Use Inspector fallback prefab for pool {poolName}: {prefabKey}", this)
        );
    }

    public bool ReplacePoolPrefab(string poolName, GameObject prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        ObjPool pool = pools.Find(p => p.poolName == poolName);
        if (pool == null)
        {
            Debug.LogWarning($"[ObjPoolManager] Pool not found: {poolName}", this);
            return false;
        }

        pool.SetPrefab(prefab);
        pool.Rewarm();
        return true;
    }

    private IEnumerator PreloadConfiguredAddressablePools()
    {
        // 对象池 Addressables 预加载现在完全由 Lua 配置驱动。
        // 配置位置：Assets/Lua/config/pool_config.lua
        // 这样新增可热更对象池时，只需要新增 Lua 配置和 Addressable key，不再修改这里的 C#。
        if (!LuaConfig.TryGetTable("config.pool_config", "addressable_pools", out XLua.LuaTable addressablePools))
        {
            // 找不到配置时不影响游戏启动，所有对象池继续使用 Inspector 里原本拖好的 prefab。
            Debug.LogWarning("[ObjPoolManager] config.pool_config.addressable_pools not found. Use Inspector pool prefabs.", this);
            yield break;
        }

        // 先把 LuaTable 转成 C# 列表，再逐个 yield 加载。
        // 不在 ForEach 回调里直接 yield，因为 C# 的 lambda 不能作为协程步骤暂停。
        List<PoolAddressableConfig> configs = new List<PoolAddressableConfig>();
        try
        {
            addressablePools.ForEach<int, XLua.LuaTable>((_, poolConfig) =>
            {
                if (poolConfig == null)
                {
                    return;
                }

                try
                {
                    string poolName = poolConfig.Get<string>("poolName");
                    string prefabKey = poolConfig.Get<string>("prefabKey");

                    if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(prefabKey))
                    {
                        // 单条配置不完整时只跳过这一项，不影响其它对象池加载。
                        Debug.LogWarning("[ObjPoolManager] Skip invalid addressable pool config.", this);
                        return;
                    }

                    configs.Add(new PoolAddressableConfig(poolName, prefabKey));
                }
                finally
                {
                    // xLua 的 LuaTable 是托管引用，用完要释放，避免 LuaEnv Dispose 时报引用残留。
                    poolConfig.Dispose();
                }
            });
        }
        finally
        {
            // 外层 table 同样需要释放。
            addressablePools.Dispose();
        }

        foreach (PoolAddressableConfig config in configs)
        {
            // 加载成功会替换对象池 prefab 并 Rewarm；失败则保留 Inspector fallback prefab。
            yield return PreloadAddressablePrefab(config.poolName, config.prefabKey);
        }
    }

    // 临时保存一条 Lua 对象池热更配置。
    // 使用 struct 避免为每条配置创建额外 MonoBehaviour 或 ScriptableObject。
    private readonly struct PoolAddressableConfig
    {
        public readonly string poolName;
        public readonly string prefabKey;

        public PoolAddressableConfig(string poolName, string prefabKey)
        {
            this.poolName = poolName;
            this.prefabKey = prefabKey;
        }
    }
}
