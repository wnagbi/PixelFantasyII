using UnityEngine;

// Drop generator attached to enemies. Reads drop chance from Lua when available.
public class PickUpGenerator : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    public PropPrefab[] propPrefab;

    public void DropItems()
    {
        Vector3 pos = transform.position;
        foreach (PropPrefab propprefab in propPrefab)
        {
            if (propprefab == null)
            {
                continue;
            }

            // Blood potions are capped globally so the map cannot fill with healing items.
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
public class PropPrefab
{
    public string prefabName;
    public GameObject prefab;
    [Range(0f, 100f)] public float PropPercentage;
}
