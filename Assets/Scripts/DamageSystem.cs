using UnityEngine;

public enum DamageTargetType
{
    // 伤害目标是敌人。
    Enemy,
    // 伤害目标是玩家。
    Player
}

public enum DamageSourceType
{
    // 武器造成的伤害，例如飞刀、导弹、飞剑。
    Weapon,
    // 技能造成的伤害，例如次元斩。
    Skill,
    // 敌人与玩家接触时造成的伤害。
    EnemyContact,
    // 场景机关、陷阱等环境伤害，第一版先预留。
    Environment,
    // 兼容旧代码或调试调用的伤害来源。
    Debug
}

// 一次伤害结算所需要的上下文。
// 武器、技能、敌人接触都先把信息整理成这个结构，再交给 DamageSystem 统一处理。
public struct DamageContext
{
    // 伤害来源对象，可以是武器控制器、技能特效、敌人等。
    public GameObject attacker;
    // 被攻击对象，ApplyToEnemy / ApplyToPlayer 会从这里取 Enemy 或 Player 组件。
    public GameObject target;
    // 命中位置，用于显示伤害数字或后续做受击特效。
    public Vector3 hitPoint;

    // 基础伤害，例如武器自身 damage。
    public float baseDamage;
    // 额外伤害，例如 PlayerData.ExtraDamge。
    public float bonusDamage;
    // 伤害倍率；如果外部忘记设置，DamageSystem 会把 0 当作 1 处理。
    public float multiplier;

    // 目标类型和来源类型，用于后续扩展不同规则。
    public DamageTargetType targetType;
    public DamageSourceType sourceType;

    // 是否显示伤害数字。玩家受伤第一版不飘字，敌人受伤默认飘字。
    public bool showDamageNumber;
    // 预留：以后接护盾、防御时可以用。第一版暂不计算防御。
    public bool ignoreDefense;
    // 是否秒杀目标。飞剑锁定目标、次元斩这类逻辑使用它。
    public bool instantKill;
}

// DamageSystem 返回的结果，方便调用方知道这次伤害是否真的生效、是否击杀。
public struct DamageResult
{
    public float finalDamage;
    public bool applied;
    public bool killed;
}

// 统一战斗伤害管线。
// 所有伤害都应该尽量经过这里：计算基础伤害 -> Lua 修正 -> 目标扣血 -> 显示伤害数字。
public static class DamageSystem
{
    // 新版统一伤害 Lua 模块。
    private const string DamageRuleModule = "hotfix.combat.damage_rule";
    // 旧模块保留为 fallback，避免旧 Lua 文件还没同步时伤害规则失效。
    private const string LegacyEnemyRuleModule = "hotfix.enemy.enemy_state";
    private const string LegacyPlayerRuleModule = "hotfix.player.player_rule";

    public static DamageResult ApplyToEnemy(DamageContext context)
    {
        // 先从 context.target 上取 Enemy。目标为空或已经禁用时，本次伤害直接跳过。
        Enemy enemy = ResolveTarget<Enemy>(context);
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
        {
            return CreateSkippedResult();
        }

        context.targetType = DamageTargetType.Enemy;
        float finalDamage = CalculateBaseDamage(context);
        if (context.instantKill)
        {
            // 秒杀不直接销毁对象，而是转成一笔足够大的伤害，继续走敌人受伤/死亡事件。
            finalDamage = enemy.Health > 0f ? enemy.Health : enemy.maxHealht;
        }

        // Lua 在这里获得最终修正机会，例如统一把敌人受到的伤害乘以倍率。
        finalDamage = AdjustEnemyDamage(enemy, finalDamage);
        if (finalDamage <= 0f)
        {
            return CreateSkippedResult(finalDamage);
        }

        // 真正扣血交回 Enemy 自己做，DamageSystem 不直接修改 Enemy 字段。
        DamageResult result = enemy.ApplyDamageFromSystem(finalDamage);
        if (result.applied && context.showDamageNumber && DamageNumberController.instance != null)
        {
            DamageNumberController.instance.SpawnDamage(result.finalDamage, context.hitPoint);
        }

        return result;
    }

