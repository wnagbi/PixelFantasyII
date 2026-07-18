-- 任务目标和奖励类型配置。
-- TaskController 仍负责显示与事件监听，task_rule.lua 负责解释本表。
return {
    tasks = {
        -- key 是任务 ID，必须和任务 XML / TaskController 中的 ID 对应。
        [1] = {
            -- goal 为完成目标；reward 由 task_rule.lua 转成具体 C# 奖励动作。
            goal = 10,
            reward = "weapon_select",
        }
    }
}
