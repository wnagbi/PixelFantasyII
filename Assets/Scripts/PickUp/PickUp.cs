using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 拾取物类型。
public enum PickUpTpye 
{
    Exp, Blood
}
// 单个拾取物行为。
// 负责吸附玩家、碰撞拾取、增加经验或回复生命，并支持 Lua 配置拾取距离/速度/数值。
public class PickUp : MonoBehaviour
{
    public const int MaxActiveBloodPickups = 5;
    public static int ActiveBloodPickups { get; private set; }

    public AudioClip a;
    public PickUpTpye pickUpTpye;
    public float value;
    public float potionHealing;

    public float pickUpDistance;
    public float moveSpeed;

    private float originDistance;
    private float originSpeed;
    public Transform player;
    private bool missingPlayerWarningShown;
    private bool countedAsActiveBlood;
    private void Awake()
    {
        originDistance = pickUpDistance;
        originSpeed = moveSpeed;
    }
    private void OnEnable()
    {
        if (pickUpTpye == PickUpTpye.Blood && !countedAsActiveBlood)
        {
            ActiveBloodPickups++;
            countedAsActiveBlood = true;
        }

        pickUpDistance = originDistance;
        moveSpeed = originSpeed;
        if (pickUpTpye == PickUpTpye.Exp) 
        {
            float scale = Random.Range(1, 2f);
            transform.localScale = new Vector3(scale,scale,scale);
        }

        // 拾取物每次从对象池启用时，从 Lua 配置刷新吸附距离和移动速度。
        // 这样不同类型拾取物可以在不改 C# 的情况下调整手感。
        float luaDistance = LuaConfig.GetFloat("config.pickup_config", pickUpTpye.ToString(), "pickup_distance", pickUpDistance);
        float luaSpeed = LuaConfig.GetFloat("config.pickup_config", pickUpTpye.ToString(), "move_speed", moveSpeed);
        pickUpDistance = luaDistance;
        moveSpeed = luaSpeed;
    }

    private void OnDisable()
    {
        if (countedAsActiveBlood)
        {
            ActiveBloodPickups = Mathf.Max(0, ActiveBloodPickups - 1);
            countedAsActiveBlood = false;
        }
    }

    /// <summary>
    /// 判断当前条件是否允许执行 CanSpawnBloodPickup 对应的操作。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 PickUp 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public static bool CanSpawnBloodPickup()
    {
        return ActiveBloodPickups < MaxActiveBloodPickups;
    }

    private void Start()
    {
        TryBindPlayer();
    }
    private void Update()
    {
        if (player == null && !TryBindPlayer())
        {
            return;
        }

        if(Vector2.Distance(transform.position,player.position) < pickUpDistance) 
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.Translate (dir * moveSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// 处理当前对象进入二维触发器时的交互逻辑。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Unity 按生命周期或消息规则自动调用，不要从普通业务代码直接调用。
    /// </remarks>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            CollectPickUp(collision);
        }
    }
    /// <summary>
    /// 根据拾取物类型应用经验、回血或其它收集效果并回收对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 PickUp 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void CollectPickUp(Collider2D collision) 
    {
        switch (pickUpTpye)
        {
            case PickUpTpye.Exp:
                AudioController.instance.PlaySE(a);
                float expValue = value;

                // 经验值交给 Lua 修正，例如按关卡时间、倍率 Buff、难度调整。
                if (LuaConfig.TryCallFloat("hotfix.pickup.pickup_rule", "GetExpValue", this, value, out float luaExpValue))
                {
                    expValue = luaExpValue;
                }
                PlayerData.getInstance().Exp += Mathf.RoundToInt(expValue);
                //Debug.Log("Exp: "+ PlayerData.getInstance().Exp);          
                break;
            case PickUpTpye.Blood:
                if (PlayerData.getInstance().CurrentHealth == PlayerData.getInstance().CurrentMaxHealth)
                    return;
                float healValue = potionHealing;

                // 回复量交给 Lua 修正，Lua 失败时使用 Inspector 中的 potionHealing。
                if (LuaConfig.TryCallFloat("hotfix.pickup.pickup_rule", "GetHealValue", this, potionHealing, out float luaHealValue))
                {
                    healValue = luaHealValue;
                }
                PlayerData.getInstance().AddHealth(healValue);
                break;


        }
        ObjPoolManager.instance.ReturnObj(gameObject);

    }

    /// <summary>
    /// 尝试执行 TryBindPlayer，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 PickUp 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private bool TryBindPlayer()
    {
        if (PlayerRuntimeRegistry.TryGetPlayerTransform(out player))
        {
            return true;
        }

        if (!missingPlayerWarningShown)
        {
            Debug.LogWarning("[PickUp] Player is not registered yet. Pickup movement will wait.", this);
            missingPlayerWarningShown = true;
        }

        return false;
    }
}
