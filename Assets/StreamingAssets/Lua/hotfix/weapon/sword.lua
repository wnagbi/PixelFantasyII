-- 飞剑热更规则。目标选择和逐帧追踪由 SwordController / EnemyManager 负责。
local M = {}

function M.OnAttack(host)
    -- 飞剑没有周期性生成攻击，移动仍由 C# Host 的 Update 处理。
end

-- 调整速度、数量和伤害，最后重建飞剑并重新分配有效目标。
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
