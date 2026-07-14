using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 浮游炮武器控制器。
// 浮游炮数量、旋转、开火和升级规则优先由 hotfix.weapon.funnel.lua 接管。
public class FunnelController : HotfixWeaponController
{
    public GameObject rotationPoint;
    public GameObject laser;
    public List<GameObject> funnels;



    protected override void Start()
    {
        base.Start();
    }
    protected override void Attack()
    {
        // Lua 攻击成功时不执行 C# 默认激光发射。
        if (TryLuaAttack())
        {
            return;
        }

        base.Attack();
        ShootLaser(funnels);
    }
    protected override void CDTime()
    {
        Rotation();
    }
    protected override void Refresh()
    {
        funnels.Clear();
        for (int i = 0; i < count; i++) 
        {
            Vector3  rota = Vector3.forward * 360 *i / count;
            GameObject funnelObj = InstantiateRuntimePrefab(rotationPoint.transform.position, Quaternion.identity, rotationPoint.transform);
            if (funnelObj == null)
            {
                continue;
            }

            Transform funnel = funnelObj.transform;
            funnels.Add(funnel.gameObject);
            funnel.Rotate(rota);          
            funnel.Translate(funnel.up* 1.5f,Space.World);

        }
    }
    public void Rotation() 
    {
        rotationPoint.transform.rotation = Quaternion.Euler(0f, 0f, rotationPoint.transform.rotation.eulerAngles.z + (speed * Time.deltaTime));
    }
    public void ShootLaser(List<GameObject> objs) 
    {
        for (int i = 0; i < objs.Count; i++) 
        {
            InstantiateRuntimeSecondaryPrefab(laser, objs[i].transform.position, objs[i].transform.rotation * Quaternion.Euler(0, 0, 90f), objs[i].transform);
        }
        
    }

    public void RebuildFunnels()
    {
        // 暴露给 Lua：升级后清空并按当前 count 重建浮游炮。
        ClearChildren(rotationPoint != null ? rotationPoint.transform : transform.GetChild(0));
        Refresh();
    }

    protected override void OnHotfixStartReady()
    {
        RebuildFunnels();
    }

    public void levelUp()
    {
        // 升级优先交给 Lua，Lua 失败时走 C# 默认升级表。
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
                count++;
                level++;
                break;
            case 2:
                damage+=5;
                level++;
                break;
            case 3:
                count++;
                level++;
                break;
            case 4:
                speed*=2;
                level++;
                break;
            case 5:
                cooldownDuration /= 2;
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Funnel");
                weapon.weaponLevel++;
                break;
        }
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Destroy(transform.GetChild(0).GetChild(i).gameObject);
        }
        Refresh();
    }

    protected override string GetDefaultLuaModuleName()
    {
        // 默认 Lua 模块路径。
        return "hotfix.weapon.funnel";
    }
}
