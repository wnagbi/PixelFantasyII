-- 主动技能热更规则。
-- Lua 决定解锁/CD条件和技能参数，C# Host 负责 UnityEvent、UI、特效和 DOTween。
local skill_config = require("config.skill_config")

local M = {}

-- 返回 true 表示该技能 ID 已由 Lua 接管，C# 不再执行默认释放分支。
function M.OnSkill(host, skillId)
    if skillId == 1 then
        -- 磁铁：由 C# 执行全场拾取 UnityEvent。
        if host:CanUseSkill(1) then
            host:TriggerMagnet(skill_config.magnet_cd)
        end
        return true
    elseif skillId == 2 then
        -- 狂怒：Lua 传入额外伤害、持续时间和冷却。
        if host:CanUseSkill(2) then
            host:TriggerRage(skill_config.rage_extra_damage, skill_config.rage_duration, skill_config.rage_cd)
        end
        return true
    elseif skillId == 3 then
        -- 次元斩：C# 负责 Addressables 特效实例化和伤害表现。
        if host:CanUseSkill(3) then
            host:TriggerDimensionSlash(skill_config.dimension_slash_cd)
        end
        return true
    end

    return false
end

return M
