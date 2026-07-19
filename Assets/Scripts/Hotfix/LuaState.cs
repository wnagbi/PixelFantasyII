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

    /// <summary>
    /// luaModule 例如 hotfix.enemy.enemy_state，stateName 例如 EnemyMove。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 LuaState 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public LuaState(object owner, string luaModule, string stateName)
    {
        // luaModule 例如 hotfix.enemy.enemy_state，stateName 例如 EnemyMove。
        this.owner = owner;
        luaModuleName = luaModule;
        this.stateName = stateName;
        LoadModule();
    }

    /// <summary>
    /// 进入当前状态并应用该状态的初始表现。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnEnter()
    {
        // 转发 IState.OnEnter -> Lua EnemyMove.OnEnter(owner)。
        Call(onEnter);
    }

    /// <summary>
    /// 逐帧执行当前状态的逻辑更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnUpData()
    {
        // 项目原接口拼写是 OnUpData，这里保持接口不变，转发到 Lua 的 OnUpdate。
        Call(onUpdate);
    }

    /// <summary>
    /// 在固定时间步执行当前状态的物理更新。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnFixUpData()
    {
        // 转发固定帧逻辑，通常放物理移动或碰撞相关状态判断。
        Call(onFixedUpdate);
    }

    /// <summary>
    /// 退出当前状态并清理该状态留下的临时效果。
    /// </summary>
    /// <remarks>
    /// 使用注意：由状态机按固定顺序调用，切换状态时必须保证 OnExit 与 OnEnter 成对执行。
    /// </remarks>
    public void OnExit()
    {
        // 离开状态时通知 Lua。
        Call(onExit);
    }

    /// <summary>
    /// 加载 LuaState 中与 LoadModule 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaState 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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

    /// <summary>
    /// 每个状态回调统一从这里进入，Lua 报错只输出 Warning。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaState 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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
