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
    private void InitializeAllPools() //锟斤拷始锟斤拷锟斤拷锟斤拷锟?
    {
        // 场景启动时预热所有对象池。
        foreach (var pool in pools) 
        {
            pool.Initiliza();
        }
    }
    public GameObject GetObj(string name) //锟斤拷取锟斤拷锟斤拷
    {
        // 按 poolName 查找对象池并取出一个对象。
        var pool = pools.Find(p => p.poolName == name);
        if(pool != null)
            return pool.GetObj();
        return null;
    }

    public void ReturnObj(GameObject obj) //锟斤拷锟截讹拷锟斤拷锟斤拷锟?
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
        string enemyPoolName = LuaConfig.GetString("config.stage_config", "enemy_spawn", "pool_name", "Silm");
        string enemyPrefabKey = LuaConfig.GetString("config.stage_config", "enemy_spawn", "prefabKey", string.Empty);
        yield return PreloadAddressablePrefab(enemyPoolName, enemyPrefabKey);

        yield return PreloadDropAddressablePool("Exp");
        yield return PreloadDropAddressablePool("Potion");
    }

    private IEnumerator PreloadDropAddressablePool(string dropConfigKey)
    {
        if (!TryGetDropAddressableConfig(dropConfigKey, out string poolName, out string prefabKey))
        {
            yield break;
        }

        yield return PreloadAddressablePrefab(poolName, prefabKey);
    }

    private bool TryGetDropAddressableConfig(string dropConfigKey, out string poolName, out string prefabKey)
    {
        poolName = dropConfigKey;
        prefabKey = string.Empty;

        if (!LuaConfig.TryGetTable("config.stage_config", "drops", out XLua.LuaTable drops))
        {
            return false;
        }

        XLua.LuaTable drop = null;
        try
        {
            drop = drops.Get<string, XLua.LuaTable>(dropConfigKey);
            if (drop == null)
            {
                return false;
            }

            string luaPoolName = drop.Get<string>("poolName");
            string luaPrefabKey = drop.Get<string>("prefabKey");

            poolName = string.IsNullOrWhiteSpace(luaPoolName) ? dropConfigKey : luaPoolName;
            prefabKey = string.IsNullOrWhiteSpace(luaPrefabKey) ? string.Empty : luaPrefabKey;
            return !string.IsNullOrWhiteSpace(prefabKey);
        }
        catch
        {
            return false;
        }
        finally
        {
            drop?.Dispose();
            drops.Dispose();
        }
    }
}
