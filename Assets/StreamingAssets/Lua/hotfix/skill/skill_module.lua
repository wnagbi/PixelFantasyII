-- 主动技能热更规则。
-- Lua 决定解锁/CD条件和技能参数，C# Host 负责 UnityEvent、UI、特效和 DOTween。
local skill_config = require("config.skill_config")

local M = {}

-- 用途：根据可读字符串 key 执行磁铁、狂怒或次元斩的可热更新释放规则。
-- 使用注意：返回 true 表示 Lua 已接管且 C# 不执行默认分支；未知 key 返回 false 并走 C# fallback。
function M.OnSkill(host, skillKey)
    local skill = skill_config.skills[skillKey]
    if skill == nil then
        return false
    end

    if skillKey == "magnet" then
        -- 磁铁：由 C# 执行全场拾取 UnityEvent。
        if host:CanUseSkill(skill.id) then
            host:TriggerMagnet(skill.cooldown)
        end
        return true
    elseif skillKey == "rage" then
        -- 狂怒：Lua 传入额外伤害、持续时间和冷却。
        if host:CanUseSkill(skill.id) then
            host:TriggerRage(skill.extra_damage, skill.duration, skill.cooldown)
        end
        return true
    elseif skillKey == "dimension_slash" then
        -- 次元斩：C# 负责 Addressables 特效实例化和伤害表现。
        if host:CanUseSkill(skill.id) then
            host:TriggerDimensionSlash(skill.cooldown)
        end
        return true
    end

    return false
end

return M
