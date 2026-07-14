using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 镰刀飞行实体。
// 负责沿 dirPoint 方向飞出并旋转，生命周期结束后销毁发射点。
public class Scythe : MonoBehaviour
{
    public GameObject dirPoint;
    bool finishTimer;
    ScytheController weapon;
    void Start()
    {
        // 通过层级找到 ScytheController，并按武器 timer 启动生命周期。
        weapon = dirPoint.transform.parent.parent.GetComponent<ScytheController>();
        StartCoroutine(Timer(weapon.returnTimer()));
        //Debug.Log("生成武器");
    }
    private void Update()
    {
        // dirPoint 向右移动，镰刀自身持续旋转。
        
        dirPoint.transform.Translate(weapon.speed * Vector3.right * Time.deltaTime);
        transform.Rotate(-Vector3.forward * weapon.turnSpeed * Time.deltaTime, Space.Self);
        if (finishTimer) 
        {
            // 生命周期结束后销毁 dirPoint，连带清理子物体。
            //Debug.Log("yes");
            Destroy(dirPoint);
        }

        
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) 
        {
            // 命中敌人时按当前武器伤害结算。
            collision.GetComponent<Enemy>().GetDamage(weapon.damage + PlayerData.getInstance().ExtraDamge);
            DamageNumberController.instance.SpawnDamage(weapon.damage + PlayerData.getInstance().ExtraDamge, collision.transform.position);
        }
    }
    IEnumerator Timer(float timer) 
    {
        // 按秒等待，再处理小数部分，最后标记生命周期结束。
        for (int i = 0; i < timer; i++) 
        {
            yield return new WaitForSeconds(1);
            //Debug.Log(i);
        }
        float remainTime = timer - (int)timer;
        if (remainTime != 0) 
        {
            
            yield return new WaitForSeconds(remainTime);
            //Debug.Log(timer);
            
        }
        finishTimer = true;
    }



}
