local M = {}

function M.OnAttack(host)
    host:ResetCooldown()
    local count = host.count
    if count < 1 then
        count = 1
    end

    for i = 0, count - 1 do
        local rotation = 360 * i / count
        host:SpawnPrefabUnder(host.shootPoint.transform, 0, 0, rotation)
    end
end

function M.OnLevelUp(host)
    local level = host.level
    if level == 0 then
        host.level = host.level + 1
    elseif level == 1 then
        host.damage = host.damage + 5
        host.level = host.level + 1
    elseif level == 2 then
        host.damage = host.damage + 2
        host.level = host.level + 1
    elseif level == 3 then
        host.cooldownDuration = host.cooldownDuration / 2
        host.level = host.level + 1
    elseif level == 4 then
        host.count = host.count + 2
        host.level = host.level + 1
    elseif level == 5 then
        host.damage = host.damage + 10
        host:MarkWeaponLevelMax("Scythe")
    end
end

return M
