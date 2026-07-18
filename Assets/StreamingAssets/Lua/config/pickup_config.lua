-- 拾取物数值与吸附表现配置。
-- nil 表示 Lua 不覆盖该字段，C# 继续使用 Inspector 中配置的值。
return {
    -- 经验球配置。
    Exp = {
        -- 拾取后经验值倍率。
        value_multiplier = 1,
        -- 自动吸附距离和移动速度；nil 时保留 C# 默认值。
        pickup_distance = nil,
        move_speed = nil,
    },
    -- 血瓶配置。
    Blood = {
        -- 回复生命值倍率。
        heal_multiplier = 1,
    }
}
