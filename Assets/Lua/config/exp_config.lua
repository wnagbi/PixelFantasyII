-- 局内经验曲线配置，由 ExpController 在启动时读取。
return {
    -- 经验曲线最多扩展到的等级数量。
    level_count = 100,
    -- 后续等级需求经验相对前一级的增长倍率。
    growth_multiplier = 1.1,
}
