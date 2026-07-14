using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 导弹落点/导弹动画实体。
// 负责按 MissileController 的 timer 调整动画速度，Fire 后开启碰撞并造成伤害。
public class Missile : MonoBehaviour
{
    public AnimationClip targetAnimation;
    public AnimationClip missileAnimation;
    public GameObject missile;


    private Animator missileAni;
    private Animator ani;
    private float originSpeed;
    private CapsuleCollider2D col;
    private MissileController weapon;
    private SpriteRenderer sr;
    private Color originColor;
    private Color targetColor;
    private float startTime;
    
    private void Start()
    {
        // 缓存动画、碰撞、渲染等组件。
        ani = GetComponent<Animator>();
        missileAni = missile.GetComponent<Animator>();
        originSpeed = ani.speed;
        col = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        originColor = sr.color;
        targetColor = Color.red;
        weapon = FindObjectOfType<MissileController>();
        startTime = Time.time;


        // 根据武器 timer 调整目标圈和导弹动画播放速度，让动画时长和爆炸时间一致。
        float animationLength = targetAnimation.length;
        ani.speed = animationLength/weapon.timer;
        float missileAnimationLength = missileAnimation.length;
        missileAni.speed = missileAnimationLength / weapon.timer;
        
    }
    private void Update()
    {
        //Aiming();

    }


    public void SetAnimationDuration(float newDuration) 
    {
        // 暴露给动画事件或外部逻辑：动态调整落点动画时长。
        float animationLength = targetAnimation.length;
        ani.speed = animationLength / newDuration;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 导弹碰到敌人时造成当前导弹伤害。
            collision.GetComponent<Enemy>().GetDamage(weapon.damage + PlayerData.getInstance().ExtraDamge);
            DamageNumberController.instance.SpawnDamage(weapon.damage + PlayerData.getInstance().ExtraDamge, collision.transform.position);
        }
    }
    public void Fire() 
    {
        // 动画事件调用：真正落下时开启碰撞，并短暂延迟后销毁。
        col.enabled = true;
        Invoke("MissileDestroy", 0.1f);

    }

    private void MissileDestroy() 
    {
        // 当前导弹不是对象池对象，直接销毁。
        Destroy(gameObject);
    }
}
