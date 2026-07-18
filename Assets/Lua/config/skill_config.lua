-- 主动技能规则和 Addressables 资源 key 配置。
-- SkillController 读取 CD/持续时间/伤害，UI 使用 skills 表加载图标与特效。
return {
    -- 磁铁技能冷却时间。
    magnet_cd = 10,
    -- 狂怒技能冷却、持续时间和额外伤害。
    rage_cd = 10,
    rage_duration = 5,
    rage_extra_damage = 20,
    -- 次元斩冷却时间。
    dimension_slash_cd = 10,
    -- 技能 ID 必须与 Skill、SkillController 和玩家存档中的 ID 保持一致。
    skills = {
        [1] = {
            -- iconKey/effectKey 是 Addressables Address；空字符串表示使用 Inspector fallback。
            iconKey = "ui/skill/icon_magnet",
            effectKey = "",
        },
        [2] = {
            iconKey = "ui/skill/icon_rage",
            effectKey = "",
        },
        [3] = {
            iconKey = "ui/skill/icon_dimension_slash",
            effectKey = "vfx/skill/dimension_slash",
        },
    },
}
