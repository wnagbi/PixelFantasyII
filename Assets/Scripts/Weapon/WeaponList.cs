using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// 玩家身上的武器列表管理器。
// 负责根据选择结果激活新武器，或者给已经获得的武器升级。
public class WeaponList : MonoBehaviour
{
    // Inspector 中按固定顺序配置玩家所有武器对象。
    public GameObject[] weaponList;
    // 当前已经获得的武器。
    public List<GameObject> weaponBag;
    private Weapon weapon;
    private int levelUpID;

    private void Start()
    {
        
    }
    /// <summary>
    /// 没获得过：激活武器并加入背包；已获得：调用对应 Weapon.LevelUp。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 WeaponList 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void GetWeapon(int index)
    {
        // 没获得过：激活武器并加入背包；已获得：调用对应 Weapon.LevelUp。
        if (CheckWeapon(weaponList[index]))
        {
            weaponList[index].SetActive(true);
            weaponBag.Add(weaponList[index]);
            
        }
        else
        {
            weaponBag[levelUpID].GetComponent<Weapon>().LevelUp();
        }
    }


    /// <summary>
    /// 检查并处理 WeaponList 中与 CheckWeapon 对应的条件。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 WeaponList 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public bool CheckWeapon(GameObject obj) 
    {
        // 检查目标武器是否已经在背包里；如果找到，记录位置供升级使用。
        for (int i = 0; i < weaponBag.Count; i++) 
        {
            if (weaponBag[i] == obj) 
            {
                levelUpID = i;
                return false;
            }
        }
        return true;

    }

}
