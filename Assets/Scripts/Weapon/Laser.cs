using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 浮游炮发射出的激光实体。
// 负责短暂存在、碰撞敌人并按 FunnelController 当前伤害结算。
public class Laser : MonoBehaviour
{
    private FunnelController weapon;
    private void Start()
    {
        // 找到场景中的浮游炮控制器，用它的 damage 作为伤害来源。
        weapon = FindObjectOfType<FunnelController>();
    }
    private void OnEnable()
    {
        // 激光只存在很短时间，避免一直挂在浮游炮上造成持续碰撞。
        Invoke("LaserDestory", 0.5f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) 
        {
            // 伤害 = 武器基础伤害 + 玩家临时额外伤害。
            collision.GetComponent<Enemy>().GetDamage(weapon.damage + PlayerData.getInstance().ExtraDamge);
            DamageNumberController.instance.SpawnDamage(weapon.damage + PlayerData.getInstance().ExtraDamge, collision.transform.position);
        }
    }

    public void LaserDestory()
    {
        // 当前激光不是对象池对象，直接销毁。
        Destroy(gameObject);
    }
}
