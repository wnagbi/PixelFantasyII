-- 任务目标与奖励规则，读取静态 task_config 并驱动 C# TaskController。
local task_config = require("config.task_config")

local M = {}

-- 查询任务目标；配置缺失时返回 C# / XML 传入的 fallback。
function M.GetTaskGoal(taskController, taskId, fallback)
    local task = task_config.tasks and task_config.tasks[taskId]
    if task and task.goal then
        return task.goal
    end
    return fallback
end

-- 执行任务奖励。返回 true 表示 Lua 已处理，false 让 C# 执行默认奖励。
function M.OnTaskComplete(taskController, taskId)
    local task = task_config.tasks and task_config.tasks[taskId]
    if task and task.reward == "weapon_select" then
        taskController.selectWeapon:Invoke()
        return true
    end
    return false
end

return M
