-- 任务目标与奖励规则，读取静态 task_config 并驱动 C# TaskController。
local task_config = require("config.task_config")

local M = {}

-- 用途：根据任务 ID 返回可热更新的任务目标数量。
-- 使用注意：配置缺失时必须返回 fallback，避免任务目标变成 nil 或 0。
function M.GetTaskGoal(taskController, taskId, fallback)
    local task = task_config.tasks and task_config.tasks[taskId]
    if task and task.goal then
        return task.goal
    end
    return fallback
end

-- 用途：执行任务完成奖励并通知 C# 是否已经由 Lua 处理。
-- 使用注意：返回 true 会阻止 C# fallback；只有确实发放奖励后才能返回 true。
function M.OnTaskComplete(taskController, taskId)
    local task = task_config.tasks and task_config.tasks[taskId]
    if task and task.reward == "weapon_select" then
        taskController.selectWeapon:Invoke()
        return true
    end
    return false
end

return M
