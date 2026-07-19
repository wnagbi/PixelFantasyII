-- 敌人 Lua 状态机模块。
-- LuaState 会把 IState 生命周期转发到下方同名 table；enemy 是当前 C# Enemy 实例。
local M = {}

-- 用途：作为统一伤害模块不可用时的敌人伤害修正 fallback。
-- 使用注意：必须返回有效数字；新伤害规则应优先写入 hotfix.combat.damage_rule。
function M.AdjustDamage(enemy, damage)
    return damage
end

-- 用途：接管敌人死亡后的击杀计数、掉落触发和对象池回收流程。
-- 使用注意：返回 true 前必须完成销毁流程，否则 C# 不会再执行 DefaultEnemyDestroy。
function M.OnEnemyDestroy(enemy, id)
    enemy:DefaultEnemyDestroy()
    return true
end

-- Idle 当前只保留完整接口，方便后续热更待机判断。
M.EnemyIdle = {
    -- 用途：进入待机状态时初始化待机表现。
    -- 使用注意：由 LuaState.OnEnter 调用；当前为空是为了保留稳定扩展点。
    OnEnter = function(enemy)
    end,

    -- 用途：执行待机状态的逐帧判断。
    -- 使用注意：由 LuaState.OnUpData 每帧调用，避免在此执行资源加载或场景扫描。
    OnUpdate = function(enemy)
    end,

    -- 用途：执行待机状态的固定时间步逻辑。
    -- 使用注意：由 LuaState.OnFixUpData 调用，仅放置物理相关规则。
    OnFixedUpdate = function(enemy)
    end,

    -- 用途：退出待机状态时清理临时效果。
    -- 使用注意：由 LuaState.OnExit 调用；当前为空时不需要额外清理。
    OnExit = function(enemy)
    end,
}

-- Move 每帧追踪玩家，并根据受伤/死亡标记切换状态。
M.EnemyMove = {
    -- 用途：进入移动状态时初始化追踪行为。
    -- 使用注意：由 LuaState.OnEnter 调用；不要在这里缓存可能被对象池回收的玩家引用。
    OnEnter = function(enemy)
    end,

    -- 用途：追踪玩家并根据受伤、死亡标记切换状态。
    -- 使用注意：每帧调用；isDie 为 true 表示已经死亡，状态切换条件不能反向判断。
    OnUpdate = function(enemy)
        enemy:ChasePlayer()
        if enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Hurt)
        end
        if enemy.isDie and not enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Die)
        end
    end,

    -- 用途：执行移动状态的固定时间步规则。
    -- 使用注意：当前移动由 C# ChasePlayer 负责，空实现用于保持 LuaState 接口完整。
    OnFixedUpdate = function(enemy)
    end,

    -- 用途：退出移动状态时清理移动状态的临时效果。
    -- 使用注意：由状态机自动调用；当前没有额外资源需要释放。
    OnExit = function(enemy)
    end,
}

-- Hurt 进入时触发闪白，受伤结束或死亡后切换状态。
M.EnemyHurt = {
    -- 用途：进入受伤状态时触发敌人闪白反馈。
    -- 使用注意：FlashColor 参数单位为秒，重复进入时应允许 C# 重置颜色计时。
    OnEnter = function(enemy)
        enemy:FlashColor(0.1)
    end,

    -- 用途：根据死亡和受伤标记决定离开受伤状态后的目标状态。
    -- 使用注意：每帧调用；死亡判断必须优先于恢复移动状态。
    OnUpdate = function(enemy)
        if enemy.isDie and enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Die)
        end
        if not enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Move)
        end
    end,

    -- 用途：执行受伤状态的固定时间步规则。
    -- 使用注意：当前为空，不要把普通逐帧状态切换放到这里重复执行。
    OnFixedUpdate = function(enemy)
    end,

    -- 用途：退出受伤状态时清理临时规则。
    -- 使用注意：闪白颜色恢复由 C# 管理，当前无需在 Lua 重复处理。
    OnExit = function(enemy)
    end,
}

-- Die 进入时执行击杀、掉落和对象池回收流程。
M.EnemyDie = {
    -- 用途：进入死亡状态时触发统一击杀、掉落和对象池回收流程。
    -- 使用注意：EnemyDestroy 可能立即禁用对象，后续代码不能继续访问已回收实例。
    OnEnter = function(enemy)
        enemy:EnemyDestroy()
    end,

    -- 用途：处理对象池重新启用后死亡标记已被清除的状态恢复。
    -- 使用注意：仅在对象仍激活时有效；isDie 为 false 才能回到移动状态。
    OnUpdate = function(enemy)
        if not enemy.isDie then
            enemy:TransitionState(CS.EnemyStateType.Move)
        end
    end,

    -- 用途：保留死亡状态的固定时间步扩展入口。
    -- 使用注意：对象通常会在 OnEnter 中回收，不应在这里依赖其仍处于激活状态。
    OnFixedUpdate = function(enemy)
    end,

    -- 用途：退出死亡状态时清除死亡标记供对象池下次复用。
    -- 使用注意：必须重置 isDie，否则重新启用后的敌人会被有效性过滤器排除。
    OnExit = function(enemy)
        enemy.isDie = false
    end,
}

-- Attack 预留给后续特殊敌人的主动攻击行为。
M.EnemyAttack = {
    -- 用途：进入主动攻击状态时初始化特殊敌人攻击表现。
    -- 使用注意：当前为空，扩展时应保留 C# 碰撞伤害仍走 DamageSystem。
    OnEnter = function(enemy)
    end,

    -- 用途：执行主动攻击状态的逐帧规则。
    -- 使用注意：每帧调用，避免在这里直接 Instantiate 高频攻击对象。
    OnUpdate = function(enemy)
    end,

    -- 用途：执行主动攻击状态的物理更新。
    -- 使用注意：仅放置需要固定时间步的 Rigidbody2D 操作。
    OnFixedUpdate = function(enemy)
    end,

    -- 用途：退出主动攻击状态时恢复临时状态。
    -- 使用注意：扩展攻击 Buff 或动画标记时必须在这里成对清理。
    OnExit = function(enemy)
    end,
}

return M
