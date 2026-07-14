using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 环绕刀实体。
// 负责在碰到敌人时造成 KnifeController 当前伤害。
public class Knife : MonoBehaviour
{
    KnifeController weapon;
    private void Start()
    {
        // 刀是 rotationPoint 的子物体，所以通过 parent.parent 找到 KnifeController。
        weapon = transform.parent.parent.GetComponent<KnifeController>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Enemy")) 
        {
            // 造成伤害并生成飘字。
            collision.GetComponent<Enemy>().GetDamage(weapon.damage+ PlayerData.getInstance().ExtraDamge);
            DamageNumberController.instance.SpawnDamage(weapon.damage + PlayerData.getInstance().ExtraDamge,collision.transform.position);
        }
    }
}
