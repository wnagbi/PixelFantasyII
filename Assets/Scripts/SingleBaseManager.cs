using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 普通 C# 类使用的简易单例基类。
// 注意：它不是 MonoBehaviour 单例，适合 PlayerData 这类不挂场景物体的数据类。
public class SingleBaseManager<T> where T: new()
{
    private static T instance;

    // 获取唯一实例。第一次访问时通过 new T() 创建，之后一直复用同一个对象。
    /// <summary>
    /// 获取或延迟创建当前泛型单例实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SingleBaseManager 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public static T getInstance() 
    {
        if (instance == null)
            instance =new T();
        return instance;
    }
}
