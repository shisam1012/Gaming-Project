// FILE: Board.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-100)]
public class Board : MonoBehaviour
{
    [Header("Board Size & Prefabs")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Stone[] stonePrefabsRef;

    [Header("Input / UI")]
    [SerializeField] private EventSystem es;

    [Header("Runtime Grids (debug viewable)")]
    [SerializeField] private Stone[,] allStones;
    private BackGroundTile[,] allTiles;

    public int Width  => width;
    public int Height => height;

    private bool isBusy = false;
    public bool IsBusy => isBusy;

    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1), 
        new Vector2Int(0, -1),  
        new Vector2Int(-1, 0), 
        new Vector2Int(1, 0)  
    };

    private void Awake()
    {
        EnsureEventSystemAndRaycaster();
    }

    private void Start()
    {
        allTiles  = new BackGroundTile[width, height];
        allStones = new Stone[width, height];
        SetUp();

        EnsureSolvableAfterRefill();
    }

#if UNITY_EDITOR
    // Editor hotkeys to test without menus:
    // R       = reshuffle once (ALWAYS)
    // Shift+R = force at least one reshuffle, then ensure solvable
    // D       = dump board state to Console
    private void Update()
    {
        if (!Application.isPlaying || isBusy) return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            DumpBoardToConsole();
            return;
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
            Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[Board] Hotkey: Force reshuffle then ensure solvable");
            EnsureSolvableAfterRefill(forceAtLeastOnce: true);
            return;
        }

        // R => reshuffle once (always)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[Board] Hotkey: Reshuffle once (always)");
            ReshuffleStones();
            DumpBoardToConsole();
        }
    }
#endif

    private void SetUp()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2(x, y);

                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.name = $"({x}_{y})";
                var bgTile = cell.GetComponent<BackGroundTile>();
                if (bgTile != null)
                {
                    bgTile.SetTileType(BackGroundTile.BackgroundType.Sand);
                    allTiles[x, y] = bgTile;
                }

                int idx = Random.Range(0, stonePrefabsRef.Length);
                var stone = Instantiate(stonePrefabsRef[idx], pos, Quaternion.identity, transform);
                stone.Init(this);
                stone.column = x;
                stone.row = y;
                stone.name = $"({x}_{y})_stone";
                allStones[x, y] = stone;
            }
        }


        allTiles[width - 1, height - 1].SetTileType(BackGroundTile.BackgroundType.Fluid);
    }


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


    public void RemoveStones(List<Vector2Int> stonePositions)
    {
        if (isBusy) return;
        StartCoroutine(RemoveStonesRoutine(stonePositions));
    }

    private IEnumerator RemoveStonesRoutine(List<Vector2Int> stonePositions)
    {
        isBusy = true;

        foreach (var pos in stonePositions)
        {
            var s = GetStone(pos.x, pos.y);
            if (s == null) continue;

            Destroy(s.gameObject);
            SetStone(pos.x, pos.y, null);
        }


        FlipBackgroundTiles(stonePositions);


        FloodFillFluid();


        CollapseAllColumns();
        yield return null;

        RefillBoard();
        yield return null;


        EnsureSolvableAfterRefill();

        isBusy = false;
    }

    private void CollapseAllColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int writeY = 0;
            for (int y = 0; y < height; y++)
            {
                var s = GetStone(x, y);
                if (s == null) continue;

                if (y != writeY)
                {
                    SetStone(x, writeY, s);
                    SetStone(x, y, null);

                    s.column = x;
                    s.row = writeY;
                    s.transform.position = new Vector2(x, writeY);
                }
                writeY++;
            }
        }
    }

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
                stone.name = $"({x}_{y})_stone";
                SetStone(x, y, stone);
            }
        }
    }

    private void FlipBackgroundTiles(IEnumerable<Vector2Int> cleared)
    {
        if (allTiles == null) return;
        foreach (var p in cleared)
        {
            if (!InBounds(p.x, p.y)) continue;
            var tile = allTiles[p.x, p.y];
            if (tile != null && tile.Type == BackGroundTile.BackgroundType.Sand)
            {
                tile.SetTileType(BackGroundTile.BackgroundType.Empty);
            }
        }
    }

    private void FloodFillFluid()
    {
        var start = new Vector2Int(width - 1, height - 1); 

        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var tile = allTiles[current.x, current.y];

            if (tile.Type == BackGroundTile.BackgroundType.Empty)
            {
                tile.SetTileType(BackGroundTile.BackgroundType.Fluid);
            }

            foreach (var dir in directions)
            {
                var next = current + dir;
                if (!InBounds(next.x, next.y)) continue;
                if (visited.Contains(next)) continue;

                var nextTile = allTiles[next.x, next.y];
                if (nextTile.Type == BackGroundTile.BackgroundType.Empty ||
                    nextTile.Type == BackGroundTile.BackgroundType.Fluid)
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
    }


    private void EnsureSolvableAfterRefill(bool forceAtLeastOnce = false)
    {
        int attempts = 0;

        if (forceAtLeastOnce)
        {
            ReshuffleStones();
            attempts++;
        }

        while (!HasAnyMatchOf3() && attempts < 25)
        {
            ReshuffleStones();
            attempts++;
        }

        if (attempts > 0)
            Debug.Log($"[Board] Reshuffled {attempts} time(s).");

#if UNITY_EDITOR
        DumpBoardToConsole();
#endif
    }

    private bool HasAnyMatchOf3()
    {
        var visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (visited[x, y]) continue;

                var s = GetStone(x, y);
                if (s == null) continue;

                int size = FloodCountSameType(x, y, s.Type, visited);
                if (size >= 3) return true; 
            }
        }
        return false; 
    }

    private int FloodCountSameType(int sx, int sy, Stone.StoneType type, bool[,] visited)
    {
        if (!InBounds(sx, sy)) return 0;
        var start = GetStone(sx, sy);
        if (start == null || start.Type != type) return 0;

        int count = 0;
        var q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(sx, sy));
        visited[sx, sy] = true;

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            count++;

            for (int i = 0; i < directions.Length; i++)
            {
                var n = p + directions[i];
                if (!InBounds(n.x, n.y)) continue;
                if (visited[n.x, n.y]) continue;

                var sn = GetStone(n.x, n.y);
                if (sn != null && sn.Type == type)
                {
                    visited[n.x, n.y] = true;
                    q.Enqueue(n);
                }
            }
        }

        return count;
    }

    private void ReshuffleStones()
    {
        var list = new List<Stone>(width * height);


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var s = GetStone(x, y);
                if (s != null)
                {
                    list.Add(s);
                    SetStone(x, y, null);
                }
            }
        }


        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }

        // place back
        int k = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (k >= list.Count) continue;
                var s = list[k++];
                s.column = x;
                s.row = y;
                s.transform.position = new Vector2(x, y);
                SetStone(x, y, s);
            }
        }
    }


