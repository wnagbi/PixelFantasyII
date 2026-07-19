-- 环绕刀热更规则。Lua 修改旋转和成长参数，C# 负责碰撞与统一伤害结算。
local M = {}

-- 用途：按速度和 deltaTime 持续旋转环绕刀根节点。
-- 使用注意：该入口可能逐帧执行，不要在这里反复重建刀阵或加载资源。
function M.OnAttack(host)
    host:ResetCooldown()
    local rotation = host.rotationPoint.transform.rotation.eulerAngles
    host.rotationPoint.transform.rotation = CS.UnityEngine.Quaternion.Euler(0, 0, rotation.z + host.speed * CS.UnityEngine.Time.deltaTime)
end

-- 用途：调整环绕刀伤害、数量和转速，并重建刀阵应用最新配置。
-- 使用注意：每次调用只推进一个等级；重建必须通过 host:RebuildKnives 保持 Prefab 注入流程。
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
