using UnityEngine;

// 当前场景玩家运行时注册表。
// Player 在 OnEnable/OnDisable 注册和注销，其他系统无需使用 FindObjectOfType 扫描场景。
public static class PlayerRuntimeRegistry
{
    // 当前激活玩家；单机战斗场景约定只存在一个 Player。
    public static Player Current { get; private set; }

    public static Transform CurrentTransform
    {
        get { return Current != null ? Current.transform : null; }
    }

    public static void Register(Player player)
    {
        // 新场景玩家启用时覆盖旧引用。
        if (player == null)
        {
            return;
        }

        Current = player;
    }

    public static void Unregister(Player player)
    {
        // 只允许当前注册者清空引用，避免旧对象禁用时误删新玩家。
        if (Current == player)
        {
            Current = null;
        }
    }
    public static Player GetPlayer()
    {
        return Current;
    }

    public static bool TryGetPlayer(out Player player)
    {
        // Try 模式让调用方在玩家尚未生成时安全等待下一帧。
        player = Current;
        return player != null;
    }

    public static bool TryGetPlayerTransform(out Transform playerTransform)
    {
        // 大多数追踪/刷怪逻辑只需要 Transform，不必依赖完整 Player。
        playerTransform = CurrentTransform;
        return playerTransform != null;
    }
}
