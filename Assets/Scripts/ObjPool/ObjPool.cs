using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
// 单个对象池配置与运行时缓存。
// 一个 ObjPool 对应一种 prefab，例如敌人、经验、掉落物等。
public class ObjPool 
{
    public string poolName;
    public GameObject prefab;
    // 初始预热数量，以及不够用时每次扩容数量。
    public int size;
    public int expandSize;
    public Transform parentTransform;

    // 未激活、可复用对象栈。
    private Stack<GameObject> inactiveObjects = new Stack<GameObject>();

    // 该对象池创建过的所有对象，用于判断 ReturnObj 是否属于当前池。
    public List<GameObject> allObjects = new List<GameObject>();

    /// <summary>
    /// 没有 prefab 时不初始化，避免空引用。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ObjPool 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Initiliza() 
    {
        // 没有 prefab 时不初始化，避免空引用。
        if(prefab == null)
        {
            return;
        }
        PreWarmPool(size);
    }

    /// <summary>
    /// 设置 ObjPool 中与 SetPrefab 对应的状态或数据。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ObjPool 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetPrefab(GameObject newPrefab)
    {
        if (newPrefab != null)
        {
            prefab = newPrefab;
        }
    }

    /// <summary>
    /// 清理旧对象并使用当前 Prefab 重新预热对象池。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ObjPool 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Rewarm()
    {
        for (int i = allObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = allObjects[i];
            if (obj == null)
            {
                allObjects.RemoveAt(i);
                continue;
            }

            if (!obj.activeSelf)
            {
                Object.Destroy(obj);
                allObjects.RemoveAt(i);
            }
        }

        inactiveObjects.Clear();
        PreWarmPool(size);
    }

    /// <summary>
    /// 提前创建一批 inactive 对象，减少战斗中 Instantiate 卡顿。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ObjPool 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void PreWarmPool(int num)
    {
        // 提前创建一批 inactive 对象，减少战斗中 Instantiate 卡顿。
        for (int i = 0; i < num; i++) 
        { 
            CreateNewObj();
        }
    }
    /// <summary>
    /// 优先复用已经回收的对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ObjPool 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public GameObject GetObj()
    {
        // 优先复用已经回收的对象。
        if (inactiveObjects.Count > 0) 
        {
            var obj = inactiveObjects.Pop();
            obj.SetActive(true);
            return obj;
        }
        // 池为空时自动扩容，然后重新取对象。
        ExpandPool();
        return GetObj();
    }
    /// <summary>
    /// 归还或返回 ObjPool 中与 ReturnObj 对应的对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ObjPool 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ReturnObj(GameObject obj) 
    {
        // 只允许回收本池创建的对象，避免不同池之间混淆。
        if (!allObjects.Contains(obj)) 
        {
            return ;
        }
        // 回收时隐藏对象并压回栈，下一次 GetObj 会再次启用。
        obj.SetActive(false);
        inactiveObjects.Push(obj);
        if(parentTransform != null) 
            obj.transform.SetParent(parentTransform); 
    }
    /// <summary>
    /// 新建对象默认 inactive，等 GetObj 时再启用。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ObjPool 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private GameObject CreateNewObj() 
    {
        // 新建对象默认 inactive，等 GetObj 时再启用。
        var newObj = Object.Instantiate(prefab, parentTransform);
        newObj.SetActive(false);
        allObjects.Add(newObj);
        inactiveObjects.Push(newObj);
        return newObj;
    }
    /// <summary>
    /// 按配置数量批量扩容。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ObjPool 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void ExpandPool()
    {
        // 按配置数量批量扩容。
        PreWarmPool(expandSize);
    }
    /// <summary>
    /// 销毁该池创建的所有对象，通常用于场景卸载或彻底重置。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ObjPool 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Clear() 
    {
        // 销毁该池创建的所有对象，通常用于场景卸载或彻底重置。
        foreach (var obj in allObjects) 
        {
            Object.Destroy(obj);
        }
        inactiveObjects.Clear();
        allObjects.Clear();
    }
}
