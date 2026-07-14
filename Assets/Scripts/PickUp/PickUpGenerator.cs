using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 掉落生成器。
// 挂在敌人身上，敌人死亡时根据掉落配置从对象池生成经验、血瓶等拾取物。
public class PickUpGenerator : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    public PropPrefab[] propPrefab;

    public void DropItems() 
    {
        Vector3 pos = transform.position;
        foreach (var propprefab in propPrefab) 
        {
            // 掉落概率优先从 Lua 配置读取。
            // key 使用对象池/Prefab 名称，所以 Lua 可以按不同掉落物单独配置概率。
            float percentage = GetDropPercentage(propprefab);
            if (Random.Range(0f, 100f) <= percentage) 
            {
                GameObject item = ObjPoolManager.instance.GetObj(propprefab.prefabName);
                if (item != null)
                {
                    item.transform.position = pos + offset;
                }
                //Instantiate(propprefab.prefab, pos + offset, Quaternion.identity);
            }
        }
    }

    private float GetDropPercentage(PropPrefab propprefab)
    {
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
// 单个掉落项配置。
// prefabName 对应对象池名称，PropPercentage 是默认掉落概率。
public class PropPrefab 
{
    public string prefabName;
    public GameObject prefab;
    [Range(0f, 100f)] public float PropPercentage;
}
