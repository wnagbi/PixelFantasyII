using System;
using UnityEngine;
using XLua;

// 通用 Lua 组件桥。
// 把这个脚本挂到 GameObject 上，并填写 luaModule，就可以把 Unity 生命周期转发给 Lua table。
// 适合后续做轻量逻辑，不适合直接替代所有复杂 MonoBehaviour。
public class LuaBehaviour : MonoBehaviour
{
    // Lua 模块名，例如 hotfix.some_logic。
    // 模块应 return 一个 table，并按需提供 Awake/Start/Update 等函数。
    public string luaModule;

    // require 得到的模块 table，以及从 table 中缓存出来的生命周期函数。
    private LuaTable moduleTable;
    private LuaFunction awakeFunc;
    private LuaFunction startFunc;
    private LuaFunction updateFunc;
    private LuaFunction fixedUpdateFunc;
    private LuaFunction onEnableFunc;
    private LuaFunction onDisableFunc;
    private LuaFunction onDestroyFunc;

    private void Awake()
    {
        // Awake 阶段先加载模块，随后调用 Lua 的 Awake(host)。
        LoadModule();
        Call(awakeFunc);
    }

    private void Start()
    {
        // 转发 Unity Start。
        Call(startFunc);
    }

    private void Update()
    {
        // 转发 Unity Update，并把 deltaTime 传给 Lua。
        Call(updateFunc, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // 转发 Unity FixedUpdate，并把 fixedDeltaTime 传给 Lua。
        Call(fixedUpdateFunc, Time.fixedDeltaTime);
    }

    private void OnEnable()
    {
        // 对象池反复启用对象时，Awake 不会重复执行；这里确保重新启用时 Lua 模块已加载。
        if (moduleTable == null)
        {
            LoadModule();
        }

        Call(onEnableFunc);
    }

    private void OnDisable()
    {
        // 转发 Unity OnDisable。这里不释放 Lua 引用，因为对象池对象可能还会再次启用。
        Call(onDisableFunc);
    }

    private void OnDestroy()
    {
        // 真正销毁对象时，先通知 Lua，再释放 xLua 引用。
        Call(onDestroyFunc);
        DisposeLuaRefs();
    }

    /// <summary>
    /// 加载 LuaBehaviour 中与 LoadModule 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaBehaviour 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void LoadModule()
    {
        // 已加载或没有填写模块名时直接返回。
        if (moduleTable != null || string.IsNullOrWhiteSpace(luaModule))
        {
            return;
        }

        if (!LuaManager.Instance.TryRequireTable(luaModule, out moduleTable))
        {
            // Lua 缺失或报错时不影响 GameObject 创建。
            return;
        }

        // 这些函数都允许缺省；Lua 没写哪个生命周期，C# 就不会调用哪个。
        awakeFunc = moduleTable.Get<LuaFunction>("Awake");
        startFunc = moduleTable.Get<LuaFunction>("Start");
        updateFunc = moduleTable.Get<LuaFunction>("Update");
        fixedUpdateFunc = moduleTable.Get<LuaFunction>("FixedUpdate");
        onEnableFunc = moduleTable.Get<LuaFunction>("OnEnable");
        onDisableFunc = moduleTable.Get<LuaFunction>("OnDisable");
        onDestroyFunc = moduleTable.Get<LuaFunction>("OnDestroy");
    }

    /// <summary>
    /// 统一调用入口。所有 Lua 生命周期错误都只打 Warning，避免中断 Unity 生命周期。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaBehaviour 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void Call(LuaFunction func, params object[] args)
    {
        // 统一调用入口。所有 Lua 生命周期错误都只打 Warning，避免中断 Unity 生命周期。
        if (func == null)
        {
            return;
        }

        try
        {
            if (args == null || args.Length == 0)
            {
                // 生命周期函数的第一个参数固定是 host，也就是当前 LuaBehaviour。
                func.Call(this);
            }
            else if (args.Length == 1)
            {
                func.Call(this, args[0]);
            }
            else
            {
                object[] callArgs = new object[args.Length + 1];
                callArgs[0] = this;
                Array.Copy(args, 0, callArgs, 1, args.Length);
                func.Call(callArgs);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaBehaviour] {luaModule} call failed: {ex.Message}", this);
        }
    }

    /// <summary>
    /// 释放 LuaBehaviour 持有的非托管引用或运行时资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 LuaBehaviour 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void DisposeLuaRefs()
    {
        // xLua 要求 LuaTable/LuaFunction 用完释放，否则 LuaEnv.Dispose 时可能提示引用未释放。
        awakeFunc?.Dispose();
        startFunc?.Dispose();
        updateFunc?.Dispose();
        fixedUpdateFunc?.Dispose();
        onEnableFunc?.Dispose();
        onDisableFunc?.Dispose();
        onDestroyFunc?.Dispose();
        moduleTable?.Dispose();

        awakeFunc = null;
        startFunc = null;
        updateFunc = null;
        fixedUpdateFunc = null;
        onEnableFunc = null;
        onDisableFunc = null;
        onDestroyFunc = null;
        moduleTable = null;
    }
}
