local M = {}

function M.OnAttack(host)
    host:ResetCooldown()
    host:ShootLaser(host.funnels)
end

function M.OnLevelUp(host)
    local level = host.level
    if level == 0 then
        host.level = host.level + 1
    elseif level == 1 then
        host.count = host.count + 1
        host.level = host.level + 1
    elseif level == 2 then
        host.damage = host.damage + 5
        host.level = host.level + 1
    elseif level == 3 then
        host.count = host.count + 1
        host.level = host.level + 1
    elseif level == 4 then
        host.speed = host.speed * 2
        host.level = host.level + 1
    elseif level == 5 then
        host.cooldownDuration = host.cooldownDuration / 2
        host:MarkWeaponLevelMax("Funnel")
    end

    host:RebuildFunnels()
end

return M
