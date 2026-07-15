using UnityEngine;

// Stores the currently active scene Player without scanning the scene at runtime.
public static class PlayerRuntimeRegistry
{
    public static Player Current { get; private set; }

    public static Transform CurrentTransform
    {
        get { return Current != null ? Current.transform : null; }
    }

    public static void Register(Player player)
    {
        if (player == null)
        {
            return;
        }

        Current = player;
    }

    public static void Unregister(Player player)
    {
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
        player = Current;
        return player != null;
    }

    public static bool TryGetPlayerTransform(out Transform playerTransform)
    {
        playerTransform = CurrentTransform;
        return playerTransform != null;
    }
}
