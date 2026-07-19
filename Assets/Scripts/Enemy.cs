using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EnemyStateType
{
    Idle,
    Move,
    Attack,
    Hurt,
    Die
}

// 敌人运行时核心组件。
// 统一管理属性、C#/Lua 状态机、受伤死亡入口以及对象池启用/回收生命周期。
public class Enemy : MonoBehaviour
{
    public bool useLuaStateMachine = true;
    public float enemySpeed;
    public float colliderDamage;
    public float colliderDisntance;
    public LayerMask playerMask;
    public float Health;
    public float maxHealht;

    public AudioClip vfDie;
    public UnityEvent OnHurt;
    public UnityEvent OnDie;

    public bool isHurt;
    public bool isDie;
    [HideInInspector] public Animator ani;
    public bool live;

    private PickUpGenerator pickUpGenerator;
    private Rigidbody2D rig;
    private SpriteRenderer sr;
    private Transform target;
    private Color originColor;
    private IState currentState;
    private float originSpeed;
    private readonly Dictionary<EnemyStateType, IState> states = new Dictionary<EnemyStateType, IState>();

    private void Awake()
    {
        maxHealht = Health;

        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        pickUpGenerator = GetComponent<PickUpGenerator>();
        originColor = sr.color;
        originSpeed = enemySpeed;

        if (useLuaStateMachine)
        {
            UseLuaStates();
        }
        else
        {
            UseDefaultStates();
        }

        TransitionState(EnemyStateType.Move);
    }

