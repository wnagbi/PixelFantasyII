-- 玩家可热更规则，由 Player、ExpController 和 DamageSystem 调用。
local M = {}

-- 旧版玩家受伤修正入口，新版 damage_rule.lua 不可用时作为 fallback。
function M.AdjustDamage(player, damage)
    return damage
end

-- 升级入口。这里复用 C# 默认扣经验/加等级流程，并把剩余经验返回给 ExpController。
function M.OnLevelUp(expController, currentExperience)
    expController:DefaultLevelUp()
    return expController.currentExperience
end

return M
