using UnityEngine;

// 浮游炮生成的短生命周期激光。命中后只提交伤害上下文，不直接扣血。
public class Laser : MonoBehaviour
{
    private FunnelController weapon;

    public void Init(FunnelController owner)
    {
        // 激光由 FunnelController 生成时注入 owner，避免运行时查找控制器。
        weapon = owner;
    }

    private void OnEnable()
    {
        // 每次生成后只存在短暂时间；当前激光不是对象池对象，因此到时直接销毁。
        Invoke(nameof(LaserDestory), 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
        {
            return;
        }

        if (weapon == null)
        {
            Debug.LogWarning("[Laser] Owner controller is missing. Damage skipped.", this);
            return;
        }

        if (!collision.TryGetComponent(out Enemy enemy))
        {
            return;
        }

        // 激光只负责告诉 DamageSystem“谁打了谁、基础伤害是多少、命中点在哪”。
        DamageSystem.ApplyToEnemy(
            DamageSystem.CreateWeaponDamage(
                weapon.gameObject,
                enemy,
                collision.transform.position,
                weapon.damage,
                PlayerData.getInstance().ExtraDamge
            )
        );
    }

    public void LaserDestory()
    {
        // 方法保持 public，兼容可能存在的动画事件调用。
        Destroy(gameObject);
    }
}
