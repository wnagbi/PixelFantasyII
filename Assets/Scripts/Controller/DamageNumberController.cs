using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 伤害数字生成器。
// 负责从对象池取出 DamageNumber，并把它显示在受击位置附近。
public class DamageNumberController : MonoBehaviour
{
    public DamageNumber numberToSpawn;
    public Transform numberCanvas;
    public static DamageNumberController instance;
    private void Awake()
    {
        // 敌人/武器等脚本通过 instance 请求生成伤害数字。
        instance = this;
    }
    /// <summary>
    /// 从伤害数字对象池取出实例并设置显示数值和位置。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 DamageNumberController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SpawnDamage(float damageAmount, Vector3 location) 
    {
        // 显示时取整，避免 UI 上出现过长小数。
        int rounded =Mathf.RoundToInt(damageAmount);
        //Instantiate(numberToSpawn, location, Quaternion.identity, numberCanvas)

        // 使用对象池，避免战斗中频繁 Instantiate/Destroy 伤害数字。
        DamageNumber newDamage = ObjPoolManager.instance.GetObj("DamageNumber").GetComponent<DamageNumber>();
        newDamage.transform.position = location + new Vector3(0,1,0);
        newDamage.Setup(rounded);
        newDamage.gameObject.SetActive(true);
    }
}
