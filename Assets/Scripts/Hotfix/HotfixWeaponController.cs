using System;
using System.Collections;
using UnityEngine;
using XLua;

// 热更武器的通用 C# Host。
// 子类仍然挂在原来的武器 Prefab 上，保留 Inspector 字段、Prefab 引用和 Unity 生命周期；
// 真正容易变化的攻击、升级、CD、数量等规则，则通过 luaModuleName 指向的 Lua table 来执行。
public class HotfixWeaponController : WeaponController
{
    [Header("Lua Hotfix")]
    // Lua 模块名使用点号路径，例如 hotfix.weapon.missile。
    // 如果这里留空，子类可以通过 GetDefaultLuaModuleName() 提供默认模块。
    public string luaModuleName;

    // 关闭后完全走原 C# 逻辑，适合定位问题或临时禁用某个武器的热更。
    public bool enableLuaHotfix = true;

    [Header("Addressables Prefab Hotfix")]
    public string prefabKey;
    public string secondaryPrefabKey;

    protected GameObject runtimePrefab;
    protected GameObject runtimeSecondaryPrefab;
    protected bool IsHotfixStartReady => startupReady;

    // require 返回的 Lua table，以及从 table 里缓存出来的函数引用。
    // 缓存函数可以避免每帧重复从 table 里查找 OnUpdate。
    private LuaTable luaModule;
    private LuaFunction onStart;
    private LuaFunction onUpdate;
    private LuaFunction onAttack;
    private LuaFunction onLevelUp;
    private bool startupReady;

    protected override void Start()
    {
        // 先执行 WeaponController 原本的初始化，确保 weapon、cooldown 等字段可用。
        base.Start();

        // 再加载 Lua 模块。Lua 的 OnStart(host) 可以读取/修改这个武器实例。
        LoadLuaModule();
        LoadPrefabKeysFromLuaConfig();
        StartCoroutine(InitializeHotfixStart());
    }

    protected override void Update()
    {
        if (!startupReady)
        {
            return;
        }

        // 保留原本和 Weapon 数据对象的同步。
        weapon.weaponLevel = level;
        currentCooldown -= Time.deltaTime;

        // 如果 Lua 定义了 OnUpdate(host, deltaTime)，这里每帧会先给 Lua 一个机会处理逻辑。
        TryCall(onUpdate, Time.deltaTime);

        if (currentCooldown <= 0f)
        {
            Attack();
        }
        else
        {
            CDTime();
        }
    }

    /// <summary>
    /// 攻击优先交给 Lua。Lua 文件缺失、函数缺失或执行报错时，回退父类默认 Attack。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Attack()
    {
        // 攻击优先交给 Lua。Lua 文件缺失、函数缺失或执行报错时，回退父类默认 Attack。
        if (!TryCall(onAttack))
        {
            base.Attack();
        }
    }

    /// <summary>
    /// 初始化 HotfixWeaponController 中与 InitializeHotfixStart 对应的依赖和状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private IEnumerator InitializeHotfixStart()
    {
        yield return LoadAddressablePrefabs();
        TryCall(onStart);
        startupReady = true;
        OnHotfixStartReady();
    }

    /// <summary>
    /// 给 Weapon.LevelUp() 或子类升级逻辑调用的入口。 Lua 侧通常写 OnLevelUp(host)，修改 damage/count/cooldownDuration 等字段。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public virtual void LuaLevelUp()
    {
        // 给 Weapon.LevelUp() 或子类升级逻辑调用的入口。
        // Lua 侧通常写 OnLevelUp(host)，修改 damage/count/cooldownDuration 等字段。
        TryCall(onLevelUp);
    }

    /// <summary>
    /// 清零当前武器冷却计时，开始下一轮冷却。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ResetCooldown()
    {
        // 暴露给 Lua：Lua 完成一次攻击后调用，重置冷却计时。
        currentCooldown = cooldownDuration;
    }

    /// <summary>
    /// 使用当前运行时武器 Prefab 在世界坐标生成对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public GameObject SpawnPrefabAt(float x, float y)
    {
        // 暴露给 Lua：以武器物体当前位置为原点，生成默认 prefab。
        if (GetRuntimePrefab(prefab) == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Cannot spawn because prefab is null.", this);
            return null;
        }

        return SpawnAddressableOrFallback(x, y, 0f);
    }

    /// <summary>
    /// 使用当前运行时武器 Prefab 在指定位置和角度生成对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public GameObject SpawnPrefab(float x, float y, float rotationZ)
    {
        // 暴露给 Lua：生成默认 prefab，并指定 Z 轴旋转角度。
        if (GetRuntimePrefab(prefab) == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Cannot spawn because prefab is null.", this);
            return null;
        }

        return SpawnAddressableOrFallback(x, y, rotationZ);
    }

