using System.Collections.Generic;
using UnityEngine;

// 飞剑武器控制器。
// 生成飞剑、升级重建等规则优先由 hotfix.weapon.sword.lua 接管；寻敌移动仍保留在 C# 中执行。
public class SwordController : HotfixWeaponController
{
    public Vector3 offset;
    private Transform sword;

    public bool isRotating = true;

    [SerializeField] private List<Transform> swords = new List<Transform>();

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

            Transform swordTransform = swords[i];
            Sword swordComponent = swordTransform.GetComponent<Sword>();
            if (swordComponent == null)
            {
                continue;
            }

            Transform target = swordComponent.enemy;
            if (!IsValidTarget(target))
            {
                // 目标死亡、回收或为空时重新分配，避免飞剑追对象池里的旧对象。
                AssignTarget(swordTransform);
                target = swordComponent.enemy;
            }

            if (!IsValidTarget(target))
            {
                continue;
            }

            RotationSword(swordTransform, target);
            MoveObject(swordTransform, target);
        }
    }

    private void AssignTarget(Transform swordTransform)
    {
        if (swordTransform == null)
        {
            return;
        }

        // 第一版飞剑使用随机寻敌；如果想改成追最近目标，只需要换成 GetNearestEnemy。
        Enemy target = EnemyManager.Instance != null ? EnemyManager.Instance.GetRandomEnemy() : null;
        Sword swordComponent = swordTransform.GetComponent<Sword>();
        if (swordComponent != null)
        {
            swordComponent.enemy = target != null ? target.transform : null;
        }
    }

    private bool IsValidTarget(Transform target)
    {
        // 飞剑保存的是 Transform，所以这里再反查 Enemy 状态，防止锁定死亡/回收对象。
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return false;
        }

        Enemy enemy = target.GetComponent<Enemy>();
        return enemy != null && enemy.live && !enemy.isDie && enemy.Health > 0f;
    }

    public void MoveObject(Transform swordTransform, Transform target)
    {
        swordTransform.position = Vector3.MoveTowards(swordTransform.position, target.position, speed * Time.deltaTime);
    }

    public void RotationSword(Transform swordTransform, Transform target)
    {
        Vector2 dir = swordTransform.position - target.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle + 45f);
        swordTransform.rotation = Quaternion.RotateTowards(swordTransform.rotation, targetRotation, turnSpeed * Time.deltaTime);
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
        // 生成时立刻分配一次目标；没有敌人时保持 null，Update 中会继续尝试。
        AssignTarget(sword);
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
            if (swords[i] != null)
            {
                Destroy(swords[i].gameObject);
            }
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
