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
    /// <summary>
    /// 初始化并预热 Inspector 中配置的全部对象池。
    /// </summary>
    /// <remarks>
    /// 使用注意：只在管理器 Awake 中执行一次；重复预热会额外创建对象。
    /// </remarks>
    private void InitializeAllPools() 
    {
        // 场景启动时预热所有对象池。
        foreach (var pool in pools) 
        {
            pool.Initiliza();
        }
    }
    /// <summary>
    /// 按 poolName 从对应对象池取出并启用一个对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：池名不存在时返回 null；调用方必须判空并保证名称与 Lua 配置一致。
    /// </remarks>
    public GameObject GetObj(string name) 
    {
        // 按 poolName 查找对象池并取出一个对象。
        ObjPool pool = pools.Find(p => p.poolName == name);
        if(pool != null)
            return pool.GetObj();
        return null;
    }

    /// <summary>
    /// 查找对象所属的池并将其禁用、归还。
    /// </summary>
    /// <remarks>
    /// 使用注意：对象必须由本管理器中的池创建；同一对象不能重复归还。
    /// </remarks>
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

    /// <summary>
    /// 预加载 Addressable Prefab，并在成功后替换指定池的默认 Prefab。
    /// </summary>
    /// <remarks>
    /// 使用注意：这是协程；失败时保留 Inspector fallback，不应阻断其它池加载。
    /// </remarks>
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

    /// <summary>
    /// 用已加载 Prefab 替换目标对象池模板并重新预热。
    /// </summary>
    /// <remarks>
    /// 使用注意：Rewarm 会清理旧池对象；应在正式刷怪和掉落开始前调用。
    /// </remarks>
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

    /// <summary>
    /// 读取 config.pool_config 并依次预加载全部可热更新对象池 Prefab。
    /// </summary>
    /// <remarks>
    /// 使用注意：Lua 配置缺失或单项无效时使用 Inspector fallback；LuaTable 用完必须 Dispose。
    /// </remarks>
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

        /// <summary>
        /// 创建一条已校验的对象池名称与 Addressable key 配置。
        /// </summary>
        /// <remarks>
        /// 使用注意：仅作为协程加载前的临时值，不持有 LuaTable 或 Addressables Handle。
        /// </remarks>
        public PoolAddressableConfig(string poolName, string prefabKey)
        {
            this.poolName = poolName;
            this.prefabKey = prefabKey;
        }
    }
}
