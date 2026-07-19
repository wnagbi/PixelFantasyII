-- 镰刀热更规则。Lua 负责环形发射角度和成长，Scythe C# 实体负责飞行与碰撞。
local M = {}

-- 用途：按 count 将镰刀均匀分布到 360 度方向并生成投射物。
-- 使用注意：count 至少为 1，且生成必须通过 Host 接口以使用运行时 Addressables Prefab。
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

-- 用途：调整镰刀伤害、冷却和发射数量，并处理满级状态。
-- 使用注意：冷却值不得降到 0 以下；每次调用只推进一个等级。
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
