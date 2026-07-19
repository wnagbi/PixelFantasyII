using System;
using UnityEngine;
using XLua;

// LuaConfig 是“读 Lua 配置/调用 Lua 规则”的安全工具类。
// 业务代码通过这里访问 Lua，Lua 文件缺失或报错时会返回 fallback，不让游戏因为热更脚本问题崩溃。
public static class LuaConfig
{
    /// <summary>
    /// 读取 Lua 模块根 table 上的数字字段，例如 config.exp_config.level_count。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static float GetFloat(string moduleName, string key, float fallback)
    {
        // 读取 Lua 模块根 table 上的数字字段，例如 config.exp_config.level_count。
        return GetValue(moduleName, key, fallback);
    }

    /// <summary>
    /// xLua 读到的 number 可能是 double/long，这里最终会尝试转成 int。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static int GetInt(string moduleName, string key, int fallback)
    {
        // xLua 读到的 number 可能是 double/long，这里最终会尝试转成 int。
        return GetValue(moduleName, key, fallback);
    }

    /// <summary>
    /// 读取根 table 上的 bool 字段。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool GetBool(string moduleName, string key, bool fallback)
    {
        // 读取根 table 上的 bool 字段。
        return GetValue(moduleName, key, fallback);
    }

    /// <summary>
    /// 空字符串也按无效值处理，避免 Lua 配错后覆盖掉 C# 默认值。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static string GetString(string moduleName, string key, string fallback)
    {
        // 空字符串也按无效值处理，避免 Lua 配错后覆盖掉 C# 默认值。
        string value = GetValue(moduleName, key, fallback);
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    /// <summary>
    /// 读取嵌套 table 字段，例如 config.stage_config.enemy_spawn.initial_interval。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static float GetFloat(string moduleName, string tableKey, string key, float fallback)
    {
        // 读取嵌套 table 字段，例如 config.stage_config.enemy_spawn.initial_interval。
        return GetNestedValue(moduleName, tableKey, key, fallback);
    }

    /// <summary>
    /// 读取嵌套 table 的 int 配置。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static int GetInt(string moduleName, string tableKey, string key, int fallback)
    {
        // 读取嵌套 table 的 int 配置。
        return GetNestedValue(moduleName, tableKey, key, fallback);
    }

    /// <summary>
    /// 读取嵌套 table 的 bool 配置。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool GetBool(string moduleName, string tableKey, string key, bool fallback)
    {
        // 读取嵌套 table 的 bool 配置。
        return GetNestedValue(moduleName, tableKey, key, fallback);
    }

    /// <summary>
    /// 读取嵌套 table 的 string 配置。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static string GetString(string moduleName, string tableKey, string key, string fallback)
    {
        // 读取嵌套 table 的 string 配置。
        string value = GetNestedValue(moduleName, tableKey, key, fallback);
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    /// <summary>
    /// 尝试执行 TryGetTable，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool TryGetTable(string moduleName, string tableKey, out LuaTable table)
    {
        // 给需要直接遍历 Lua table 的系统使用。调用者拿到 table 后负责 Dispose。
        table = null;
        if (!LuaManager.Instance.TryRequireTable(moduleName, out LuaTable root))
        {
            return false;
        }

        try
        {
            table = root.Get<LuaTable>(tableKey);
            return table != null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaConfig] Failed to read table {moduleName}.{tableKey}: {ex.Message}");
            return false;
        }
        finally
        {
            root.Dispose();
        }
    }

    /// <summary>
    /// 尝试执行 TryCallBool，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool TryCallBool(string moduleName, string functionName, object host, int id, out bool result)
    {
        // 调用 Lua 函数并要求返回 bool，常用于“Lua 是否接管了这次行为”。
        return TryCall(moduleName, functionName, out result, host, id);
    }

    /// <summary>
    /// 尝试执行 TryCallFloat，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool TryCallFloat(string moduleName, string functionName, object host, float input, out float result)
    {
        // 调用 Lua 函数并要求返回 float，常用于伤害、经验、治疗量等数值修正。
        return TryCall(moduleName, functionName, out result, host, input);
    }

    /// <summary>
    /// 尝试执行 TryCallInt，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool TryCallInt(string moduleName, string functionName, object host, int input, out int result)
    {
        // 调用 Lua 函数并要求返回 int，常用于等级、任务目标、经验等整数规则。
        return TryCall(moduleName, functionName, out result, host, input);
    }

    /// <summary>
    /// 尝试执行 TryCallString，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool TryCallString(string moduleName, string functionName, object host, string input, out string result)
    {
        // 调用 Lua 函数并要求返回 string，常用于对象池名称、配置 key 等字符串规则。
        return TryCall(moduleName, functionName, out result, host, input);
    }

    /// <summary>
    /// 尝试执行 TryCall，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：通过本类公开入口维护统一状态，并由调用方处理失败返回或回退逻辑。
    /// </remarks>
    public static bool TryCall(string moduleName, string functionName, params object[] args)
    {
        // 不关心返回值的 Lua 调用入口。只要函数存在并成功执行，就返回 true。
        if (!LuaManager.Instance.TryRequireTable(moduleName, out LuaTable table))
        {
            return false;
        }

        LuaFunction func = null;
        try
        {
            func = table.Get<LuaFunction>(functionName);
            if (func == null)
            {
                return false;
            }

            func.Call(args);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaConfig] {moduleName}.{functionName} failed: {ex.Message}");
            return false;
        }
        finally
        {
            func?.Dispose();
            table.Dispose();
        }
    }

