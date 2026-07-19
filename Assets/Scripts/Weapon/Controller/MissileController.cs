using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 导弹武器控制器。
// 运行时攻击和升级优先交给 hotfix.weapon.missile.lua，Lua 失败时保留下面的 C# 回退逻辑。
public class MissileController : HotfixWeaponController
{
    
    public float missileAttackRangeOut;
    public float missileAttackRangeIn;

    /// <summary>
    /// 如果 Inspector 没填 luaModuleName，默认加载这个 Lua 文件。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 MissileController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override string GetDefaultLuaModuleName()
    {
        // 如果 Inspector 没填 luaModuleName，默认加载这个 Lua 文件。
        return "hotfix.weapon.missile";
    }

    /// <summary>
    /// 导弹发射优先走 Lua 的 OnAttack(host)。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 MissileController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Attack()
    {
        // 导弹发射优先走 Lua 的 OnAttack(host)。
        if (TryLuaAttack())
        {
            return;
        }

        base.Attack();
        // for (int i = 0; i < count; i++) 
        // {
        //     float radius = Random.Range(missileAttackRangeIn, missileAttackRangeOut);
        //     float angle = Random.Range(0f, 2f * Mathf.PI);
        //     float x = radius * Mathf.Cos(angle);
        //     float y = radius * Mathf.Sin(angle);
        //     Vector2 attackPosition = new Vector2(x, y);            
        //     GameObject missle = Instantiate(prefab, (Vector2)transform.position+ attackPosition, Quaternion.identity);            

        // }
        

    }

    /// <summary>
    /// 武器 Prefab 实例化后向实体注入所属 Controller。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 MissileController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void OnRuntimePrefabInstantiated(GameObject instance, bool isSecondaryPrefab)
    {
        if (instance != null && instance.TryGetComponent(out Missile missile))
        {
            missile.Init(this);
        }
    }
    

    /// <summary>
    /// 升级优先走 Lua 的 OnLevelUp(host)，方便热更伤害、CD、发射数量和满级规则。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 MissileController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void levelUp()
    {
        // 升级优先走 Lua 的 OnLevelUp(host)，方便热更伤害、CD、发射数量和满级规则。
        if (TryLuaLevelUp())
        {
            return;
        }

        switch (level)
        {
            case 0:     
                level++;
                break;
            case 1:
                cooldownDuration -= 1;
                
                level++;
                break;
            case 2:
                damage += 10;
                level++;
                break;
            case 3:
                
                level++;
                break;
            case 4:
                timer /= 2;
                level++;
                break;
            case 5:
                damage *= 2;
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Missle");
                weapon.weaponLevel++;
                break;
        }

    }
    /// <summary>
    /// 在延迟结束后销毁或回收指定导弹对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 MissileController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void MissileDestroy(GameObject obj)
    {
        Destroy(obj);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(transform.position, missileAttackRangeOut);
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(transform.position, missileAttackRangeIn);
    //}
}
