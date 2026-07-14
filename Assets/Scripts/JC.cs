using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 次元斩/全屏斩击类技能效果。
// 生成后暂停时间播放粒子，结束时对范围内敌人造成高额伤害，然后恢复时间并销毁自身。
public class JC : MonoBehaviour
{
    public ParticleSystem particle;
    private Collider2D col;

    [Header("时间控制")]
    [SerializeField] private float timeFreezeDuration = 2f; // 时间暂停总时长

    void Awake()
    {
        // 配置粒子不受时间缩放影响
        var main = particle.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true; // 关键设置！

        col = GetComponent<Collider2D>();
        col.enabled = true;
    }

    void Start()
    {
        StartCoroutine(TimeFreezeRoutine());
    }

    IEnumerator TimeFreezeRoutine()
    {
        // 暂停游戏时间
        Time.timeScale = 0f;

        // 使用真实时间检测粒子播放
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < timeFreezeDuration)
        {
            // 持续造成伤害（使用真实时间间隔）
            
            yield return new WaitForSecondsRealtime(0.1f); // 每0.1秒检测一次
        }

        // 恢复时间
        Time.timeScale = 1f;
        DealDamageToEnemies();
        SkillController.Instance.RestoreFilter();
        Destroy(gameObject);
    }

    void DealDamageToEnemies()
    {
        // 查询当前碰撞器重叠到的敌人。
        ContactFilter2D filter = new ContactFilter2D();
        Collider2D[] results = new Collider2D[10];

        int count = col.OverlapCollider(filter, results);
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = results[i].GetComponent<Enemy>();
            if (enemy != null)
            {
                // 直接秒杀
                // 这里通过 Enemy.GetDamage 走正常受伤/死亡流程，而不是直接销毁敌人。
                enemy.GetDamage(9999);
                DamageNumberController.instance.SpawnDamage(
                    9999,
                    enemy.transform.position
                );
            }
        }
    }
}

