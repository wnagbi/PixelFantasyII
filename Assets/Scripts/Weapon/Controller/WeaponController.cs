using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Weapon))]
// 所有武器控制器的基础类。
// 管理通用武器数值、冷却计时和攻击入口；具体武器通过 override Attack/Refresh/CDTime 扩展行为。
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Stats")]
    [HideInInspector]public int level;
    public GameObject prefab;
    public float damage;
    public int count;
    public float speed;
    public float turnSpeed;
    public float timer;
    public float cooldownDuration;
    [HideInInspector]public float currentCooldown;
    [HideInInspector]public Weapon weapon;

    protected virtual void Start()
    {
        // 子类 Start 通常先调用 base.Start()，确保 weapon 和冷却初始化完成。
        InitializeWeapon();
    }

    /// <summary>
    /// 读取同物体上的 Weapon 数据组件，并把当前冷却设为初始 CD。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 WeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected void InitializeWeapon()
    {
        // 读取同物体上的 Weapon 数据组件，并把当前冷却设为初始 CD。
        currentCooldown =cooldownDuration;
        weapon = GetComponent<Weapon>();
    }
    protected virtual void Update() 
    {
        // 同步 Weapon 数据组件里的等级，供 UI/选择界面显示。
        weapon.weaponLevel = level;
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
            Attack();
        else
            CDTime();

    }
    /// <summary>
    /// 冷却期间的行为钩子。环绕类武器会在这里旋转。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 WeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected virtual void CDTime() 
    {
        // 冷却期间的行为钩子。环绕类武器会在这里旋转。
    }
    /// <summary>
    /// 根据最新数据刷新 WeaponController 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 WeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected virtual void Refresh() 
    {
        // 数量/等级变化后重建武器表现的钩子。
    }
    /// <summary>
    /// 默认攻击只重置冷却；具体武器负责真正生成子弹或执行攻击。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 WeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected virtual void Attack() 
    {
        // 默认攻击只重置冷却；具体武器负责真正生成子弹或执行攻击。
        currentCooldown = cooldownDuration;
    }
    protected virtual void OnEnable()
    {
        // 武器第一次获得/启用时等级加一。
        level++;
    }
  
    
}
