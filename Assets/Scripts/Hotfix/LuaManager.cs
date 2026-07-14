using System;
using System.IO;
using System.Text;
using UnityEngine;
using XLua;

// LuaManager 是整个 xLua 运行时的入口。
// 它负责创建 LuaEnv、注册 Lua 文件加载器、执行 main.lua，并提供 C# 调 Lua 的统一入口。
public sealed class LuaManager : MonoBehaviour
{
    private static LuaManager instance;

    // xLua 的虚拟机环境。所有 require、DoString、LuaFunction 调用都依赖它。
    private LuaEnv luaEnv;

    // 防止重复 Init。LuaEnv 一个运行期只需要初始化一次，Reload 时才会 Dispose 后重建。
    private bool initialized;

    // 单例入口。任何系统只要访问 LuaManager.Instance，就能确保场景里有一个 LuaManager。
    public static LuaManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("[LuaManager]");
                instance = go.AddComponent<LuaManager>();

                // 只在真正运行游戏时跨场景保留；编辑器工具执行代码时不强行 DontDestroy。
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(go);
                }
            }

            return instance;
        }
    }

    public LuaEnv Env
    {
        get
        {
            // 外部拿 Env 前先确保 Init 完成，避免空引用。
            Init();
            return luaEnv;
        }
    }

    // 在第一个场景加载前自动启动 Lua。
    // 这样武器、技能、敌人等组件 Start 时，Lua 环境已经准备好了。
    // 服务器 Lua 热更新模式下，不能在这里自动 Init。
    // 否则 LuaManager 会在下载服务器 zip 前读取旧 Lua，热更新就失去意义。
    // 现在由 GameBootstrap 在 LuaHotfixRemoteUpdater.CheckAndApply() 完成后手动 Init。
    private static void Bootstrap()
    {
        Instance.Init();
    }

    public void Init()
    {
        if (initialized)
        {
            return;
        }

        // 确保热更目录存在。打包后玩家/测试者可以把 Lua 覆盖文件放到这个目录。
        LuaLoader.EnsureHotfixRoot();

        luaEnv = new LuaEnv();

        // 注册自定义加载器。Lua require("a.b") 时会走 LuaLoader.Load 查找 a/b.lua。
        luaEnv.AddLoader(LuaLoader.Load);
        initialized = true;
        Debug.Log($"[LuaManager] LuaHotfix path: {LuaLoader.HotfixRoot}");

        // main.lua 是 Lua 侧入口，适合做全局初始化、打印版本、预加载配置等。
        SafeDoString("require('main')", "main");
    }

    // 直接执行一段 Lua 字符串。一般用于测试或少量工具代码，不建议玩法里到处拼字符串。
    public object[] DoString(string code)
    {
        Init();
        return luaEnv.DoString(code);
    }

    // C# 侧 require Lua 模块。moduleName 使用点号形式，例如 hotfix.weapon.missile。
    public object[] Require(string moduleName)
    {
        Init();
        string chunk = $"return require('{moduleName}')";
        return luaEnv.DoString(chunk, moduleName);
    }

    // 读取返回 table 的 Lua 模块。项目里的规则模块一般都 return M，所以常用这个接口。
    public LuaTable RequireTable(string moduleName)
    {
        object[] results = Require(moduleName);
        if (results != null && results.Length > 0)
        {
            return results[0] as LuaTable;
        }

        return null;
    }

    // 安全 require：Lua 文件缺失或报错时只打 Warning，并返回 false。
    // 这样 C# Host 可以继续走默认逻辑，不会因为热更文件错误导致游戏崩溃。
    public bool TryRequireTable(string moduleName, out LuaTable table)
    {
        table = null;
        try
        {
            table = RequireTable(moduleName);
            return table != null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaManager] Failed to require '{moduleName}': {ex.Message}");
            return false;
        }
    }

    public void Reload()
    {
        // 整体重载 LuaEnv。适合调试，但会清掉所有 Lua 状态。
        Dispose();
        Init();
    }

    // 清掉 require 缓存。Lua 的 require 默认只加载一次，改文件后必须清缓存才会重新读取。
    public void ClearRequireCache(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName) || luaEnv == null)
        {
            return;
        }

        SafeDoString($"package.loaded['{moduleName}'] = nil", $"clear:{moduleName}");
    }

    // 重新加载单个模块：先清 package.loaded，再 require。
    // 调试某个 Lua 文件时比 Reload 整个 LuaEnv 更轻。
    public object[] ReloadModule(string moduleName)
    {
        ClearRequireCache(moduleName);
        return Require(moduleName);
    }

    // xLua 建议定期 Tick，用于处理 Lua 侧 GC 等维护工作。
    public void Tick()
    {
        if (luaEnv != null)
        {
            luaEnv.Tick();
        }
    }

    public void Dispose()
    {
        initialized = false;
        if (luaEnv == null)
        {
            return;
        }

        try
        {
            luaEnv.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaManager] Dispose failed: {ex.Message}");
        }
        finally
        {
            luaEnv = null;
        }
    }

    private void Update()
    {
        // 每帧 Tick LuaEnv，保持 xLua 内部状态健康。
        Tick();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            Dispose();
            instance = null;
        }
    }

    // 安全执行 Lua 字符串。启动 main.lua 或清缓存时使用，错误只打 Warning。
    private object[] SafeDoString(string code, string chunkName)
    {
        try
        {
            return luaEnv.DoString(code, chunkName);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaManager] Failed to execute '{chunkName}': {ex.Message}");
            return null;
        }
    }
}

