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

    /// <summary>
    /// 根据最新数据刷新 SwordController 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SwordController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Refresh()
    {
        InitializeWeapon();
        for (int i = 0; i < count; i++)
        {
            SwordGenerator();
        }
    }

    /// <summary>
    /// 当前飞剑攻击逻辑是 Lua 优先；没有 Lua 时这里不会额外执行父类攻击。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SwordController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
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

    /// <summary>
    /// 为指定飞剑分配当前最近的有效敌人目标。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SwordController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void AssignTarget(Transform swordTransform)
    {
        if (swordTransform == null)
        {
            return;
        }

        // 第一版飞剑使用随机寻敌；如果想改成追最近目标，只需要换成 GetNearestEnemy。
        Enemy target = EnemyManager.Instance != null ? EnemyManager.Instance.GetNearestEnemy(transform.position) : null;
        Sword swordComponent = swordTransform.GetComponent<Sword>();
        if (swordComponent != null)
        {
            swordComponent.enemy = target != null ? target.transform : null;
        }
    }

    /// <summary>
    /// 判断 SwordController 当前是否满足 IsValidTarget 对应的状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SwordController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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

    /// <summary>
    /// 把飞剑朝当前有效目标移动。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SwordController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void MoveObject(Transform swordTransform, Transform target)
    {
        swordTransform.position = Vector3.MoveTowards(swordTransform.position, target.position, speed * Time.deltaTime);
    }

    /// <summary>
    /// 让飞剑朝向当前移动目标。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SwordController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void RotationSword(Transform swordTransform, Transform target)
    {
        Vector2 dir = swordTransform.position - target.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle + 45f);
        swordTransform.rotation = Quaternion.RotateTowards(swordTransform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 根据当前数量创建飞剑并为每把飞剑分配目标。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SwordController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
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

    /// <summary>
    /// 武器 Prefab 实例化后向实体注入所属 Controller。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SwordController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void OnRuntimePrefabInstantiated(GameObject instance, bool isSecondaryPrefab)
    {
        if (instance != null && instance.TryGetComponent(out Sword swordComponent))
        {
            swordComponent.Init(this);
        }
    }

    /// <summary>
    /// 暴露给 Lua：升级后销毁旧飞剑并按当前 count 重新生成。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SwordController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
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

    /// <summary>
    /// 在 Lua 模块和 Addressables Prefab 准备完成后初始化具体武器表现。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SwordController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void OnHotfixStartReady()
    {
        RebuildSwords();
    }

    /// <summary>
    /// 升级优先交给 Lua，Lua 失败时使用下面的 C# 默认升级表。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SwordController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
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

    /// <summary>
    /// 默认 Lua 模块路径。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SwordController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override string GetDefaultLuaModuleName()
    {
        // 默认 Lua 模块路径。
        return "hotfix.weapon.sword";
    }
}
