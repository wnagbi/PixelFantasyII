using UnityEngine;

// 飞剑实体。生成后由 SwordController 注入 owner，并通过 DamageSystem 结算伤害。
public class Sword : MonoBehaviour
{
    public Transform enemy;
    public SwordController weapon;

    private void Start()
    {
        // 初次启用时随机分配有效敌人；Controller 会在目标失效后继续重新寻敌。
        Enemy target = EnemyManager.Instance != null ? EnemyManager.Instance.GetRandomEnemy() : null;
        enemy = target != null ? target.transform : null;
    }

    public void Init(SwordController owner)
    {
        // 生成时注入 Controller，用于读取伤害等武器数值。
        weapon = owner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || weapon == null || !collision.TryGetComponent(out Enemy targetEnemy))
        {
            return;
        }

        // 先创建普通武器伤害上下文，后面再按“是否命中锁定目标”追加秒杀规则。
        DamageContext context = DamageSystem.CreateWeaponDamage(
            weapon.gameObject,
            targetEnemy,
            collision.transform.position,
            weapon.damage,
            PlayerData.getInstance().ExtraDamge
        );

        if (enemy != null && collision.transform == enemy)
        {
            // 命中锁定目标时不直接写 enemy.GetDamage(maxHealth)，而是让 DamageSystem 按秒杀规则处理。
            context.instantKill = true;
        }

        DamageSystem.ApplyToEnemy(context);
    }
}
