-- 飞剑热更规则。目标选择和逐帧追踪由 SwordController / EnemyManager 负责。
local M = {}

-- 用途：保留飞剑攻击钩子，实际追踪和移动继续由 SwordController 执行。
-- 使用注意：当前不应在这里周期性生成飞剑，避免与 C# 重建逻辑重复。
function M.OnAttack(host)
    -- 飞剑没有周期性生成攻击，移动仍由 C# Host 的 Update 处理。
end

-- 用途：调整飞剑速度、数量和伤害，并重建飞剑重新分配有效目标。
-- 使用注意：目标选择统一交给 EnemyManager；不要在 Lua 中缓存对象池敌人引用。
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
