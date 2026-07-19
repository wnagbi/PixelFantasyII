using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

// 镰刀武器控制器。
// 发射数量、CD、伤害和升级规则优先由 hotfix.weapon.scythe.lua 接管。
public class ScytheController : HotfixWeaponController
{
    public GameObject shootPoint;

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Lua 攻击成功时直接返回；否则使用原 C# 环形发射逻辑。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ScytheController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Attack()
    {
        // Lua 攻击成功时直接返回；否则使用原 C# 环形发射逻辑。
        if (TryLuaAttack())
        {
            return;
        }

        base.Attack();
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = Vector3.forward * 360 * i / count;
            GameObject scytheObj = InstantiateRuntimePrefab(shootPoint.transform.position, Quaternion.identity, shootPoint.transform);
            if (scytheObj == null)
            {
                continue;
            }

            Transform scythe = scytheObj.transform;
            scythe.Rotate(dir);
        }
    }
    /// <summary>
    /// 归还或返回 ScytheController 中与 returnTimer 对应的对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ScytheController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public float returnTimer()
    {
        return timer;
    }

    /// <summary>
    /// 升级优先走 Lua，失败时保留 C# 默认升级表。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ScytheController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void levelUp()
    {
        // 升级优先走 Lua，失败时保留 C# 默认升级表。
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
                damage+=5;
                level++;
                break;
            case 2:
                damage+=2;
                level++;
                break;
            case 3:
                cooldownDuration/=2;
                level++;
                break;
            case 4:
                count += 2;
                level++;
                break;
            case 5:
                damage += 10;
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Scythe");
                weapon.weaponLevel++;
                break;
        }
        
        

    }

    /// <summary>
    /// 默认 Lua 模块路径。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ScytheController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override string GetDefaultLuaModuleName()
    {
        // 默认 Lua 模块路径。
        return "hotfix.weapon.scythe";
    }

}
