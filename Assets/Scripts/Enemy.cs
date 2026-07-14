using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
// 敌人状态类型。Enemy 会用这个枚举从状态字典里取出当前状态。
public enum EnemyStateType 
{
    Idle,Move,Attack,Hurt,Die
}
// 敌人主体控制器。
// 保存敌人数值、组件引用、状态机、受伤/死亡流程，并接入 Lua 热更状态机。
public class Enemy : MonoBehaviour
{
    // 敌人状态机热更开关。
    // true 时 Move/Hurt/Die/Attack 等状态会走 hotfix.enemy.enemy_state.lua；
    // false 时使用下面 UseDefaultStates() 里注册的 C# 状态。
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
    [HideInInspector]public Animator ani;
    private PickUpGenerator pickUpGenerator;
    private Rigidbody2D rig;
    private SpriteRenderer sr;
    private Transform target;
    private Color originColor;
    private IState currentState;
    private float originSpeed;
    public bool live;
    Dictionary<EnemyStateType,IState> states = new Dictionary<EnemyStateType,IState>();


    private void Awake()
    {
        if(FindAnyObjectByType<Player>()!=null)
            target = FindAnyObjectByType<Player>().transform;
        else
            target = FindAnyObjectByType<PlayerNetWrok>().transform;
        maxHealht = Health;
        
        ani = GetComponent<Animator>(); 
        rig = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        pickUpGenerator = GetComponent<PickUpGenerator>();
        originColor = sr.color;
        originSpeed = enemySpeed;

        // 初始化状态表：优先使用 Lua 状态机，方便热更敌人追踪、受伤、死亡等规则。
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
        EnemyManager.Instance.AddEnemy(this);
        //Debug.Log("生命" + Health + "是否死亡"+isDie+"是否受伤" +isHurt);

    }
    public void TransitionState(EnemyStateType type) 
    {
        if (!states.ContainsKey(type))
        {
            UseDefaultStates();
        }

        if (currentState != null)
        {
            currentState.OnExit();
        }
        currentState = states[type];
        currentState.OnEnter();
    }
    private void Update()
    {

        currentState.OnUpData();
    }
    private void FixedUpdate()
    {
        //ColliderAttack();
        currentState.OnFixUpData();
    }
    private void OnDisable()
    {
        EnemyManager.Instance.RemoveEnemy(this);
    }
    public void ChasePlayer()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        //Debug.Log("方向"+ dir);
        if (dir.x > 0)
            sr.flipX = true;
        else
            sr.flipX = false;
        rig.velocity = dir * enemySpeed;
        //Debug.Log("敌人速度" + rig.velocity);
    }

    public void GetDamage(float damage) 
    {
        // 伤害先交给 Lua 修正，例如根据敌人类型、关卡时间、Buff 改变最终扣血。
        // Lua 不存在或报错时，damage 保持 C# 传入的默认值。
        if (LuaConfig.TryCallFloat("hotfix.enemy.enemy_state", "AdjustDamage", this, damage, out float luaDamage))
        {
            damage = luaDamage;
        }

        Health -= damage;
        OnHurt?.Invoke();
        if (Health <= 0)
        { 
            OnDie.Invoke();
        }

    }
    public void FlashColor(float time)
    {
        sr.material.color = Color.red;
        Invoke("ResetColor", time);
    }
    private void ResetColor()
    {
        
        sr.material.color = originColor;
        isHurt = false;

    }

    public void EnemeyHurt() 
    {
        isHurt = true;
        AudioController.instance.PlaySE(vfDie);
    }
    public void EnemyDie() 
    {
        isDie = true;
        live = false;
    }
    public void EnemyDestroy() 
    {
        // 死亡收尾先交给 Lua 判断是否完全接管。
        // Lua 返回 handled=true 时，说明击杀计数、掉落、回收等已经由 Lua/C# 辅助方法处理，这里不再执行默认逻辑。
        if (LuaConfig.TryCallBool("hotfix.enemy.enemy_state", "OnEnemyDestroy", this, 0, out bool handled) && handled)
        {
            return;
        }

        DefaultEnemyDestroy();
    }

    public void DefaultEnemyDestroy()
    {
        // Lua 未接管时的 C# 回退：记录击杀数、执行掉落、回收到对象池。
        int kill = PlayerPrefs.GetInt("KillNum");
        kill++;
        PlayerPrefs.SetInt("KillNum", kill);
        pickUpGenerator.DropItems();
        isDie = false;
        ObjPoolManager.instance.ReturnObj(gameObject);
    }
    public void UseDefaultStates()
    {
        // C# 原始状态机。Lua 模块缺失、调试禁用热更时可以回到这套逻辑。
        states.Clear();
        states.Add(EnemyStateType.Idle, new EnemyIdleState(this));
        states.Add(EnemyStateType.Move, new EnemyMoveState(this));
        states.Add(EnemyStateType.Attack, new EnemyAttackState(this));
        states.Add(EnemyStateType.Hurt, new EnemyHurtState(this));
        states.Add(EnemyStateType.Die, new EnemyDieState(this));
    }

    public void UseLuaStates()
    {
        // Lua 状态机。每个 LuaState 会从 enemy_state.lua 中取对应状态 table。
        states.Clear();
        states.Add(EnemyStateType.Idle, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyIdle"));
        states.Add(EnemyStateType.Move, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyMove"));
        states.Add(EnemyStateType.Attack, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyAttack"));
        states.Add(EnemyStateType.Hurt, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyHurt"));
        states.Add(EnemyStateType.Die, new LuaState(this, "hotfix.enemy.enemy_state", "EnemyDie"));
    }
    public void ColliderAttack() 
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, colliderDisntance, playerMask);
        foreach (Collider2D hitCollider in hitColliders) 
        {
            if (hitCollider.CompareTag("Player")) 
            {
                hitCollider.GetComponent<Player>().GetDamage(colliderDamage);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            collision.GetComponent<Player>().GetDamage(colliderDamage);
        }
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, colliderDisntance);
    //}
}
