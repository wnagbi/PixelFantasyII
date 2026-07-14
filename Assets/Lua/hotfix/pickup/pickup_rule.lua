local pickup_config = require("config.pickup_config")

local M = {}

function M.GetExpValue(pickup, value)
    local cfg = pickup_config.Exp or {}
    return value * (cfg.value_multiplier or 1)
end

function M.GetHealValue(pickup, value)
    local cfg = pickup_config.Blood or {}
    return value * (cfg.heal_multiplier or 1)
end

return M
