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

    protected override void Attack()
    {
        if (TryLuaAttack())
        {
            return;
        }

        base.Attack();
        rotationPoint.transform.rotation = Quaternion.Euler(0f, 0f, rotationPoint.transform.rotation.eulerAngles.z + (speed * Time.deltaTime));
    }

    protected override void Refresh()
    {
    }

    public void RebuildOrbitObjects()
    {
        ClearChildren(rotationPoint != null ? rotationPoint.transform : transform.GetChild(0));
        SpawnOrbitObjects();
    }

    protected override void OnHotfixStartReady()
    {
        RebuildOrbitObjects();
    }

    private void SpawnOrbitObjects()
    {
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

    public void levelUp()
    {
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

    protected override string GetDefaultLuaModuleName()
    {
        return "hotfix.weapon.new_weapon";
    }
}
