-- 关卡运行时刷怪规则。
-- 返回 nil 表示不覆盖 EnemySpawner 当前对象池名称，继续使用 stage_config 默认值。
local M = {}

-- 用途：根据关卡时间或刷怪器状态动态选择本次生成使用的对象池。
-- 使用注意：返回 nil 表示继续使用 stage_config 默认池名；返回的名称必须与 ObjPool.poolName 一致。
function M.GetSpawnPoolName(spawner)
    return nil
end

return M
