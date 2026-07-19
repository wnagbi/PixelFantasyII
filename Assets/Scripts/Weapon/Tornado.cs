using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;
using static Cinemachine.DocumentationSortingAttribute;

// 龙卷风实体。
// 在玩家周围随机移动，进入范围的敌人被减速，停留时持续受到伤害。
public class Tornado : MonoBehaviour
{
    private TornadoController weapon;
    private float startTime;
    private float distance;
    private Vector3 originPos;
    private Vector3 newPos;
    private bool isAttack;
    
    /// <summary>
    /// 初始化 Tornado 中与 Init 对应的依赖和状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Tornado 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Init(TornadoController owner)
    {
        weapon = owner;
    }
    private void Update()
    {
        if (weapon == null)
        {
            return;
        }

        // 没有目标点时选择新目标；有目标点时向目标点插值移动。
        if (!isAttack)
            Attack();
        else
        {
            float dist = (Time.time - startTime) * weapon.speed;
            float factor = dist / distance;
            transform.position = Vector3.Lerp(originPos, newPos, factor);
            if (transform.position == newPos) 
            {
                // 到达目标点后，下帧重新选择新的随机目标。
                isAttack = false;
                originPos = transform.position;
                //Debug.Log("切换目标");
            }

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
        if (collision.CompareTag("Enemy"))
        {
            if (weapon == null)
            {
                return;
            }

            // 进入龙卷风范围时减速。
            collision.GetComponent<Enemy>().enemySpeed /= 2;
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
        if (collision.CompareTag("Enemy"))
        {
            if (weapon == null)
            {
                return;
            }

            if (!collision.TryGetComponent(out Enemy enemy))
            {
                return;
            }

            // 龙卷风持续伤害也统一走 DamageSystem，保留原本 ExtraDamge / 10 的规则。
            DamageSystem.ApplyToEnemy(
                DamageSystem.CreateWeaponDamage(
                    weapon.gameObject,
                    enemy,
                    collision.transform.position,
                    weapon.damage,
                    PlayerData.getInstance().ExtraDamge / 10f
                )
            );
        }
    }
    /// <summary>
    /// 处理当前对象离开二维触发器时的状态恢复。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Unity 按生命周期或消息规则自动调用，不要从普通业务代码直接调用。
    /// </remarks>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (weapon == null)
            {
                return;
            }

            // 离开范围时恢复速度。
            collision.GetComponent<Enemy>().enemySpeed *= 2;
        }
    }
    /// <summary>
    /// 选择玩家附近 moveRange 范围内的随机点作为下一段移动目标。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Tornado 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void Attack() 
    {
        // 选择玩家附近 moveRange 范围内的随机点作为下一段移动目标。
        
        startTime = Time.time;
        float newX = weapon.transform.position.x + Random.Range(-weapon.moveRange, weapon.moveRange);
        float newY = weapon.transform.position.y + Random.Range(-weapon.moveRange, weapon.moveRange);
        newPos = new Vector3(newX, newY, 0);
        distance = Vector3.Distance(originPos, new Vector3(newX, newY, 0));
        isAttack = true;
    }

}