    public static DamageResult ApplyToPlayer(DamageContext context)
    {
        // 玩家受伤也走同一套计算和 Lua 修正，但第一版不显示伤害数字。
        Player player = ResolveTarget<Player>(context);
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            return CreateSkippedResult();
        }

        context.targetType = DamageTargetType.Player;
        float finalDamage = CalculateBaseDamage(context);
        finalDamage = AdjustPlayerDamage(player, finalDamage);
        if (finalDamage <= 0f)
        {
            return CreateSkippedResult(finalDamage);
        }

        return player.ApplyDamageFromSystem(finalDamage);
    }

    public static DamageContext CreateWeaponDamage(
        GameObject attacker,
        Enemy target,
        Vector3 hitPoint,
        float baseDamage,
        float bonusDamage = 0f)
    {
        // 武器伤害的快捷构造方法，避免每个武器重复写一大段 DamageContext 初始化。
        return new DamageContext
        {
            attacker = attacker,
            target = target != null ? target.gameObject : null,
            hitPoint = hitPoint,
            baseDamage = baseDamage,
            bonusDamage = bonusDamage,
            multiplier = 1f,
            targetType = DamageTargetType.Enemy,
            sourceType = DamageSourceType.Weapon,
            showDamageNumber = true,
            ignoreDefense = false,
            instantKill = false
        };
    }

    public static DamageContext CreateEnemyContactDamage(
        Enemy attacker,
        Player target,
        Vector3 hitPoint,
        float baseDamage)
    {
        // 敌人接触玩家伤害的快捷构造方法。
        return new DamageContext
        {
            attacker = attacker != null ? attacker.gameObject : null,
            target = target != null ? target.gameObject : null,
            hitPoint = hitPoint,
            baseDamage = baseDamage,
            bonusDamage = 0f,
            multiplier = 1f,
            targetType = DamageTargetType.Player,
            sourceType = DamageSourceType.EnemyContact,
            showDamageNumber = false,
            ignoreDefense = false,
            instantKill = false
        };
    }

    private static float CalculateBaseDamage(DamageContext context)
    {
        // 防止外部没有设置 multiplier 导致所有伤害被乘成 0。
        float multiplier = Mathf.Approximately(context.multiplier, 0f) ? 1f : context.multiplier;
        return (context.baseDamage + context.bonusDamage) * multiplier;
    }

    private static float AdjustEnemyDamage(Enemy enemy, float damage)
    {
        // 优先走新版统一伤害 Lua；失败时回退到旧 enemy_state.lua。
        if (LuaConfig.TryCallFloat(DamageRuleModule, "AdjustEnemyDamage", enemy, damage, out float luaDamage))
        {
            return luaDamage;
        }

        if (LuaConfig.TryCallFloat(LegacyEnemyRuleModule, "AdjustDamage", enemy, damage, out luaDamage))
        {
            return luaDamage;
        }

        return damage;
    }

    private static float AdjustPlayerDamage(Player player, float damage)
    {
        // 优先走新版统一伤害 Lua；失败时回退到旧 player_rule.lua。
        if (LuaConfig.TryCallFloat(DamageRuleModule, "AdjustPlayerDamage", player, damage, out float luaDamage))
        {
            return luaDamage;
        }

        if (LuaConfig.TryCallFloat(LegacyPlayerRuleModule, "AdjustDamage", player, damage, out luaDamage))
        {
            return luaDamage;
        }

        return damage;
    }

    private static T ResolveTarget<T>(DamageContext context) where T : Component
    {
        // 统一处理 target 为空、组件不存在等情况，避免每个入口重复判空。
        if (context.target == null)
        {
            return null;
        }

        return context.target.TryGetComponent(out T component) ? component : null;
    }

    private static DamageResult CreateSkippedResult(float finalDamage = 0f)
    {
        // 表示这次伤害没有真正应用，例如目标为空、伤害小于等于 0。
        return new DamageResult
        {
            finalDamage = finalDamage,
            applied = false,
            killed = false
        };
    }
}
