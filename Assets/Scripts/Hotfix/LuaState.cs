using System;
using UnityEngine;
using XLua;

// 把项目原来的 IState 状态机桥接到 Lua。
// Enemy 可以继续使用 C# 的状态机框架，但每个状态的 OnEnter/OnUpdate/OnExit 由 Lua table 决定。
public class LuaState : IState
{
    // owner 是状态所属对象，当前主要是 Enemy。
    // Lua 每次回调都会收到 owner，因此可以读写 enemySpeed、isHurt、TransitionState 等 C# 成员。
    private readonly object owner;
    private readonly string luaModuleName;
    private readonly string stateName;

    // moduleTable 是 require 返回的根 table；onEnter 等函数来自指定 stateName 的子 table。
    private LuaTable moduleTable;
    private LuaFunction onEnter;
    private LuaFunction onUpdate;
    private LuaFunction onFixedUpdate;
    private LuaFunction onExit;

    public LuaState(object owner, string luaModule, string stateName)
    {
        // luaModule 例如 hotfix.enemy.enemy_state，stateName 例如 EnemyMove。
        this.owner = owner;
        luaModuleName = luaModule;
        this.stateName = stateName;
        LoadModule();
    }

    public void OnEnter()
    {
        // 转发 IState.OnEnter -> Lua EnemyMove.OnEnter(owner)。
        Call(onEnter);
    }

    public void OnUpData()
    {
        // 项目原接口拼写是 OnUpData，这里保持接口不变，转发到 Lua 的 OnUpdate。
        Call(onUpdate);
    }

    public void OnFixUpData()
    {
        // 转发固定帧逻辑，通常放物理移动或碰撞相关状态判断。
        Call(onFixedUpdate);
    }

    public void OnExit()
    {
        // 离开状态时通知 Lua。
        Call(onExit);
    }

    private void LoadModule()
    {
        // 没有模块名时不接 Lua，外层状态机会根据情况回退。
        if (string.IsNullOrWhiteSpace(luaModuleName))
        {
            return;
        }

        if (!LuaManager.Instance.TryRequireTable(luaModuleName, out moduleTable))
        {
            // Lua 状态模块加载失败时不抛异常，避免敌人生成直接失败。
            return;
        }

        LuaTable stateTable = null;
        try
        {
            stateTable = string.IsNullOrWhiteSpace(stateName)
                ? moduleTable
                : moduleTable.Get<LuaTable>(stateName);

            if (stateTable == null)
            {
                // Lua 模块里没有对应状态 table，则这个状态什么都不做。
                return;
            }

            // 状态函数都允许缺省；缺省时对应生命周期不执行 Lua。
            onEnter = stateTable.Get<LuaFunction>("OnEnter");
            onUpdate = stateTable.Get<LuaFunction>("OnUpdate");
            onFixedUpdate = stateTable.Get<LuaFunction>("OnFixedUpdate");
            onExit = stateTable.Get<LuaFunction>("OnExit");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaState] Failed to load {luaModuleName}.{stateName}: {ex.Message}");
        }
        finally
        {
            if (stateTable != null && stateTable != moduleTable)
            {
                stateTable.Dispose();
            }
        }
    }

    private void Call(LuaFunction func)
    {
        // 每个状态回调统一从这里进入，Lua 报错只输出 Warning。
        if (func == null)
        {
            return;
        }

        try
        {
            // owner 固定作为第一个参数传给 Lua。
            func.Call(owner);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaState] {luaModuleName}.{stateName} failed: {ex.Message}");
        }
    }
}
