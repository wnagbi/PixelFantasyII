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
                // 次元斩属于技能伤害，这里用 instantKill 表达秒杀语义。
                // 这样仍会走敌人的受伤/死亡事件，而不是直接销毁敌人对象。
                DamageContext context = new DamageContext
                {
                    attacker = gameObject,
                    target = enemy.gameObject,
                    hitPoint = enemy.transform.position,
                    baseDamage = 0f,
                    bonusDamage = 0f,
                    multiplier = 1f,
                    targetType = DamageTargetType.Enemy,
                    sourceType = DamageSourceType.Skill,
                    showDamageNumber = true,
                    ignoreDefense = true,
                    instantKill = true
                };
                DamageSystem.ApplyToEnemy(context);
            }
        }
    }
}

