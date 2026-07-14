local M = {}

function M.AdjustDamage(enemy, damage)
    return damage
end

function M.OnEnemyDestroy(enemy, id)
    enemy:DefaultEnemyDestroy()
    return true
end

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