    /// <summary>
    /// 使用当前运行时武器 Prefab 在父节点局部坐标生成对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public GameObject SpawnPrefabUnder(Transform parent, float x, float y, float rotationZ)
    {
        // 暴露给 Lua：在指定父节点下生成 prefab，适合 Funnel/Sword 这类围绕父物体排布的武器。
        if (GetRuntimePrefab(prefab) == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Cannot spawn because prefab is null.", this);
            return null;
        }

        return SpawnAddressableOrFallbackUnder(parent, x, y, rotationZ);
    }

    /// <summary>
    /// 暴露给 Lua：获取离当前武器最近的有效敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public Enemy GetNearestEnemy()
    {
        // 暴露给 Lua：获取离当前武器最近的有效敌人。
        if (EnemyManager.Instance == null)
        {
            return null;
        }

        return EnemyManager.Instance.GetNearestEnemy(transform.position);
    }

    /// <summary>
    /// 暴露给 Lua：获取范围内最近的有效敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public Enemy GetNearestEnemyInRange(float range)
    {
        // 暴露给 Lua：获取范围内最近的有效敌人。
        if (EnemyManager.Instance == null)
        {
            return null;
        }

        return EnemyManager.Instance.GetNearestEnemyInRange(transform.position, range);
    }

    /// <summary>
    /// 暴露给 Lua：获取一个随机有效敌人。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public Enemy GetRandomEnemy()
    {
        // 暴露给 Lua：获取一个随机有效敌人。
        if (EnemyManager.Instance == null)
        {
            return null;
        }

        return EnemyManager.Instance.GetRandomEnemy();
    }

    /// <summary>
    /// Lua 侧更常用 Transform，返回 null 时 Lua 需要自行跳过本次攻击。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public Transform GetNearestEnemyTransform()
    {
        // Lua 侧更常用 Transform，返回 null 时 Lua 需要自行跳过本次攻击。
        Enemy enemy = GetNearestEnemy();
        return enemy != null ? enemy.transform : null;
    }

    /// <summary>
    /// 给 Lua 武器脚本提供随机目标 Transform，筛选逻辑仍统一在 EnemyManager。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public Transform GetRandomEnemyTransform()
    {
        // 给 Lua 武器脚本提供随机目标 Transform，筛选逻辑仍统一在 EnemyManager。
        Enemy enemy = GetRandomEnemy();
        return enemy != null ? enemy.transform : null;
    }

    /// <summary>
    /// 使用调用方提供的 Prefab 在指定位置和角度生成对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public GameObject SpawnCustomPrefab(GameObject sourcePrefab, float x, float y, float rotationZ)
    {
        // 暴露给 Lua：不使用当前武器的 prefab，而是使用 Lua/C# 传入的其它 prefab。
        if (sourcePrefab == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Cannot spawn because source prefab is null.", this);
            return null;
        }

        Vector2 position = (Vector2)transform.position + new Vector2(x, y);
        GameObject instance = Instantiate(sourcePrefab, position, Quaternion.Euler(0f, 0f, rotationZ));
        OnRuntimePrefabInstantiated(instance, false);
        return instance;
    }

    /// <summary>
    /// 加载 HotfixWeaponController 中与 LoadAddressablePrefabs 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public IEnumerator LoadAddressablePrefabs()
    {
        if (!string.IsNullOrWhiteSpace(prefabKey))
        {
            yield return AddressableResourceManager.PreloadPrefab(
                prefabKey,
                loadedPrefab =>
                {
                    runtimePrefab = loadedPrefab;
                    Debug.Log($"[{GetType().Name}] Loaded Addressable prefab: {prefabKey}", this);
                },
                () => Debug.LogWarning($"[{GetType().Name}] Use Inspector fallback prefab: {prefabKey}", this)
            );
        }

        if (!string.IsNullOrWhiteSpace(secondaryPrefabKey))
        {
            yield return AddressableResourceManager.PreloadPrefab(
                secondaryPrefabKey,
                loadedPrefab =>
                {
                    runtimeSecondaryPrefab = loadedPrefab;
                    Debug.Log($"[{GetType().Name}] Loaded Addressable secondary prefab: {secondaryPrefabKey}", this);
                },
                () => Debug.LogWarning($"[{GetType().Name}] Use Inspector fallback secondary prefab: {secondaryPrefabKey}", this)
            );
        }

    }

    /// <summary>
    /// 使用 Addressables 主 Prefab 或 Inspector fallback 在世界坐标生成对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public GameObject SpawnAddressableOrFallback(float x, float y, float rotationZ)
    {
        Vector2 position = (Vector2)transform.position + new Vector2(x, y);
        return InstantiateRuntimePrefab(position, Quaternion.Euler(0f, 0f, rotationZ));
    }

