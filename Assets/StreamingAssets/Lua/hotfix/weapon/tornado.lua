-- 龙卷风热更规则。持久化实体的随机移动、减速和持续伤害仍在 C# 中执行。
local M = {}

-- 用途：预留龙卷风逐帧热更规则入口。
-- 使用注意：该函数每帧调用，应避免场景扫描、资源加载和临时 table 分配。
function M.OnUpdate(host, deltaTime)
    -- 当前没有额外 Lua 逐帧行为；等级变化时会统一重建现有龙卷风。
end

-- 用途：保留龙卷风攻击钩子，持续伤害仍由 C# 碰撞停留处理。
-- 使用注意：不要在这里重复生成龙卷风，否则会绕过 Controller 的数量管理。
function M.OnAttack(host)
    -- 龙卷风没有独立攻击节拍，伤害由实体碰撞停留触发。
end

-- 用途：调整龙卷风伤害、数量和移动范围，并重建现有实体。
-- 使用注意：重建必须通过 host:RebuildTornadoes，以保持组件 owner 注入和 Prefab fallback。
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
