-- 所有热更武器的公共元数据和 Addressables Prefab key。
-- 配置 key 必须与各 Controller 的 Lua 配置名一致。
return {
    missile = {
        -- select_name 用于满级后从升级候选列表移除。
        select_name = "Missle",
        -- level_max 是规则层约定的最高等级。
        level_max = 5,
        -- prefabKey 是 Addressables Address；加载失败时使用 Inspector prefab。
        prefabKey = "weapon/projectile/missile",
    },
    knife = {
        select_name = "Knifle",
        level_max = 5,
        prefabKey = "weapon/projectile/knife",
    },
    scythe = {
        select_name = "Scythe",
        level_max = 5,
        prefabKey = "weapon/projectile/scythe",
    },
    sword = {
        select_name = "Sword",
        level_max = 5,
        prefabKey = "weapon/projectile/sword",
    },
    funnel = {
        select_name = "Funnel",
        level_max = 5,
        -- 浮游炮主体和激光使用两个独立资源 key。
        prefabKey = "weapon/projectile/funnel",
        laserKey = "weapon/projectile/funnel_laser",
    },
    tornado = {
        select_name = "Tornado",
        level_max = 5,
        prefabKey = "weapon/projectile/tornado",
    },
    new_weapon = {
        -- 示例扩展武器尚未配置远端 Prefab，空 key 会直接使用 Inspector 引用。
        select_name = "Funnel",
        level_max = 5,
        prefabKey = "",
    }
}
