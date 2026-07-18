-- 关卡刷怪和掉落配置。
-- EnemySpawner 读取 enemy_spawn，PickUpGenerator 读取 drops。
return {
    -- 敌人生成节奏与默认对象池。
    enemy_spawn = {
        -- 初始间隔、最短间隔、加速系数和达到最高难度所需时间。
        initial_interval = 0.5,
        min_interval = 0.5,
        acceleration_factor = 1,
        max_difficulty_time = 300,
        -- pool_name 用于对象池取对象；prefabKey 保留给资源热更新兼容入口。
        pool_name = "Silm",
        prefabKey = "enemy/slim",
    },
    -- 各掉落物对象池、资源 key 和百分比权重。
    drops = {
        Exp = {
            poolName = "Exp",
            prefabKey = "pickup/exp",
            percentage = 60,
        },
        Potion = {
            poolName = "Potion",
            prefabKey = "pickup/blood",
            percentage = 10,
        },
    }
}
