local M = {}

function M.OnAttack(host)
    host:ResetCooldown()
    local rotation = host.rotationPoint.transform.rotation.eulerAngles
    host.rotationPoint.transform.rotation = CS.UnityEngine.Quaternion.Euler(0, 0, rotation.z + host.speed * CS.UnityEngine.Time.deltaTime)
end

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
