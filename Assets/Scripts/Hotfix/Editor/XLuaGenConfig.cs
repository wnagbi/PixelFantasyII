using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XLua;

// xLua 代码生成配置。
// 这里列出的类型会影响 XLua/Generate Code 生成出来的桥接代码。
// 规则简单记：Lua 要访问的 C# 类型放 LuaCallCSharp；C# 要用强类型委托接 Lua 函数时放 CSharpCallLua。
public static class XLuaGenConfig
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>
    {
        // Unity 常用类型：Lua 武器/敌人逻辑会访问 Transform、Time、Debug、物理组件等。
        typeof(GameObject),
        typeof(Component),
        typeof(Transform),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Quaternion),
        typeof(Mathf),
        typeof(UnityEngine.Random),
        typeof(Time),
        typeof(Debug),
        typeof(Rigidbody2D),
        typeof(Collider2D),
        typeof(Physics2D),
        typeof(LayerMask),
        typeof(Animator),
        typeof(SpriteRenderer),
        typeof(Image),
        typeof(UnityEvent),

        // 项目核心数据与玩家类型：Lua 可以读写玩家数值、调用玩家/经验相关方法。
        typeof(PlayerData),
        typeof(Player),
        typeof(PlayerStateType),
        typeof(ExpController),

        // 武器系统：Lua 武器模块通过 HotfixWeaponController 访问 damage、count、prefab 等 Host 字段。
        typeof(Weapon),
        typeof(WeaponController),
        typeof(HotfixWeaponController),
        typeof(MissileController),
        typeof(KnifeController),
        typeof(ScytheController),
        typeof(SwordController),
        typeof(FunnelController),
        typeof(TornadoController),
        typeof(NewController),

        // 技能、敌人、刷怪、拾取、任务等单机核心玩法入口。
        typeof(SkillController),
        typeof(Enemy),
        typeof(EnemyStateType),
        typeof(EnemySpawner),
        typeof(PickUp),
        typeof(PickUpTpye),
        typeof(PickUpGenerator),
        typeof(TaskController),
        typeof(ObjPoolManager),
        typeof(AudioController),
        typeof(WeaponSelectController)
    };

    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>
    {
        // C# 如果要把 Lua 函数当成这些委托类型保存/调用，就需要生成对应适配代码。
        // 当前大部分代码直接使用 LuaFunction.Call，保留这些委托是为了后续扩展。
        typeof(Action),
        typeof(Action<object>),
        typeof(Action<object, float>),
        typeof(Func<object, bool>),
        typeof(Func<object, int, bool>)
    };
}