#if UNITY_EDITOR
    [ContextMenu("Debug/Force Reshuffle (Always)")]
    public void ForceReshuffleOnce()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Board] Play Mode only for reshuffle.");
            return;
        }
        ReshuffleStones();
        Debug.Log("[Board] Forced reshuffle (once).");
        DumpBoardToConsole();
    }

    [ContextMenu("Debug/Force Reshuffle Until Solvable (Even If Already Solvable)")]
    public void ForceReshuffleUntilSolvable_Always()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Board] Play Mode only for reshuffle.");
            return;
        }
        EnsureSolvableAfterRefill(forceAtLeastOnce: true);
    }

    [ContextMenu("Debug/Force Reshuffle Until Solvable")]
    public void ForceReshuffleUntilSolvable()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Board] Play Mode only for reshuffle.");
            return;
        }
        EnsureSolvableAfterRefill();
    }

    [ContextMenu("Debug/Dump Board To Console")]
    public void DumpBoardToConsole()
    {
        if (allStones == null) return;
        Debug.Log("==== BOARD DUMP ====");
        for (int y = height - 1; y >= 0; y--)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append($"y={y}: ");
            for (int x = 0; x < width; x++)
            {
                var s = GetStone(x, y);
                char c = s == null ? '.' : s.Type.ToString()[0];
                sb.Append(c).Append(' ');
            }
            Debug.Log(sb.ToString());
        }
        Debug.Log("=====================");
    }
#endif


    private void EnsureEventSystemAndRaycaster()
    {
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
        {
            cam.gameObject.AddComponent<Physics2DRaycaster>();
        }

        if (es == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
        else
        {
#if ENABLE_INPUT_SYSTEM
            if (es.GetComponent<InputSystemUIInputModule>() == null && es.GetComponent<StandaloneInputModule>() == null)
            {
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (es.GetComponent<StandaloneInputModule>() == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}
