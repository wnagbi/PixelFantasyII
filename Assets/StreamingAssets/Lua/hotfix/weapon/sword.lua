local M = {}

function M.OnAttack(host)
    -- Sword movement is still handled by the C# host Update loop.
end

function M.OnLevelUp(host)
    local level = host.level
    if level == 0 then
        host.level = host.level + 1
    elseif level == 1 then
        host.speed = host.speed * 2
        host.level = host.level + 1
    elseif level == 2 then
        host.count = host.count + 1
        host.level = host.level + 1
    elseif level == 3 then
        host.damage = host.damage + 5
        host.level = host.level + 1
    elseif level == 4 then
        host.count = host.count + 1
        host.level = host.level + 1
    elseif level == 5 then
        host.count = host.count + 2
        host:MarkWeaponLevelMax("Sword")
    end

    host:RebuildSwords()
end

return M
