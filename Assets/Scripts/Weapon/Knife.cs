using UnityEngine;

// 环绕刀实体。命中敌人后不直接扣血，而是把伤害交给 DamageSystem。
public class Knife : MonoBehaviour
{
    private KnifeController weapon;

    private void Start()
    {
        // 刀实体位于 rotationPoint 子层级，通过父级缓存所属 Controller。
        weapon = transform.parent.parent.GetComponent<KnifeController>();
    }

    /// <summary>
    /// 处理当前对象进入二维触发器时的交互逻辑。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Unity 按生命周期或消息规则自动调用，不要从普通业务代码直接调用。
    /// </remarks>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只处理带 Enemy 标签且确实挂有 Enemy 组件的碰撞对象。
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
