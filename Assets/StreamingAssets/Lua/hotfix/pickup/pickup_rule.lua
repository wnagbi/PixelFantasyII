-- 拾取物数值规则。配置与行为分离，方便只调整倍率而不改 C#。
local pickup_config = require("config.pickup_config")

local M = {}

-- 返回经验球最终经验值；pickup 是触发拾取的 C# PickUp 实例。
function M.GetExpValue(pickup, value)
    local cfg = pickup_config.Exp or {}
    return value * (cfg.value_multiplier or 1)
end

-- 返回血瓶最终回复量。
function M.GetHealValue(pickup, value)
    local cfg = pickup_config.Blood or {}
    return value * (cfg.heal_multiplier or 1)
end

return M
