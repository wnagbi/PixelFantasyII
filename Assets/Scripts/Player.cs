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

// Main player controller: movement, state machine and damage entry.
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

    public void TransitionState(PlayerStateType type)
    {
        currentState?.OnExit();
        currentState = states[type];
        currentState.OnEnter();
    }

    public Animator GetAni()
    {
        return ani;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        inputValue = ctx.ReadValue<Vector2>();
    }

    public void Move()
    {
        isRuning = inputValue.magnitude > 0.0001f;
        ani.SetFloat("LookX", inputValue.x);
        ani.SetFloat("LookY", inputValue.y);
        rig.velocity = inputValue * PlayerData.getInstance().CurrentSpeed;
    }

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

    public void PlayerHurt()
    {
        isHurt = true;
        Controller.instance.StartVibration(1f, 1f, 0.5f);
        FlashColor(0.5f);
    }

    private void FlashColor(float time)
    {
        sr.material.color = Color.red;
        Invoke(nameof(ResetColor), time);
    }

    private void ResetColor()
    {
        sr.material.color = originColor;
    }
}
