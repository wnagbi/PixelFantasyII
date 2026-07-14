local M = {}

function M.OnUpdate(host, deltaTime)
    -- Existing tornado objects are persistent; level changes rebuild them.
end

function M.OnAttack(host)
    -- Tornado currently has no active attack tick in C#; stats are hotfixed through OnLevelUp.
end

function M.OnLevelUp(host)
    local level = host.level
    if level == 0 then
        host.level = host.level + 1
    elseif level == 1 then
        host.damage = host.damage + 1
        host.level = host.level + 1
    elseif level == 2 then
        host.count = host.count + 1
        host.level = host.level + 1
    elseif level == 3 then
        host.moveRange = host.moveRange - 1
        host.level = host.level + 1
    elseif level == 4 then
        host.damage = host.damage + 2
        host.level = host.level + 1
    elseif level == 5 then
        host.moveRange = host.moveRange - 2
        host:MarkWeaponLevelMax("Funnel")
    end

    host:RebuildTornadoes()
end

return M
