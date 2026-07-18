-- 关卡运行时刷怪规则。
-- 返回 nil 表示不覆盖 EnemySpawner 当前对象池名称，继续使用 stage_config 默认值。
local M = {}

-- 可以根据 spawner.gameTime 等 C# 字段动态切换敌人池。
function M.GetSpawnPoolName(spawner)
    return nil
end

return M
