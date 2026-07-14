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
    public void GetWeapon(int index) //判断武器是否获得
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
