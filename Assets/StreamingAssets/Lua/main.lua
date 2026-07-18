-- xLua 启动入口。
-- LuaManager.Init() 创建 LuaEnv 后会执行本文件，用于确认脚本环境已经可用。
print("[Lua] main.lua loaded")

-- 返回当前包内 Lua 版本信息；后续可以在调试面板中读取并展示。
return {
    version = "0.1.0"
}
