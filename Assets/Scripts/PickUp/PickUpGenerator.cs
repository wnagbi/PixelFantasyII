using UnityEngine;

// 敌人掉落生成器。
// 优先读取 Lua 掉落概率和对象池名称，规则缺失时使用 Inspector 默认配置。
public class PickUpGenerator : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    public PropPrefab[] propPrefab;

    public void DropItems()
    {
        // 每种掉落独立做一次概率判定，成功后从对应对象池取出并放到敌人死亡点。
        Vector3 pos = transform.position;
        foreach (PropPrefab propprefab in propPrefab)
        {
            if (propprefab == null)
            {
                continue;
            }

            // 血瓶使用全局数量上限，避免场景中长期堆积过多回复道具。
            if (IsBloodDrop(propprefab) && !PickUp.CanSpawnBloodPickup())
            {
                continue;
            }

            float percentage = GetDropPercentage(propprefab);
            if (Random.Range(0f, 100f) > percentage)
            {
                continue;
            }

            GameObject item = ObjPoolManager.instance.GetObj(propprefab.prefabName);
            if (item != null)
            {
                item.transform.position = pos + offset;
            }
        }
    }

    private bool IsBloodDrop(PropPrefab propprefab)
    {
        // 优先读取 Prefab 上的 PickUp 类型，旧配置则兼容 Potion/Blood 池名。
        if (propprefab.prefab != null
            && propprefab.prefab.TryGetComponent(out PickUp pickup)
            && pickup.pickUpTpye == PickUpTpye.Blood)
        {
            return true;
        }

        return propprefab.prefabName == "Potion" || propprefab.prefabName == "Blood";
    }

    private float GetDropPercentage(PropPrefab propprefab)
    {
        // stage_config.drops 支持 table 结构；读取失败时回退 Inspector 百分比。
        if (propprefab == null || string.IsNullOrWhiteSpace(propprefab.prefabName))
        {
            return 0f;
        }

        if (!LuaConfig.TryGetTable("config.stage_config", "drops", out XLua.LuaTable drops))
        {
            return propprefab.PropPercentage;
        }

        XLua.LuaTable drop = null;
        try
        {
            drop = drops.Get<string, XLua.LuaTable>(propprefab.prefabName);
            if (drop != null)
            {
                object percentage = drop.Get<object>("percentage");
                return ConvertLuaFloat(percentage, propprefab.PropPercentage);
            }

            object flatPercentage = drops.Get<object>(propprefab.prefabName);
            return ConvertLuaFloat(flatPercentage, propprefab.PropPercentage);
        }
        catch
        {
            return propprefab.PropPercentage;
        }
        finally
        {
            drop?.Dispose();
            drops.Dispose();
        }
    }

    private float ConvertLuaFloat(object value, float fallback)
    {
        // xLua number 可能以不同托管数值类型返回，统一宽松转换成 float。
        if (value == null)
        {
            return fallback;
        }

        try
        {
            return System.Convert.ToSingle(value);
        }
        catch
        {
            return fallback;
        }
    }
}

[System.Serializable]
// Inspector 中的一条掉落配置：对象池名称、fallback Prefab 和默认概率。
public class PropPrefab
{
    public string prefabName;
    public GameObject prefab;
    [Range(0f, 100f)] public float PropPercentage;
}
