-- 新武器模板规则，演示如何复用 HotfixWeaponController 的旋转、重建和满级接口。
local M = {}

-- 用途：持续旋转新武器模板的轨道根节点。
-- 使用注意：该入口可能逐帧调用，不要在这里反复生成轨道对象。
function M.OnAttack(host)
    host:ResetCooldown()
    local rotation = host.rotationPoint.transform.rotation.eulerAngles
    host.rotationPoint.transform.rotation = CS.UnityEngine.Quaternion.Euler(0, 0, rotation.z + host.speed * CS.UnityEngine.Time.deltaTime)
end

-- 用途：演示通过 Lua 增加轨道对象数量并处理满级状态。
-- 使用注意：这是模板规则，复制新武器时必须同步修改满级选择名称和对应 C# Host 接口。
function M.OnLevelUp(host)
    local level = host.level
    if level >= 0 and level < 5 then
        host.count = host.count + 1
        host.level = host.level + 1
    elseif level == 5 then
        host:MarkWeaponLevelMax("Funnel")
    end

    host:RebuildOrbitObjects()
end

return M
