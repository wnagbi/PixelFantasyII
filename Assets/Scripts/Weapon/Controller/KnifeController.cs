using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 飞刀/环绕刀武器控制器。
// 攻击旋转、升级成长、重建刀阵等规则优先由 hotfix.weapon.knife.lua 接管。
public class KnifeController : HotfixWeaponController
{
    public GameObject rotationPoint;
    

    
    protected override void Start()
    {
        base.Start();
       
    }
    protected override void Refresh()
    {
        InitializeWeapon();
        for (int i = 0; i < count; i++)
        {
            Vector3 rota = Vector3.forward * 360 * i / count;
            GameObject knifeObj = InstantiateRuntimePrefab(rotationPoint.transform.position, Quaternion.identity, rotationPoint.transform);
            if (knifeObj == null)
            {
                continue;
            }

            Transform newKnife = knifeObj.transform;
            newKnife.Rotate(rota);
            newKnife.Translate(newKnife.up * 2f, Space.World);
        }
    }



    protected override void Attack()
    {
        // Lua 处理成功时不执行 C# 默认旋转；Lua 失败才回退。
        if (TryLuaAttack())
        {
            return;
        }

        base.Attack();
        rotationPoint.transform.rotation = Quaternion.Euler(0f, 0f, rotationPoint.transform.rotation.eulerAngles.z + (speed * Time.deltaTime));

    }
    public void levelUp() 
    {
        // Lua 可以热更 count、damage、speed 和满级移除逻辑。
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
                damage += 5;        
                level++;
                break;
            case 2:
                count += 2;            
                level++;
                break;
            case 3:
                count += 2;
                level++;
                break;
            case 4:
                count += 2;                
                level++;
                break;
            case 5:
                speed *= 2;
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Knifle");
                weapon.weaponLevel++;
                break;
        }
        for (int i = 0; i < transform.GetChild(0).childCount; i++) 
        {
            Destroy(transform.GetChild(0).GetChild(i).gameObject);
        }
        Refresh();
    }

    public void RebuildKnives()
    {
        // 暴露给 Lua：升级后清空旧刀阵并按当前 count 重新生成。
        ClearChildren(rotationPoint != null ? rotationPoint.transform : transform.GetChild(0));
        Refresh();
    }

    protected override void OnHotfixStartReady()
    {
        RebuildKnives();
    }

    protected override string GetDefaultLuaModuleName()
    {
        // 默认 Lua 模块路径。
        return "hotfix.weapon.knife";
    }
    
}
