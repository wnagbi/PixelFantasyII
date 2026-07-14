local skill_config = require("config.skill_config")

local M = {}

function M.OnSkill(host, skillId)
    if skillId == 1 then
        if host:CanUseSkill(1) then
            host:TriggerMagnet(skill_config.magnet_cd)
        end
        return true
    elseif skillId == 2 then
        if host:CanUseSkill(2) then
            host:TriggerRage(skill_config.rage_extra_damage, skill_config.rage_duration, skill_config.rage_cd)
        end
        return true
    elseif skillId == 3 then
        if host:CanUseSkill(3) then
            host:TriggerDimensionSlash(skill_config.dimension_slash_cd)
        end
        return true
    end

    return false
end

return M
