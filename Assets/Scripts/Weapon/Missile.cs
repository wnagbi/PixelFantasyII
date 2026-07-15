using UnityEngine;

// 导弹落点/导弹实体。动画表现留在本脚本，伤害结算交给 DamageSystem。
public class Missile : MonoBehaviour
{
    public AnimationClip targetAnimation;
    public AnimationClip missileAnimation;
    public GameObject missile;

    private Animator missileAni;
    private Animator ani;
    private CapsuleCollider2D col;
    private MissileController weapon;

    private void Start()
    {
        ani = GetComponent<Animator>();
        missileAni = missile != null ? missile.GetComponent<Animator>() : null;
        col = GetComponent<CapsuleCollider2D>();

        if (weapon == null)
        {
            Debug.LogWarning("[Missile] Owner controller is missing. Call Init before the missile starts.", this);
            return;
        }

        if (ani != null && targetAnimation != null && weapon.timer > 0f)
        {
            ani.speed = targetAnimation.length / weapon.timer;
        }

        if (missileAni != null && missileAnimation != null && weapon.timer > 0f)
        {
            missileAni.speed = missileAnimation.length / weapon.timer;
        }
    }

    public void Init(MissileController owner)
    {
        weapon = owner;
    }

    public void SetAnimationDuration(float newDuration)
    {
        if (ani == null || targetAnimation == null || newDuration <= 0f)
        {
            return;
        }

        ani.speed = targetAnimation.length / newDuration;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
        {
            return;
        }

        if (weapon == null)
        {
            Debug.LogWarning("[Missile] Owner controller is missing. Damage skipped.", this);
            return;
        }

        if (!collision.TryGetComponent(out Enemy enemy))
        {
            return;
        }

        // 导弹伤害统一走 DamageSystem，Lua 伤害修正和伤害数字也会在那里处理。
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

    public void Fire()
    {
        if (col != null)
        {
            col.enabled = true;
        }

        Invoke(nameof(MissileDestroy), 0.1f);
    }

    private void MissileDestroy()
    {
        Destroy(gameObject);
    }
}