    /// <summary>
    /// 使用 Addressables 主 Prefab 或 fallback 在指定父节点下生成对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public GameObject SpawnAddressableOrFallbackUnder(Transform parent, float x, float y, float rotationZ)
    {
        Transform spawnParent = parent != null ? parent : transform;
        Vector3 position = spawnParent.position + new Vector3(x, y, 0f);
        return InstantiateRuntimePrefab(position, Quaternion.Euler(0f, 0f, rotationZ), spawnParent);
    }

    /// <summary>
    /// 使用运行时主 Prefab 实例化武器对象并执行组件 owner 注入。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected GameObject InstantiateRuntimePrefab(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject sourcePrefab = GetRuntimePrefab(prefab);
        if (sourcePrefab == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Cannot spawn because prefab is null.", this);
            return null;
        }

        GameObject instance = parent == null
            ? Instantiate(sourcePrefab, position, rotation)
            : Instantiate(sourcePrefab, position, rotation, parent);
        OnRuntimePrefabInstantiated(instance, false);
        return instance;
    }

    /// <summary>
    /// 使用运行时次级 Prefab 实例化附属对象并执行组件注入。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected GameObject InstantiateRuntimeSecondaryPrefab(GameObject fallbackPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject sourcePrefab = GetRuntimeSecondaryPrefab(fallbackPrefab);
        if (sourcePrefab == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Cannot spawn because secondary prefab is null.", this);
            return null;
        }

        GameObject instance = parent == null
            ? Instantiate(sourcePrefab, position, rotation)
            : Instantiate(sourcePrefab, position, rotation, parent);
        OnRuntimePrefabInstantiated(instance, true);
        return instance;
    }

    /// <summary>
    /// 返回 Addressables 运行时主 Prefab，缺失时回退 Inspector Prefab。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected GameObject GetRuntimePrefab(GameObject fallbackPrefab)
    {
        return runtimePrefab != null ? runtimePrefab : fallbackPrefab;
    }

    /// <summary>
    /// 返回 Addressables 次级 Prefab，缺失时回退调用方资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected GameObject GetRuntimeSecondaryPrefab(GameObject fallbackPrefab)
    {
        return runtimeSecondaryPrefab != null ? runtimeSecondaryPrefab : fallbackPrefab;
    }

    /// <summary>
    /// 在 Lua 模块和 Addressables Prefab 准备完成后初始化具体武器表现。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected virtual void OnHotfixStartReady()
    {
    }

    /// <summary>
    /// 武器 Prefab 实例化后向实体注入所属 Controller。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected virtual void OnRuntimePrefabInstantiated(GameObject instance, bool isSecondaryPrefab)
    {
    }

    /// <summary>
    /// 暴露给 Lua：清空某个节点下的旧武器实例，常用于升级后重新排布子弹/飞剑。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ClearChildren(Transform root)
    {
        // 暴露给 Lua：清空某个节点下的旧武器实例，常用于升级后重新排布子弹/飞剑。
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 暴露给 Lua：Lua 不直接调用 UnityEngine.Object.Destroy，统一走 C# 包一层更安全。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void DestroyGameObject(GameObject obj)
    {
        // 暴露给 Lua：Lua 不直接调用 UnityEngine.Object.Destroy，统一走 C# 包一层更安全。
        if (obj != null)
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// 根据最新数据刷新 HotfixWeaponController 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void RefreshWeapon()
    {
        // 暴露给 Lua：调用 WeaponController 原有刷新逻辑。
        Refresh();
    }

    /// <summary>
    /// 标记 HotfixWeaponController 中与 MarkWeaponLevelMax 对应的状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 HotfixWeaponController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void MarkWeaponLevelMax(string selectName)
    {
        // 暴露给 Lua：当 Lua 判断武器满级时，同步 C# Weapon 状态，并从选择列表里移除。
        Weapon targetWeapon = weapon != null ? weapon : GetComponent<Weapon>();
        if (targetWeapon != null)
        {
            targetWeapon.isLevelMax = true;
            targetWeapon.weaponLevel++;
        }

        if (WeaponSelectController.instance != null && !string.IsNullOrEmpty(selectName))
        {
            WeaponSelectController.instance.LevelMaxRemove(selectName);
        }
    }

    /// <summary>
    /// 尝试执行 TryLuaAttack，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected bool TryLuaAttack()
    {
        // 给子类保留的快捷入口：子类可以先尝试 Lua，失败再执行自己的 C# 回退逻辑。
        return TryCall(onAttack);
    }

    /// <summary>
    /// 尝试执行 TryLuaLevelUp，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected bool TryLuaLevelUp()
    {
        // 给子类保留的快捷入口：升级逻辑优先走 Lua。
        return TryCall(onLevelUp);
    }

    /// <summary>
    /// 尝试执行 TryLuaUpdate，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected bool TryLuaUpdate(float deltaTime)
    {
        // 给子类保留的快捷入口：Update 逻辑优先走 Lua。
        return TryCall(onUpdate, deltaTime);
    }

    /// <summary>
    /// 子类可以 override 这个方法，避免每个 Prefab 都手填 luaModuleName。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected virtual string GetDefaultLuaModuleName()
    {
        // 子类可以 override 这个方法，避免每个 Prefab 都手填 luaModuleName。
        return luaModuleName;
    }

    /// <summary>
    /// 根据武器 Lua 模块名推导 weapon_config 中的配置键。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected virtual string GetWeaponConfigKey()
    {
        string moduleName = string.IsNullOrWhiteSpace(luaModuleName)
            ? GetDefaultLuaModuleName()
            : luaModuleName;

        if (string.IsNullOrWhiteSpace(moduleName))
        {
            return string.Empty;
        }

        int lastDotIndex = moduleName.LastIndexOf('.');
        return lastDotIndex >= 0 ? moduleName.Substring(lastDotIndex + 1) : moduleName;
    }

    /// <summary>
    /// 加载 HotfixWeaponController 中与 LoadPrefabKeysFromLuaConfig 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void LoadPrefabKeysFromLuaConfig()
    {
        string configKey = GetWeaponConfigKey();
        if (string.IsNullOrWhiteSpace(configKey))
        {
            return;
        }

        prefabKey = LuaConfig.GetString("config.weapon_config", configKey, "prefabKey", prefabKey);
        secondaryPrefabKey = LuaConfig.GetString("config.weapon_config", configKey, "secondaryPrefabKey", secondaryPrefabKey);

        string laserKey = LuaConfig.GetString("config.weapon_config", configKey, "laserKey", string.Empty);
        if (!string.IsNullOrWhiteSpace(laserKey))
        {
            secondaryPrefabKey = laserKey;
        }
    }

    /// <summary>
    /// 加载 HotfixWeaponController 中与 LoadLuaModule 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected void LoadLuaModule()
    {
        // 没开热更时不 require Lua，后续 TryCall 会直接返回 false。
        if (!enableLuaHotfix)
        {
            return;
        }

        string moduleName = string.IsNullOrWhiteSpace(luaModuleName)
            ? GetDefaultLuaModuleName()
            : luaModuleName;

        if (string.IsNullOrWhiteSpace(moduleName))
        {
            // 没有模块名就表示这个武器暂时不接 Lua。
            return;
        }

        if (!LuaManager.Instance.TryRequireTable(moduleName, out luaModule))
        {
            // require 失败时不抛异常，武器仍然可以走 C# 默认逻辑。
            return;
        }

        // Lua 模块约定返回 table：
        // return { OnStart=function(host) end, OnAttack=function(host) end, ... }
        onStart = luaModule.Get<LuaFunction>("OnStart");
        onUpdate = luaModule.Get<LuaFunction>("OnUpdate");
        onAttack = luaModule.Get<LuaFunction>("OnAttack");
        onLevelUp = luaModule.Get<LuaFunction>("OnLevelUp");
    }

    /// <summary>
    /// 尝试执行 TryCall，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 HotfixWeaponController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected bool TryCall(LuaFunction func, params object[] args)
    {
        // 所有 Lua 调用都从这里走，保证错误处理和回退逻辑一致。
        if (!enableLuaHotfix || func == null)
        {
            return false;
        }

        try
        {
            if (args == null || args.Length == 0)
            {
                // Lua 侧第一个参数固定传 host，也就是当前这个武器 C# 实例。
                func.Call(this);
            }
            else if (args.Length == 1)
            {
                func.Call(this, args[0]);
            }
            else
            {
                object[] callArgs = new object[args.Length + 1];
                callArgs[0] = this;
                Array.Copy(args, 0, callArgs, 1, args.Length);
                func.Call(callArgs);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[{GetType().Name}] Lua call failed: {ex.Message}", this);
            return false;
        }
    }

    protected virtual void OnDestroy()
    {
        // xLua 的 LuaTable/LuaFunction 是托管引用，销毁 MonoBehaviour 时要释放，避免 LuaEnv Dispose 报引用残留。
        onStart?.Dispose();
        onUpdate?.Dispose();
        onAttack?.Dispose();
        onLevelUp?.Dispose();
        luaModule?.Dispose();

        onStart = null;
        onUpdate = null;
        onAttack = null;
        onLevelUp = null;
        luaModule = null;
    }
}
