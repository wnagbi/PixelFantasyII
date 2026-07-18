-- 龙卷风热更规则。持久化实体的随机移动、减速和持续伤害仍在 C# 中执行。
local M = {}

function M.OnUpdate(host, deltaTime)
    -- 当前没有额外 Lua 逐帧行为；等级变化时会统一重建现有龙卷风。
end

function M.OnAttack(host)
    -- 龙卷风没有独立攻击节拍，伤害由实体碰撞停留触发。
end

-- 调整伤害、数量和移动范围，并重建龙卷风实体。
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
