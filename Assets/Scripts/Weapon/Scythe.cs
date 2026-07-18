using System.Collections;
using UnityEngine;

// 镰刀飞行实体。负责移动表现；命中后的伤害结算交给 DamageSystem。
public class Scythe : MonoBehaviour
{
    public GameObject dirPoint;

    private bool finishTimer;
    private ScytheController weapon;

    private void Start()
    {
        // dirPoint 是整个投射物的移动根节点，Controller 位于其父级层级。
        weapon = dirPoint.transform.parent.parent.GetComponent<ScytheController>();
        if (weapon != null)
        {
            StartCoroutine(Timer(weapon.returnTimer()));
        }
    }

    private void Update()
    {
        if (weapon == null || dirPoint == null)
        {
            return;
        }

        // 根节点向发射方向移动，镰刀子物体同时自转形成飞行表现。
        dirPoint.transform.Translate(weapon.speed * Vector3.right * Time.deltaTime);
        transform.Rotate(-Vector3.forward * weapon.turnSpeed * Time.deltaTime, Space.Self);
        if (finishTimer)
        {
            Destroy(dirPoint);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || weapon == null || !collision.TryGetComponent(out Enemy enemy))
        {
            return;
        }

        // 不在武器脚本里直接扣血或生成伤害数字，避免每个武器重复写同一套公式。
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

    private IEnumerator Timer(float timer)
    {
        // 飞行时间结束后让 Update 销毁整个方向根节点。
        yield return new WaitForSeconds(timer);
        finishTimer = true;
    }
}
