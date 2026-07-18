-- 环绕刀热更规则。Lua 修改旋转和成长参数，C# 负责碰撞与统一伤害结算。
local M = {}

-- 环绕刀是持续表现型武器，每次调用通过旋转根节点更新刀阵方向。
function M.OnAttack(host)
    host:ResetCooldown()
    local rotation = host.rotationPoint.transform.rotation.eulerAngles
    host.rotationPoint.transform.rotation = CS.UnityEngine.Quaternion.Euler(0, 0, rotation.z + host.speed * CS.UnityEngine.Time.deltaTime)
end

-- 升级后调整伤害、数量或转速，并重建刀阵以应用最新数量。
function M.OnLevelUp(host)
    local level = host.level
    if level == 0 then
        host.level = host.level + 1
    elseif level == 1 then
        host.damage = host.damage + 5
        host.level = host.level + 1
    elseif level == 2 then
        host.count = host.count + 2
        host.level = host.level + 1
    elseif level == 3 then
        host.count = host.count + 2
        host.level = host.level + 1
    elseif level == 4 then
        host.count = host.count + 2
        host.level = host.level + 1
    elseif level == 5 then
        host.speed = host.speed * 2
        host:MarkWeaponLevelMax("Knifle")
    end

    host:RebuildKnives()
end

return M
