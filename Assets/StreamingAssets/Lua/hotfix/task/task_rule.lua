local task_config = require("config.task_config")

local M = {}

function M.GetTaskGoal(taskController, taskId, fallback)
    local task = task_config.tasks and task_config.tasks[taskId]
    if task and task.goal then
        return task.goal
    end
    return fallback
end

function M.OnTaskComplete(taskController, taskId)
    local task = task_config.tasks and task_config.tasks[taskId]
    if task and task.reward == "weapon_select" then
        taskController.selectWeapon:Invoke()
        return true
    end
    return false
end

return M
