-- 导弹热更规则和公共配置。
-- host 是 HotfixMissileController；生成 Prefab、动画和碰撞仍由 C# 完成。
local config = require("config.weapon_config").missile

local M = {}

-- 武器初始化钩子，用于校正非法冷却值或应用首包热更默认值。
function M.OnStart(host)
    if host.cooldownDuration <= 0 then
        host.cooldownDuration = 1
    end
    -- host.count = 5
    -- host.cooldownDuration = 0.3
end

function M.OnUpdate(host, deltaTime)
    -- 当前导弹没有额外逐帧 Lua 行为，保留接口方便后续扩展。
end

-- 在玩家周围的圆环范围随机选择落点，并按 count 生成导弹。
function M.OnAttack(host)
    host:ResetCooldown()

    local count = host.count
    if count < 1 then
        count = 1
    end

    for i = 1, count do
        local radius = CS.UnityEngine.Random.Range(host.missileAttackRangeIn, host.missileAttackRangeOut)
        local angle = CS.UnityEngine.Random.Range(0, 2 * CS.UnityEngine.Mathf.PI)
        local x = radius * CS.UnityEngine.Mathf.Cos(angle)
        local y = radius * CS.UnityEngine.Mathf.Sin(angle)
        host:SpawnPrefabAt(x, y)
    end
end

-- 导弹等级成长规则；满级时通知 C# 从升级候选列表移除。
function M.OnLevelUp(host)
    local level = host.level

    if level == 0 then
        host.level = host.level + 1
    elseif level == 1 then
        host.cooldownDuration = host.cooldownDuration - 1
        host.level = host.level + 1
    elseif level == 2 then
        host.damage = host.damage + 10
        host.level = host.level + 1
    elseif level == 3 then
        host.level = host.level + 1
    elseif level == 4 then
        host.timer = host.timer / 2
        host.level = host.level + 1
    elseif level == 5 then
        host.damage = host.damage * 2
        host:MarkWeaponLevelMax(config.select_name)
    end
end

return M
