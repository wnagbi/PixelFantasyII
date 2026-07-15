using UnityEngine;

// 环绕刀实体。命中敌人后不直接扣血，而是把伤害交给 DamageSystem。
public class Knife : MonoBehaviour
{
    private KnifeController weapon;

    private void Start()
    {
        weapon = transform.parent.parent.GetComponent<KnifeController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || weapon == null || !collision.TryGetComponent(out Enemy enemy))
        {
            return;
        }

        // 武器伤害统一走 DamageSystem：基础伤害来自武器，额外伤害来自玩家 Buff。
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
}