    /// <summary>
    /// 从 Lua 模块根 table 读取指定键并转换为目标 C# 类型。
    /// </summary>
    /// <remarks>
    /// 使用注意：模块缺失、键缺失或类型转换失败时返回 fallback；LuaTable 始终在内部释放。
    /// </remarks>
    private static T GetValue<T>(string moduleName, string key, T fallback)
    {
        // 根 table 读取的统一实现。任何失败都返回 fallback。
        if (!LuaManager.Instance.TryRequireTable(moduleName, out LuaTable table))
        {
            return fallback;
        }

        try
        {
            object value = table.Get<object>(key);
            return ConvertLuaValue(value, fallback);
        }
        catch
        {
            return fallback;
        }
        finally
        {
            table.Dispose();
        }
    }

    /// <summary>
    /// 从 Lua 模块的嵌套 table 中读取指定键并转换为目标 C# 类型。
    /// </summary>
    /// <remarks>
    /// 使用注意：外层和嵌套 LuaTable 都由本方法释放，调用方不能缓存其中的引用。
    /// </remarks>
    private static T GetNestedValue<T>(string moduleName, string tableKey, string key, T fallback)
    {
        // 嵌套 table 读取的统一实现。tableKey 不存在、字段不存在、类型不匹配都会回退。
        if (!LuaManager.Instance.TryRequireTable(moduleName, out LuaTable table))
        {
            return fallback;
        }

        LuaTable nested = null;
        try
        {
            nested = table.Get<LuaTable>(tableKey);
            if (nested == null)
            {
                return fallback;
            }

            object value = nested.Get<object>(key);
            return ConvertLuaValue(value, fallback);
        }
        catch
        {
            return fallback;
        }
        finally
        {
            nested?.Dispose();
            table.Dispose();
        }
    }

    /// <summary>
    /// 调用 Lua 函数并尝试把第一个返回值转换为指定 C# 类型。
    /// </summary>
    /// <remarks>
    /// 使用注意：返回 false 表示模块、函数、调用或转换失败；业务层必须继续使用 C# fallback。
    /// </remarks>
    private static bool TryCall<T>(string moduleName, string functionName, out T result, params object[] args)
    {
        // 有返回值的 Lua 调用统一入口：require 模块 -> 找函数 -> 执行 -> 转成目标类型。
        result = default;
        if (!LuaManager.Instance.TryRequireTable(moduleName, out LuaTable table))
        {
            return false;
        }

        LuaFunction func = null;
        try
        {
            func = table.Get<LuaFunction>(functionName);
            if (func == null)
            {
                return false;
            }

            object[] values = func.Call(args);
            if (values != null && values.Length > 0 && TryConvertLuaValue(values[0], out T typedValue))
            {
                result = typedValue;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LuaConfig] {moduleName}.{functionName} failed: {ex.Message}");
            return false;
        }
        finally
        {
            func?.Dispose();
            table.Dispose();
        }
    }

    /// <summary>
    /// 把 Lua 返回对象转换为目标类型，失败时返回调用方提供的默认值。
    /// </summary>
    /// <remarks>
    /// 使用注意：该方法不抛出类型转换异常，不能用返回值判断转换是否成功。
    /// </remarks>
    private static T ConvertLuaValue<T>(object value, T fallback)
    {
        // 转换失败时保留 C# 默认值。
        return TryConvertLuaValue(value, out T result) ? result : fallback;
    }

    /// <summary>
    /// 尝试把 Lua 的 number、string、bool 或枚举值转换为指定 C# 类型。
    /// </summary>
    /// <remarks>
    /// 使用注意：失败时 result 为 default；不支持的复杂 table 应使用 LuaTable 专用读取接口。
    /// </remarks>
    private static bool TryConvertLuaValue<T>(object value, out T result)
    {
        // Lua number/string/bool 回到 C# 后不一定刚好是目标类型，所以这里做一层宽松转换。
        result = default;
        if (value == null)
        {
            return false;
        }

        if (value is T typedValue)
        {
            result = typedValue;
            return true;
        }

        try
        {
            Type targetType = typeof(T);
            if (targetType.IsEnum)
            {
                result = (T)Enum.ToObject(targetType, value);
            }
            else
            {
                result = (T)Convert.ChangeType(value, targetType);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
