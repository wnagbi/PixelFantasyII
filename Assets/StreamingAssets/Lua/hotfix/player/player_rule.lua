local M = {}

function M.AdjustDamage(player, damage)
    return damage
end

function M.OnLevelUp(expController, currentExperience)
    expController:DefaultLevelUp()
    return expController.currentExperience
end

return M
