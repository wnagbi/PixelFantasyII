using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum PlayerStateType
{
    Idle,
    Move,
    Hurt,
    Die
}

// 玩家运行时核心组件。
// 负责输入移动、状态机切换、受伤死亡入口，并向 PlayerRuntimeRegistry 注册当前玩家。
public class Player : MonoBehaviour
{
    [Header("Player")]
    public float curMaxHealth;
    public float curHealth;
    public float curSpeed;
    public UnityEvent onHurt;
    public UnityEvent onDie;

    public GameObject pickUpSet;
    [HideInInspector] public bool isRuning;
    [HideInInspector] public bool isHurt;
    [HideInInspector] public Animator ani;

    private Color originColor;
    private Vector2 inputValue;
    private Rigidbody2D rig;
    private SpriteRenderer sr;
    private IState currentState;
    private readonly Dictionary<PlayerStateType, IState> states = new Dictionary<PlayerStateType, IState>();

    private void Awake()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        PlayerData.getInstance().CurrentMaxHealth = curMaxHealth;
        PlayerData.getInstance().CurrentHealth = curHealth;
        PlayerData.getInstance().CurrentSpeed = curSpeed;

        originColor = sr.color;
        states.Add(PlayerStateType.Idle, new PlayerIdleState(this));
        states.Add(PlayerStateType.Move, new PlayerMoveState(this));
        states.Add(PlayerStateType.Hurt, new PlayerHurtState(this));
        states.Add(PlayerStateType.Die, new PlayerDieState(this));
        TransitionState(PlayerStateType.Idle);
    }

    private void OnEnable()
    {
        PlayerRuntimeRegistry.Register(this);
    }

    private void OnDisable()
    {
        PlayerRuntimeRegistry.Unregister(this);
    }

    private void Update()
    {
        Move();
        currentState.OnUpData();
        curMaxHealth = PlayerData.getInstance().CurrentMaxHealth;
        curHealth = PlayerData.getInstance().CurrentHealth;
        curSpeed = PlayerData.getInstance().CurrentSpeed;
    }

    private void FixedUpdate()
    {
        currentState.OnFixUpData();
    }

    /// <summary>
    /// 退出当前状态并进入指定的新状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TransitionState(PlayerStateType type)
    {
        currentState?.OnExit();
        currentState = states[type];
        currentState.OnEnter();
    }

    /// <summary>
    /// 返回玩家 Animator，供暂停和武器选择流程控制动画。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public Animator GetAni()
    {
        return ani;
    }

    /// <summary>
    /// 接收 Input System 移动回调并保存最新输入向量。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        inputValue = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// 根据输入向量和玩家速度更新 Rigidbody2D 速度。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Move()
    {
        isRuning = inputValue.magnitude > 0.0001f;
        ani.SetFloat("LookX", inputValue.x);
        ani.SetFloat("LookY", inputValue.y);
        rig.velocity = inputValue * PlayerData.getInstance().CurrentSpeed;
    }

    /// <summary>
    /// 触发全场拾取逻辑，用于磁铁技能收集掉落物。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void PickUpAllItem()
    {
        for (int i = 0; i < pickUpSet.transform.childCount; i++)
        {
            if (pickUpSet.transform.GetChild(i).gameObject.activeSelf)
            {
                PickUp pickUp = pickUpSet.transform.GetChild(i).GetComponent<PickUp>();
                pickUp.pickUpDistance = 1000000;
                pickUp.moveSpeed *= 4;
            }
        }
    }

    /// <summary>
    /// 兼容旧入口：外部仍可调用 player.GetDamage(value)。 实际伤害计算、Lua 修正和扣血流程都统一交给 DamageSystem。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void GetDamage(float damage)
    {
        // 兼容旧入口：外部仍可调用 player.GetDamage(value)。
        // 实际伤害计算、Lua 修正和扣血流程都统一交给 DamageSystem。
        DamageSystem.ApplyToPlayer(new DamageContext
        {
            attacker = null,
            target = gameObject,
            hitPoint = transform.position,
            baseDamage = damage,
            bonusDamage = 0f,
            multiplier = 1f,
            targetType = DamageTargetType.Player,
            sourceType = DamageSourceType.Debug,
            showDamageNumber = false,
            ignoreDefense = false,
            instantKill = false
        });
    }

    /// <summary>
    /// 只有 DamageSystem 应该调用这个方法。 Player 自己只负责扣血和触发受伤/死亡事件，不再负责伤害公式。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public DamageResult ApplyDamageFromSystem(float finalDamage)
    {
        // 只有 DamageSystem 应该调用这个方法。
        // Player 自己只负责扣血和触发受伤/死亡事件，不再负责伤害公式。
        if (finalDamage <= 0f)
        {
            return new DamageResult
            {
                finalDamage = finalDamage,
                applied = false,
                killed = PlayerData.getInstance().CurrentHealth <= 0f
            };
        }

        PlayerData.getInstance().TakeDamage(finalDamage);
        onHurt?.Invoke();
        if (PlayerData.getInstance().CurrentHealth <= 0f)
        {
            onDie?.Invoke();
        }

        return new DamageResult
        {
            finalDamage = finalDamage,
            applied = true,
            killed = PlayerData.getInstance().CurrentHealth <= 0f
        };
    }

    /// <summary>
    /// 把玩家切换到受伤状态并触发震动反馈。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Player 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void PlayerHurt()
    {
        isHurt = true;
        Controller.instance.StartVibration(1f, 1f, 0.5f);
        FlashColor(0.5f);
    }

    /// <summary>
    /// 临时切换受击颜色并安排恢复。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Player 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void FlashColor(float time)
    {
        sr.material.color = Color.red;
        Invoke(nameof(ResetColor), time);
    }

    /// <summary>
    /// 把受击闪色恢复为对象原始颜色。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Player 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void ResetColor()
    {
        sr.material.color = originColor;
    }
}
