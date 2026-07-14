using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

// 升级时的武器选择面板控制器。
// 负责从可选武器列表中随机抽取三个选项，暂停游戏，等待玩家选择升级。
public class WeaponSelectController : MonoBehaviour
{
    // 所有武器选择卡的原始列表。
    public List<GameObject> weaponList;
    public GameObject box;
    public GameObject leftPosition;
    public GameObject rightPosition;
    public GameObject midPosition;
    private GameObject left;
    private GameObject right;
    private GameObject mid;
    public List<GameObject> selectWeapon;
    public static WeaponSelectController instance;
    public Animator ani;

    private void Awake()
    {
        // 其它武器满级时会通过 instance 调用 LevelMaxRemove。
        instance = this;
       
    }
    private void Start()
    {
        // 升级选择时会暂时禁用玩家 Animator，避免暂停时动画状态异常。
        GameObject player= FindObjectOfType<Player>().gameObject;
        ani = player.GetComponent<Animator>();
    }
    public void GenerateSelect() //使用洗牌算法
    {
        // Fisher-Yates 洗牌：把 selectWeapon 随机打乱，然后取前三个显示。
        for (int i = 0; i < selectWeapon.Count; i++) 
        {
            int randomIndex = Random.Range(i, selectWeapon.Count);
            //while (selectWeapon[randomIndex].GetComponent<WeaponSelect>().weapon.isLevelMax) 
            //{
            //    randomIndex = Random.Range(i, selectWeapon.Count);
            //}

            GameObject temp = selectWeapon[i];
            selectWeapon[i] = selectWeapon[randomIndex];
            selectWeapon[randomIndex] = temp;
        }

        // 在左中右三个位置生成选择卡。
        left = Instantiate(selectWeapon[0], leftPosition.transform.position, Quaternion.identity, leftPosition.transform);
        mid = Instantiate(selectWeapon[1], midPosition.transform.position, Quaternion.identity, midPosition.transform);
        right = Instantiate(selectWeapon[2], rightPosition.transform.position, Quaternion.identity, rightPosition.transform);
        ani.enabled = false;
    }

    public void WeapSelect() 
    {
        // 打开选择面板前清空旧引用，重新生成三个选项，并暂停游戏。
        //selectWeapon = weaponList;
        left = null;
        right = null;
        mid = null;
        GenerateSelect();
        box.SetActive(true);
        Time.timeScale = 0f;
    }
    #region Check
    public void CheckLeft() //Left button check event
    {
        // 左侧卡被选中，执行该卡的 LevelUp。
        left.GetComponent<WeaponSelect>().LevelUp();
        Time.timeScale = 1f;
        box.SetActive(false);
        ani.enabled = true;
        FinishSelect();
        
    }
    public void CheckMid()//Mid button check event
    {
        // 中间卡被选中。
        mid.GetComponent<WeaponSelect>().LevelUp();
        Time.timeScale = 1f;
        box.SetActive(false);
        ani.enabled = true;
        FinishSelect();
        
    }
    public void CheckRight()//Right button check event
    {
        // 右侧卡被选中。
        right.GetComponent<WeaponSelect>().LevelUp();
        Time.timeScale = 1f;
        box.SetActive(false);
        ani.enabled = true;
        FinishSelect();

    }
    #endregion
    public void FinishSelect() //Clear Weapon Selection when finish selecting
    {
        // 选择结束后销毁三张临时生成的卡。
        Destroy(left);
        Destroy(mid);
        Destroy(right);
    }
    public void LevelMaxRemove(string name) //Use name to remove weapon when its level reach max
    {
        // 武器满级后从可选池移除，避免后续升级再次抽到。
        selectWeapon.RemoveAll(obj =>obj.name == name);
    }

}