// LuaLoader 决定 require 时从哪里找 Lua 文件。
// 当前项目加载优先级：LuaHotfix > StreamingAssets/Lua > Assets/Lua。
public static class LuaLoader
{
    // 打包后真正热更覆盖目录。用户本地替换这里的 Lua 文件即可热更。
    public static readonly string HotfixRoot = Path.Combine(Application.persistentDataPath, "LuaHotfix");

    // 包内基础 Lua。打包时会把 Assets/Lua 同步到这里。
    public static readonly string StreamingRoot = Path.Combine(Application.streamingAssetsPath, "Lua");

    // 编辑器开发目录。没有热更文件和 StreamingAssets 文件时才回退到这里。
    public static readonly string AssetRoot = Path.Combine(Application.dataPath, "Lua");

    // xLua 自定义 Loader 签名。filepath 传进来是模块名，例如 hotfix.weapon.missile。
    public static byte[] Load(ref string filepath)
    {
        string relativePath = filepath.Replace('.', '/');
        string luaFile = relativePath + ".lua";

        // 最高优先级：本地热更目录。
        if (TryRead(Path.Combine(HotfixRoot, luaFile), out byte[] hotfixBytes))
        {
            filepath = Path.Combine(HotfixRoot, luaFile);
            return hotfixBytes;
        }

        // 第二优先级：包内 StreamingAssets。
        if (TryRead(Path.Combine(StreamingRoot, luaFile), out byte[] streamingBytes))
        {
            filepath = Path.Combine(StreamingRoot, luaFile);
            return streamingBytes;
        }

        // 最后回退：Assets/Lua，主要用于编辑器开发期。
        if (TryRead(Path.Combine(AssetRoot, luaFile), out byte[] assetBytes))
        {
            filepath = Path.Combine(AssetRoot, luaFile);
            return assetBytes;
        }

        return null;
    }

    // 统一读取 Lua 文件为 UTF8 字节。xLua Loader 返回 byte[]。
    private static bool TryRead(string path, out byte[] bytes)
    {
        bytes = null;
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            bytes = Encoding.UTF8.GetBytes(File.ReadAllText(path, Encoding.UTF8));
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaLoader] Failed to read '{path}': {ex.Message}");
            return false;
        }
    }

    // 启动时创建热更目录，并在 Console 打印路径，方便你知道打包后 LuaHotfix 放哪里。
    public static void EnsureHotfixRoot()
    {
        try
        {
            if (!Directory.Exists(HotfixRoot))
            {
                Directory.CreateDirectory(HotfixRoot);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaLoader] Failed to create hotfix path '{HotfixRoot}': {ex.Message}");
        }
    }
}
