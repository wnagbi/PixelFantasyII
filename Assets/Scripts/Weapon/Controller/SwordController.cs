using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

// 飞剑武器控制器。
// 生成飞剑、升级重建等规则优先由 hotfix.weapon.sword.lua 接管；寻敌移动仍保留在 C# 中执行。
public class SwordController : HotfixWeaponController
{
    public Vector3 offset;
    private Transform sword;

    public bool isRotating = true;

    [SerializeField]private List<Transform> swords = new List<Transform>(); 
    [SerializeField]private List<Transform> enemies = new List<Transform>(); 
    

    protected override void Start()
    {
        base.Start();
    }
    protected override void Refresh()
    {
        InitializeWeapon();
        for (int i = 0; i < count; i++)
        {
            SwordGenerator();
        }
        

    }
    protected override void Attack()
    {
        // 当前飞剑攻击逻辑是 Lua 优先；没有 Lua 时这里不会额外执行父类攻击。
        TryLuaAttack();

    }
    protected override void Update()
    {
        base.Update();
        if (!IsHotfixStartReady)
        {
            return;
        }
        
        for (int i = 0; i < count; i++) 
        {
            if (i >= swords.Count || swords[i] == null)
            {
                continue;
            }

            Transform sword =swords[i];
            Transform target = sword.GetComponent<Sword>().enemy;
            if (target.gameObject.activeSelf == false)
            {
                AssignTarget(sword);

            }
            else
            {
                RotationSword(sword,target);
                MoveObject(sword, target);
            }
        }

    }
    private void AssignTarget(Transform sword)
    {

        // Transform target = availableTargets[Random.Range(0, availableTargets.Count - 1)];
        List<Enemy> list = EnemyManager.Instance.GetEnemiesList();
        Transform target = list[Random.Range(0, list.Count - 1)].transform;
        sword.GetComponent<Sword>().enemy = target;
            
        
    }
    
    public void MoveObject(Transform sword,Transform target)
    {

        sword.position = Vector3.MoveTowards(sword.position, target.position, speed * Time.deltaTime);
    }
    public void RotationSword(Transform sword,Transform target)
    {

        Vector2 dir = sword.position - target.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle + 45f);
        sword.rotation = Quaternion.RotateTowards(sword.rotation, targetRotation, turnSpeed * Time.deltaTime);

    }
    public void SwordGenerator()
    {
        GameObject sourcePrefab = GetRuntimePrefab(prefab);
        if (sourcePrefab == null)
        {
            return;
        }

        GameObject swordObj = InstantiateRuntimePrefab(transform.position + offset, sourcePrefab.transform.rotation);
        if (swordObj == null)
        {
            return;
        }

        sword = swordObj.transform;
        swords.Add(sword);

    }

    public void RebuildSwords()
    {
        // 暴露给 Lua：升级后销毁旧飞剑并按当前 count 重新生成。
        for (int i = 0; i < swords.Count; i++)
        {
            if (swords[i] != null)
            {
                Destroy(swords[i].gameObject);
            }
        }

        swords.Clear();
        Refresh();
    }

    protected override void OnHotfixStartReady()
    {
        RebuildSwords();
    }

    public void levelUp()
    {
        // 升级优先交给 Lua，Lua 失败时使用下面的 C# 默认升级表。
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
                speed *= 2;
                level++;
                break;
            case 2:
                count += 1;
                level++;
                break;
            case 3:
                damage += 5;
                level++;
                break;
            case 4:
                count += 1;
                level++;
                break;
            case 5:
                count += 2;
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Sword");
                weapon.weaponLevel++;
                break;
        }
        for (int i = 0; i < swords.Count; i++)
        {
            Destroy(swords[i].gameObject);
         }
            swords.Clear();
        Refresh();

    }

    protected override string GetDefaultLuaModuleName()
    {
        // 默认 Lua 模块路径。
        return "hotfix.weapon.sword";
    }
}

