-- 新武器模板规则，演示如何复用 HotfixWeaponController 的旋转、重建和满级接口。
local M = {}

-- 持续旋转轨道根节点。
function M.OnAttack(host)
    host:ResetCooldown()
    local rotation = host.rotationPoint.transform.rotation.eulerAngles
    host.rotationPoint.transform.rotation = CS.UnityEngine.Quaternion.Euler(0, 0, rotation.z + host.speed * CS.UnityEngine.Time.deltaTime)
end

-- 示例成长：每级增加一个轨道物体，满级后停止进入升级候选。
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
