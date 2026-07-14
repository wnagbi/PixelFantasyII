using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;
using static Cinemachine.DocumentationSortingAttribute;

// 龙卷风实体。
// 在玩家周围随机移动，进入范围的敌人被减速，停留时持续受到伤害。
public class Tornado : MonoBehaviour
{
    private TornadoController weapon;
    private float startTime;
    private float distance;
    private Vector3 originPos;
    private Vector3 newPos;
    private bool isAttack;
    
    private void Start()
    {
        // 找到龙卷风控制器，用它的 speed、damage、moveRange 等数值。
        weapon = FindObjectOfType<TornadoController>();
    }
    private void Update()
    {
        // 没有目标点时选择新目标；有目标点时向目标点插值移动。
        if (!isAttack)
            Attack();
        else
        {
            float dist = (Time.time - startTime) * weapon.speed;
            float factor = dist / distance;
            transform.position = Vector3.Lerp(originPos, newPos, factor);
            if (transform.position == newPos) 
            {
                // 到达目标点后，下帧重新选择新的随机目标。
                isAttack = false;
                originPos = transform.position;
                //Debug.Log("切换目标");
            }

        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 进入龙卷风范围时减速。
            collision.GetComponent<Enemy>().enemySpeed /= 2;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 停留期间持续造成伤害。
            collision.GetComponent<Enemy>().GetDamage(weapon.damage + PlayerData.getInstance().ExtraDamge/10);
            DamageNumberController.instance.SpawnDamage(weapon.damage + PlayerData.getInstance().ExtraDamge / 10, collision.transform.position);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 离开范围时恢复速度。
            collision.GetComponent<Enemy>().enemySpeed *= 2;
        }
    }
    private void Attack() 
    {
        // 选择玩家附近 moveRange 范围内的随机点作为下一段移动目标。
        
        startTime = Time.time;
        float newX = weapon.transform.position.x + Random.Range(-weapon.moveRange, weapon.moveRange);
        float newY = weapon.transform.position.y + Random.Range(-weapon.moveRange, weapon.moveRange);
        newPos = new Vector3(newX, newY, 0);
        distance = Vector3.Distance(originPos, new Vector3(newX, newY, 0));
        isAttack = true;
    }

}
