-- 敌人 Lua 状态机模块。
-- LuaState 会把 IState 生命周期转发到下方同名 table；enemy 是当前 C# Enemy 实例。
local M = {}

-- 旧版敌人伤害修正入口，DamageSystem 新模块不可用时才会回退到这里。
function M.AdjustDamage(enemy, damage)
    return damage
end

-- 敌人死亡规则入口。返回 true 表示 Lua 已处理，C# 不再重复执行默认销毁逻辑。
function M.OnEnemyDestroy(enemy, id)
    enemy:DefaultEnemyDestroy()
    return true
end

-- Idle 当前只保留完整接口，方便后续热更待机判断。
M.EnemyIdle = {
    OnEnter = function(enemy)
    end,

    OnUpdate = function(enemy)
    end,

    OnFixedUpdate = function(enemy)
    end,

    OnExit = function(enemy)
    end,
}

-- Move 每帧追踪玩家，并根据受伤/死亡标记切换状态。
M.EnemyMove = {
    OnEnter = function(enemy)
    end,

    OnUpdate = function(enemy)
        enemy:ChasePlayer()
        if enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Hurt)
        end
        if enemy.isDie and not enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Die)
        end
    end,

    OnFixedUpdate = function(enemy)
    end,

    OnExit = function(enemy)
    end,
}

-- Hurt 进入时触发闪白，受伤结束或死亡后切换状态。
M.EnemyHurt = {
    OnEnter = function(enemy)
        enemy:FlashColor(0.1)
    end,

    OnUpdate = function(enemy)
        if enemy.isDie and enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Die)
        end
        if not enemy.isHurt then
            enemy:TransitionState(CS.EnemyStateType.Move)
        end
    end,

    OnFixedUpdate = function(enemy)
    end,

    OnExit = function(enemy)
    end,
}

-- Die 进入时执行击杀、掉落和对象池回收流程。
M.EnemyDie = {
    OnEnter = function(enemy)
        enemy:EnemyDestroy()
    end,

    OnUpdate = function(enemy)
        if not enemy.isDie then
            enemy:TransitionState(CS.EnemyStateType.Move)
        end
    end,

    OnFixedUpdate = function(enemy)
    end,

    OnExit = function(enemy)
        enemy.isDie = false
    end,
}

-- Attack 预留给后续特殊敌人的主动攻击行为。
M.EnemyAttack = {
    OnEnter = function(enemy)
    end,

    OnUpdate = function(enemy)
    end,

    OnFixedUpdate = function(enemy)
    end,

    OnExit = function(enemy)
    end,
}

return M
