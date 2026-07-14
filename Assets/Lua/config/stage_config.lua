return {
    enemy_spawn = {
        initial_interval = 0.5,
        min_interval = 0.5,
        acceleration_factor = 1,
        max_difficulty_time = 300,
        pool_name = "Silm",
        prefabKey = "enemy/slim",
    },
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
