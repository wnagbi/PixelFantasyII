-- 浮游炮热更规则。浮游炮实体和激光实例仍由 FunnelController 管理。
local M = {}

-- 每次冷却结束后重置 CD，并让所有浮游炮发射激光。
function M.OnAttack(host)
    host:ResetCooldown()
    host:ShootLaser(host.funnels)
end

-- 按当前等级修改数量、伤害、速度或冷却，最后重建浮游炮阵列。
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
