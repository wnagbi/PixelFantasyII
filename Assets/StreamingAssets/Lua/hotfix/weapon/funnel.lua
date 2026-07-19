-- 浮游炮热更规则。浮游炮实体和激光实例仍由 FunnelController 管理。
local M = {}

-- 用途：冷却完成后命令当前全部浮游炮发射激光。
-- 使用注意：必须重置冷却；funnels 列表由 C# 管理，Lua 不应持久缓存其中的 GameObject。
function M.OnAttack(host)
    host:ResetCooldown()
    host:ShootLaser(host.funnels)
end

-- 用途：按等级修改浮游炮数量、伤害、速度和冷却，并重建阵列。
-- 使用注意：重建使用 host:RebuildFunnels；激光 Prefab 仍由 secondaryPrefabKey 管理。
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
