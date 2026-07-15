local M = {}

-- 敌人受到伤害前的统一修正入口。
-- enemy 是 C# 的 Enemy 实例，damage 是 C# 计算出的基础最终伤害。
-- 这里可以返回 damage * 2、damage * 0.5 等，用来热更新伤害倍率。
function M.AdjustEnemyDamage(enemy, damage)
    return damage
end

-- 玩家受到伤害前的统一修正入口。
-- player 是 C# 的 Player 实例，damage 是敌人接触或其他来源传入的伤害。
function M.AdjustPlayerDamage(player, damage)
    return damage
end

return M
