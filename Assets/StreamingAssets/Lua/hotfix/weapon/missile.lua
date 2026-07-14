local config = require("config.weapon_config").missile

local M = {}

function M.OnStart(host)
    if host.cooldownDuration <= 0 then
        host.cooldownDuration = 1
    end
    -- host.count = 5
    -- host.cooldownDuration = 0.3
end

function M.OnUpdate(host, deltaTime)
end

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
