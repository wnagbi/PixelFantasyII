-- 主动技能规则、商店价格和 Addressables 资源 key 配置。
-- skills 使用可读字符串作为键；id 只用于兼容 C# 存档中的整数技能 ID。
return {
    skills = {
        magnet = {
            id = 1,
            price = 2000,
            cooldown = 10,
            iconKey = "ui/skill/icon_magnet",
            effectKey = "",
        },
        rage = {
            id = 2,
            price = 1000,
            cooldown = 10,
            duration = 5,
            extra_damage = 20,
            iconKey = "ui/skill/icon_rage",
            effectKey = "",
        },
        dimension_slash = {
            id = 3,
            price = 1500,
            cooldown = 10,
            iconKey = "ui/skill/icon_dimension_slash",
            effectKey = "vfx/skill/dimension_slash",
        },
    },
}
