local M = {}

-- 用途：在敌人扣血前统一修正 C# 已计算出的伤害值。
-- 使用注意：必须返回有效数字；enemy 是 C# Enemy 实例，异常时 C# 会回退旧规则或默认伤害。
function M.AdjustEnemyDamage(enemy, damage)
    return damage
end

-- 用途：在玩家扣血前统一修正敌人接触或其他来源传入的伤害值。
-- 使用注意：必须返回有效数字；不要在这里直接修改玩家血量，实际扣血仍由 DamageSystem 完成。
function M.AdjustPlayerDamage(player, damage)
    return damage
end

return M
