using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NewController 对应一个环绕类武器样板。
// 攻击旋转、升级数量和重建逻辑优先由 hotfix.weapon.new_weapon.lua 接管。
public class NewController : HotfixWeaponController
{
    public GameObject rotationPoint;

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Lua 返回成功时完全接管旋转；失败则使用下方 C# fallback。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 NewController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Attack()
    {
        // Lua 返回成功时完全接管旋转；失败则使用下方 C# fallback。
        if (TryLuaAttack())
        {
            return;
        }

        base.Attack();
        rotationPoint.transform.rotation = Quaternion.Euler(0f, 0f, rotationPoint.transform.rotation.eulerAngles.z + (speed * Time.deltaTime));
    }

    /// <summary>
    /// 根据最新数据刷新 NewController 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 NewController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Refresh()
    {
    }

    /// <summary>
    /// 清理旧轨道对象后按最新 count 重新生成，供 Lua 升级规则调用。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 NewController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void RebuildOrbitObjects()
    {
        // 清理旧轨道对象后按最新 count 重新生成，供 Lua 升级规则调用。
        ClearChildren(rotationPoint != null ? rotationPoint.transform : transform.GetChild(0));
        SpawnOrbitObjects();
    }

    /// <summary>
    /// Addressables Prefab 加载完成后再首次生成，避免旧资源先显示一帧。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 NewController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void OnHotfixStartReady()
    {
        // Addressables Prefab 加载完成后再首次生成，避免旧资源先显示一帧。
        RebuildOrbitObjects();
    }

    /// <summary>
    /// 按当前数量生成并均匀排列轨道武器对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 NewController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void SpawnOrbitObjects()
    {
        // 将 count 个对象均匀放置在固定半径的圆周上。
        if (rotationPoint == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 rota = Vector3.forward * 360 * i / count;
            GameObject orbitObj = InstantiateRuntimePrefab(rotationPoint.transform.position, Quaternion.identity, rotationPoint.transform);
            if (orbitObj == null)
            {
                continue;
            }

            Transform orbit = orbitObj.transform;
            orbit.Rotate(rota);
            orbit.Translate(orbit.up * 5f, Space.World);
            orbit.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }

    /// <summary>
    /// 升级优先交给 Lua；模块缺失或报错时保留默认数量成长。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 NewController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void levelUp()
    {
        // 升级优先交给 Lua；模块缺失或报错时保留默认数量成长。
        if (TryLuaLevelUp())
        {
            return;
        }

        switch (level)
        {
            case 0:
                count++;
                level++;
                break;
            case 1:
                count++;
                level++;
                break;
            case 2:
                count++;
                level++;
                break;
            case 3:
                count++;
                level++;
                break;
            case 4:
                count++;
                level++;
                break;
            case 5:
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Funnel");
                weapon.weaponLevel++;
                break;
        }

        RebuildOrbitObjects();
    }

    /// <summary>
    /// Inspector 未指定模块时使用的默认 require 路径。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 NewController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override string GetDefaultLuaModuleName()
    {
        // Inspector 未指定模块时使用的默认 require 路径。
        return "hotfix.weapon.new_weapon";
    }
}
