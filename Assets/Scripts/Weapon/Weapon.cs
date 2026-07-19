using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(WeaponController))]
// 武器数据组件。
// 和具体 WeaponController 放在同一个 GameObject 上，保存等级、是否获得、是否满级等状态。
public class Weapon : MonoBehaviour
{
    public int weaponId;
    public int weaponLevel;
    public string weaponName;
    public bool isLevelMax;
    public bool isGet;
    public UnityEvent levelUp;

    //private void OnEnable()
    //{
    //    isGet = true;
    //}
    /// <summary>
    /// 通过 UnityEvent 调用具体武器控制器上的 levelUp 方法。 这样选择界面不需要知道每种武器的具体控制器类型。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Weapon 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void LevelUp() 
    {
        // 通过 UnityEvent 调用具体武器控制器上的 levelUp 方法。
        // 这样选择界面不需要知道每种武器的具体控制器类型。
        levelUp?.Invoke();
        
    }
    

}
