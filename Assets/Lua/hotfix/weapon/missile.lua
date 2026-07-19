-- 导弹热更规则和公共配置。
-- host 是 HotfixMissileController；生成 Prefab、动画和碰撞仍由 C# 完成。
local config = require("config.weapon_config").missile

local M = {}

-- 用途：在导弹 Host 与 Addressables Prefab 准备完成后初始化可热更参数。
-- 使用注意：不要在这里生成攻击对象；仅校正参数，且 cooldownDuration 必须保持大于 0。
function M.OnStart(host)
    if host.cooldownDuration <= 0 then
        host.cooldownDuration = 1
    end
    -- host.count = 5
    -- host.cooldownDuration = 0.3
end

-- 用途：预留导弹逐帧热更行为入口。
-- 使用注意：该函数每帧调用，应避免资源加载、场景查找和大量临时对象分配。
function M.OnUpdate(host, deltaTime)
    -- 当前导弹没有额外逐帧 Lua 行为，保留接口方便后续扩展。
end

-- 用途：在玩家周围的圆环范围内按 count 随机生成导弹。
-- 使用注意：必须调用 host:ResetCooldown；生成应通过 Host 接口以复用 Addressables fallback 和组件注入。
function M.OnAttack(host)
    host:ResetCooldown()

    local count = host.count
    if count < 1 then
        count = 1
    end

    for i = 1, count do
        local radius = CS.UnityEngine.Random.Range(host.missileAttackRangeIn, host.missileAttackRangeOut)
        local angle = CS.UnityEngine.Random.Range(0, 2 * CS.UnityEngine.Mathf.PI)
        local x = radius * CS.UnityEngine.Mathf.Cos(angle)
        local y = radius * CS.UnityEngine.Mathf.Sin(angle)
        host:SpawnPrefabAt(x, y)
    end
end

-- 用途：修改导弹冷却、伤害和数量，并在满级时退出升级候选列表。
-- 使用注意：每次调用只能推进一个等级；selectName 必须与 C# 武器选择配置一致。
function M.OnLevelUp(host)
    local level = host.level

    if level == 0 then
        host.level = host.level + 1
    elseif level == 1 then
        host.cooldownDuration = host.cooldownDuration - 1
        host.level = host.level + 1
    elseif level == 2 then
        host.damage = host.damage + 10
        host.level = host.level + 1
    elseif level == 3 then
        host.level = host.level + 1
    elseif level == 4 then
        host.timer = host.timer / 2
        host.level = host.level + 1
    elseif level == 5 then
        host.damage = host.damage * 2
        host:MarkWeaponLevelMax(config.select_name)
    end
end

return M
