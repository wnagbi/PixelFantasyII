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
        // 缓存落点动画、导弹动画和命中碰撞体。
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

    /// <summary>
    /// 由生成导弹的 Controller 注入数值来源，避免场景扫描。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Missile 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Init(MissileController owner)
    {
        // 由生成导弹的 Controller 注入数值来源，避免场景扫描。
        weapon = owner;
    }

    /// <summary>
    /// 通过调整 Animator.speed 让目标动画适配热更后的落地时间。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Missile 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetAnimationDuration(float newDuration)
    {
        // 通过调整 Animator.speed 让目标动画适配热更后的落地时间。
        if (ani == null || targetAnimation == null || newDuration <= 0f)
        {
            return;
        }

        ani.speed = targetAnimation.length / newDuration;
    }

    /// <summary>
    /// 处理当前对象进入二维触发器时的交互逻辑。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Unity 按生命周期或消息规则自动调用，不要从普通业务代码直接调用。
    /// </remarks>
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

    /// <summary>
    /// 动画事件调用：打开短时间碰撞窗口，随后销毁导弹落点。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Missile 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Fire()
    {
        // 动画事件调用：打开短时间碰撞窗口，随后销毁导弹落点。
        if (col != null)
        {
            col.enabled = true;
        }

        Invoke(nameof(MissileDestroy), 0.1f);
    }

    /// <summary>
    /// 在延迟结束后销毁或回收指定导弹对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Missile 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void MissileDestroy()
    {
        Destroy(gameObject);
    }
}
