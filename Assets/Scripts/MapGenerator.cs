using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 简单的无限地图 Tile 生成器。
// 以玩家所在格子为中心生成一定范围内的 Tile，并删除远离玩家的 Tile。
public class MapGenerator : MonoBehaviour
{
    public Tilemap tileMap;
    public TileBase tile;
    private Transform player;

    // 玩家周围保留 Tile 的格子半径。
    public int renderDistance; //Generator distance

    // 记录上一次玩家所在格子，只有跨格移动时才重新生成/删除 Tile。
    private Vector3 lastPlayerPos;
    private bool initialized;
    private bool missingPlayerWarningShown;

    private void Start()
    {
        // 单机玩家存在时优先绑定单机 Player。
        TryInitialize();
    }
    private void Update()
    {
        if (!initialized && !TryInitialize())
        {
            return;
        }

        Vector3 currentPlayerCell = tileMap.WorldToCell(player.position);
        if (currentPlayerCell != lastPlayerPos) // Check whether player movew
        {
            // 玩家移动到新格子后，补齐周围新 Tile，并清理远处 Tile。
            GenerateTiles();
            DeleteTiles();
            lastPlayerPos = currentPlayerCell;

        }
    }

    /// <summary>
    /// 尝试执行 TryInitialize，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 MapGenerator 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private bool TryInitialize()
    {
        if (tileMap == null)
        {
            return false;
        }

        if (player == null && !PlayerRuntimeRegistry.TryGetPlayerTransform(out player))
        {
            if (!missingPlayerWarningShown)
            {
                Debug.LogWarning("[MapGenerator] Player is not registered yet. Tile generation will wait.", this);
                missingPlayerWarningShown = true;
            }

            return false;
        }

        GenerateTiles();
        lastPlayerPos = tileMap.WorldToCell(player.position);
        initialized = true;
        return true;
    }
    /// <summary>
    /// 生成 MapGenerator 中与 GenerateTiles 对应的内容。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 MapGenerator 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void GenerateTiles()
    {
        // 以玩家所在格子为中心，把 renderDistance 范围内缺失的 Tile 补上。
        Vector3Int playerCell = tileMap.WorldToCell(player.position);


        for (int x = playerCell.x - renderDistance; x <= playerCell.x + renderDistance; x++)
        {
            for (int y = playerCell.y - renderDistance; y <= playerCell.y + renderDistance; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);


                if (!tileMap.HasTile(cellPosition))
                {
                    tileMap.SetTile(cellPosition, tile);
                }
            }
        }
    }
    /// <summary>
    /// 遍历当前 Tilemap 已有范围，删除超过渲染距离的格子，控制 Tile 数量。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 MapGenerator 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void DeleteTiles()
    {
        // 遍历当前 Tilemap 已有范围，删除超过渲染距离的格子，控制 Tile 数量。
        Vector3Int playerCell = tileMap.WorldToCell(player.position);



        foreach (var position in tileMap.cellBounds.allPositionsWithin)
        {
            if (Vector3Int.Distance(position, playerCell) > renderDistance)
            {
                tileMap.SetTile(position, null);
            }
        }
    }


}
