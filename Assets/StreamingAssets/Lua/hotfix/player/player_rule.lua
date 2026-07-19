-- 玩家可热更规则，由 Player、ExpController 和 DamageSystem 调用。
local M = {}

-- 用途：作为新版统一伤害规则不可用时的玩家伤害修正 fallback。
-- 使用注意：必须返回有效数字；新逻辑应优先写在 hotfix.combat.damage_rule 中。
function M.AdjustDamage(player, damage)
    return damage
end

-- 用途：接管玩家升级规则，并把处理后的剩余经验返回给 ExpController。
-- 使用注意：必须让经验或等级产生有效变化，避免 ExpController 的连续升级保护判定为无进展。
function M.OnLevelUp(expController, currentExperience)
    expController:DefaultLevelUp()
    return expController.currentExperience
end

return M