    private void OnEnable()
    {
        live = true;
        enemySpeed = originSpeed;
        Health = maxHealht;
        isDie = false;
        isHurt = false;
        sr.color = originColor;
        TransitionState(EnemyStateType.Move);

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.AddEnemy(this);
        }
    }

    private void Update()
    {
        currentState.OnUpData();
    }

    private void FixedUpdate()
    {
        currentState.OnFixUpData();
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RemoveEnemy(this);
        }
    }

    /// <summary>
    /// 退出当前状态并进入指定的新状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TransitionState(EnemyStateType type)
    {
        if (!states.ContainsKey(type))
        {
            UseDefaultStates();
        }

        currentState?.OnExit();
        currentState = states[type];
        currentState.OnEnter();
    }

    /// <summary>
    /// 取得当前注册玩家并驱动敌人向玩家追踪移动。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ChasePlayer()
    {
        if (target == null && !PlayerRuntimeRegistry.TryGetPlayerTransform(out target))
        {
            rig.velocity = Vector2.zero;
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        sr.flipX = dir.x > 0f;
        rig.velocity = dir * enemySpeed;
    }

    /// <summary>
    /// 兼容旧入口：旧武器、Lua 或 UnityEvent 仍然可以调用 enemy:GetDamage(value)。 新流程会立刻转给 DamageSystem，避免这里继续写伤害公式。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void GetDamage(float damage)
    {
        // 兼容旧入口：旧武器、Lua 或 UnityEvent 仍然可以调用 enemy:GetDamage(value)。
        // 新流程会立刻转给 DamageSystem，避免这里继续写伤害公式。
        DamageSystem.ApplyToEnemy(new DamageContext
        {
            attacker = null,
            target = gameObject,
            hitPoint = transform.position,
            baseDamage = damage,
            bonusDamage = 0f,
            multiplier = 1f,
            targetType = DamageTargetType.Enemy,
            sourceType = DamageSourceType.Debug,
            showDamageNumber = false,
            ignoreDefense = false,
            instantKill = false
        });
    }

    /// <summary>
    /// 只有 DamageSystem 应该调用这个方法。 Enemy 自己只负责扣血、触发受伤/死亡事件，不再负责计算最终伤害。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public DamageResult ApplyDamageFromSystem(float finalDamage)
    {
        // 只有 DamageSystem 应该调用这个方法。
        // Enemy 自己只负责扣血、触发受伤/死亡事件，不再负责计算最终伤害。
        if (finalDamage <= 0f || isDie)
        {
            return new DamageResult { finalDamage = finalDamage, applied = false, killed = isDie };
        }

        Health -= finalDamage;
        OnHurt?.Invoke();
        if (Health <= 0f)
        {
            OnDie?.Invoke();
        }

        return new DamageResult
        {
            finalDamage = finalDamage,
            applied = true,
            killed = Health <= 0f
        };
    }

    /// <summary>
    /// 临时切换受击颜色并安排恢复。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void FlashColor(float time)
    {
        sr.material.color = Color.red;
        Invoke(nameof(ResetColor), time);
    }

    /// <summary>
    /// 把受击闪色恢复为对象原始颜色。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Enemy 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void ResetColor()
    {
        sr.material.color = originColor;
        isHurt = false;
    }

    /// <summary>
    /// 设置敌人受伤状态并触发对应状态机切换。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void EnemeyHurt()
    {
        isHurt = true;
        AudioController.instance.PlaySE(vfDie);
    }

    /// <summary>
    /// 把敌人标记为死亡并切换到死亡状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void EnemyDie()
    {
        isDie = true;
        live = false;
    }

    /// <summary>
    /// 优先调用 Lua 敌人死亡规则，未处理时执行 C# 默认销毁流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void EnemyDestroy()
    {
        if (LuaConfig.TryCallBool("hotfix.enemy.enemy_state", "OnEnemyDestroy", this, 0, out bool handled) && handled)
        {
            return;
        }

        DefaultEnemyDestroy();
    }

    /// <summary>
    /// 执行默认击杀计数、掉落生成和对象池回收流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void DefaultEnemyDestroy()
    {
        RunData.AddKill();
        pickUpGenerator.DropItems();
        isDie = true;
        ObjPoolManager.instance.ReturnObj(gameObject);
    }

    /// <summary>
    /// 为敌人创建并启用 C# 默认状态机实现。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void UseDefaultStates()
    {
        states.Clear();
        states.Add(EnemyStateType.Idle, new EnemyIdleState(this));
        states.Add(EnemyStateType.Move, new EnemyMoveState(this));
        states.Add(EnemyStateType.Attack, new EnemyAttackState(this));
        states.Add(EnemyStateType.Hurt, new EnemyHurtState(this));
        states.Add(EnemyStateType.Die, new EnemyDieState(this));
    }

    /// <summary>
    /// 为敌人创建并启用由 LuaState 转发的热更新状态机。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void UseLuaStates()
    {
        states.Clear();
        states.Add(EnemyStateType.Idle, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyIdle"));
        states.Add(EnemyStateType.Move, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyMove"));
        states.Add(EnemyStateType.Attack, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyAttack"));
        states.Add(EnemyStateType.Hurt, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyHurt"));
        states.Add(EnemyStateType.Die, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyDie"));
    }

    /// <summary>
    /// 检测敌人攻击范围并通过 DamageSystem 对玩家造成伤害。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Enemy 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ColliderAttack()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, colliderDisntance, playerMask);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player") && hitCollider.TryGetComponent(out Player player))
            {
                // 敌人范围攻击玩家，也统一交给 DamageSystem。
                DamageSystem.ApplyToPlayer(
                    DamageSystem.CreateEnemyContactDamage(this, player, hitCollider.transform.position, colliderDamage)
                );
            }
        }
    }

    /// <summary>
    /// 处理当前对象停留在二维触发器内时的持续交互。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Unity 按生命周期或消息规则自动调用，不要从普通业务代码直接调用。
    /// </remarks>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.TryGetComponent(out Player player))
        {
            // 敌人碰撞玩家时不直接调用 Player.GetDamage，避免绕过统一伤害规则。
            DamageSystem.ApplyToPlayer(
                DamageSystem.CreateEnemyContactDamage(this, player, collision.transform.position, colliderDamage)
            );
        }
    }
}
