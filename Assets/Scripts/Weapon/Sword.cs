using UnityEngine;

// 飞剑实体。
// 保存当前锁定的 enemy，碰撞目标敌人时造成秒杀，碰到其它敌人则造成普通伤害。
public class Sword : MonoBehaviour
{

    public Transform enemy;
    
    public SwordController weapon;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) 
        {
            // 如果命中的是当前锁定目标，按目标最大生命直接造成致命伤害。
            if (enemy != null && collision.transform == enemy)
            {
                float damage = collision.GetComponent<Enemy>().maxHealht;
                collision.GetComponent<Enemy>().GetDamage(damage);
                DamageNumberController.instance.SpawnDamage(damage, collision.transform.position);


            }
            else
            {
                // 非锁定目标只造成飞剑普通伤害。
                collision.GetComponent<Enemy>().GetDamage(weapon.damage + PlayerData.getInstance().ExtraDamge);
                DamageNumberController.instance.SpawnDamage(weapon.damage + PlayerData.getInstance().ExtraDamge, collision.transform.position);
            }

        }
    }
    private void Start()
    {
        // 生成时从 EnemyManager 当前存活敌人中随机选择一个目标。
        Enemy target = EnemyManager.Instance != null ? EnemyManager.Instance.GetRandomEnemy() : null;
        enemy = target != null ? target.transform : null;
        weapon = FindObjectOfType<SwordController>();

    }
    
}
