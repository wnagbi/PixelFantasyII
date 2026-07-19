-- 拾取物数值规则。配置与行为分离，方便只调整倍率而不改 C#。
local pickup_config = require("config.pickup_config")

local M = {}

-- 用途：计算经验球被拾取时最终增加的经验值。
-- 使用注意：必须返回数字；pickup 是 C# PickUp 实例，配置缺失时倍率应回退为 1。
function M.GetExpValue(pickup, value)
    local cfg = pickup_config.Exp or {}
    return value * (cfg.value_multiplier or 1)
end

-- 用途：计算血瓶被拾取时最终恢复的生命值。
-- 使用注意：必须返回数字；不要在这里直接修改 PlayerData，C# 会应用返回结果。
function M.GetHealValue(pickup, value)
    local cfg = pickup_config.Blood or {}
    return value * (cfg.heal_multiplier or 1)
end

return M
