using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // --- Inspector (configure in Unity) ---
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject cellPrefab;       // Background cell (has BackGroundTile)
    [SerializeField] private Stone[] stonePrefabsRef;     // Stone prefabs to spawn

    // --- Runtime grids ---
    [SerializeField] private Stone[,] allStones;          // Keep private; use GetStone/SetStone
    private BackGroundTile[,] allTiles;

    // --- Public read-only for other scripts ---
    public int Width  => width;
    public int Height => height;

    // --- Busy flag to lock input while board settles ---
    private bool isBusy = false;
    public bool IsBusy => isBusy;

    private void Start()
    {
        allTiles  = new BackGroundTile[width, height];
        allStones = new Stone[width, height];
        SetUp();
    }

    // Build the board: background tiles + random stones
    private void SetUp()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2(x, y);

                // Background cell
                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.name = $"( {x}_{y} )";
                var bgTile = cell.GetComponent<BackGroundTile>();
                if (bgTile != null)
                {
                    bgTile.SetTileSprite(0);   // e.g., sand
                    allTiles[x, y] = bgTile;
                }

                // Stone
                int idx = Random.Range(0, stonePrefabsRef.Length);
                var stone = Instantiate(stonePrefabsRef[idx], pos, Quaternion.identity, transform);
                stone.Init(this);
                stone.column = x;
                stone.row = y;
                stone.name = $"( {x}_{y} )_stone";
                allStones[x, y] = stone;
            }
        }
    }

    // ===== Grid helpers (safe API) =====
    public bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    public Stone GetStone(int x, int y)
    {
        if (!InBounds(x, y)) return null;
        return allStones[x, y];
    }

    private void SetStone(int x, int y, Stone s)
    {
        if (!InBounds(x, y)) return;
        allStones[x, y] = s;
    }

    // ===== Called by Stone.cs after a valid drag =====
    public void RemoveStones(List<Vector2Int> stonePositions)
    {
        if (isBusy) return; // block re-entry while settling
        StartCoroutine(RemoveStonesRoutine(stonePositions));
    }

    private IEnumerator RemoveStonesRoutine(List<Vector2Int> stonePositions)
    {
        isBusy = true;

        // 1) Destroy stones and clear grid
        foreach (var pos in stonePositions)
        {
            var s = GetStone(pos.x, pos.y);
            if (s == null) continue;

            Destroy(s.gameObject);
            SetStone(pos.x, pos.y, null);
        }

        // 2) (Optional) Flip background tiles under cleared stones (e.g., "wet")
        FlipBackgroundTiles(stonePositions);

        // 3) Gravity (collapse)
        CollapseAllColumns();
        yield return null; // let one frame pass (or wait for tweens if you add them)

        // 4) Refill
        RefillBoard();
        yield return null;

        // 5) (Optional) Cascades would run here

        isBusy = false;
    }

    // Pack stones downward in each column
    private void CollapseAllColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int writeY = 0; // next lowest empty slot
            for (int y = 0; y < height; y++)
            {
                var s = GetStone(x, y);
                if (s == null) continue;

                if (y != writeY)
                {
                    // Move stone down in grid
                    SetStone(x, writeY, s);
                    SetStone(x, y, null);

                    // Keep stone's indices + transform in sync
                    s.column = x;
                    s.row = writeY;
                    s.transform.position = new Vector2(x, writeY);
                }
                writeY++;
            }
        }
    }

    // Fill empty cells with new random stones at their final grid positions
    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (GetStone(x, y) != null) continue;

                int idx = Random.Range(0, stonePrefabsRef.Length);
                var spawnPos = new Vector2(x, y);
                var stone = Instantiate(stonePrefabsRef[idx], spawnPos, Quaternion.identity, transform);
                stone.Init(this);
                stone.column = x;
                stone.row = y;
                stone.name = $"( {x}_{y} )_stone";
                SetStone(x, y, stone);
            }
        }
    }

    // Optional: mark background tiles affected by the clear (for goals/visuals)
    private void FlipBackgroundTiles(IEnumerable<Vector2Int> cleared)
    {
        if (allTiles == null) return;
        foreach (var p in cleared)
        {
            if (!InBounds(p.x, p.y)) continue;
            var tile = allTiles[p.x, p.y];
            tile?.SetTileSprite(2); // pick your "wet" index
        }
    }
}
