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

    /// <summary>
    /// 注册当前场景中激活的玩家实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Player.OnEnable 调用；单机场景应只有一个权威玩家。
    /// </remarks>
    public static void Register(Player player)
    {
        // 新场景玩家启用时覆盖旧引用。
        if (player == null)
        {
            return;
        }

        Current = player;
    }

    /// <summary>
    /// 在玩家禁用或销毁时注销当前实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅当传入实例就是 Current 时才清空，避免旧对象注销新玩家。
    /// </remarks>
    public static void Unregister(Player player)
    {
        // 只允许当前注册者清空引用，避免旧对象禁用时误删新玩家。
        if (Current == player)
        {
            Current = null;
        }
    }
    /// <summary>
    /// 获取当前玩家；尚未注册时返回 null。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用方必须处理场景加载早期或玩家禁用期间的 null。
    /// </remarks>
    public static Player GetPlayer()
    {
        return Current;
    }

    /// <summary>
    /// 尝试取得当前激活玩家并通过 out 参数返回。
    /// </summary>
    /// <remarks>
    /// 使用注意：失败时不要进行场景扫描，可在下一帧或下次生成流程中重试。
    /// </remarks>
    public static bool TryGetPlayer(out Player player)
    {
        // Try 模式让调用方在玩家尚未生成时安全等待下一帧。
        player = Current;
        return player != null;
    }

    /// <summary>
    /// 尝试取得当前玩家 Transform，供追踪、刷怪和地图系统使用。
    /// </summary>
    /// <remarks>
    /// 使用注意：注册表不拥有 Transform 生命周期，玩家注销后不能继续缓存旧引用。
    /// </remarks>
    public static bool TryGetPlayerTransform(out Transform playerTransform)
    {
        // 大多数追踪/刷怪逻辑只需要 Transform，不必依赖完整 Player。
        playerTransform = CurrentTransform;
        return playerTransform != null;
    }
}
