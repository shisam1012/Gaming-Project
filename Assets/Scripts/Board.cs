// FILE: Board.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace GamingProject
{
    [DefaultExecutionOrder(-100)]
    public class Board : MonoBehaviour
    {
    [Header("Board Size & Prefabs")]
    [SerializeField] private int width = 7;   // default 7x10
    [SerializeField] private int height = 10;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Stone[] stonePrefabsRef;

    [Header("Grid System")]
    [SerializeField] private Grid grid; // Drag your Grid GameObject here in Inspector
    
    [Header("Input / UI")]
    [SerializeField] private EventSystem es;

    [Header("Runtime Grids (debug viewable)")]
    private Stone[,] allStones;
    private BackGroundTile[,] allTiles;
    private bool[,] playable;

    public int Width  => width;
    public int Height => height;

    private bool isBusy = false;
    public bool IsBusy => isBusy;

    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0)
    };

    // Endpoints
    [Header("Endpoints")]
    [SerializeField] private Vector2Int fluidSource = new Vector2Int(3, 3);
    [SerializeField] private Vector2Int destination = new Vector2Int(3, 0);
    [SerializeField] private bool randomizeEndpointsOnStart = false; 

    [Header("Win Event")]
    public UnityEvent onWin;


    private LevelConfig activeLevel;
    
    // Public getter for GameManager access
    public LevelConfig ActiveLevel => activeLevel;

    // ===== Public API (called by LevelManager) =====
    public void ApplyLevel(LevelConfig level)
    {
        activeLevel = level;


        ClearBoardVisuals();


        width  = activeLevel.GetWidth();
        height = activeLevel.GetHeight();

        playable = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                playable[x, y] = activeLevel.IsPlayable(x, y);

        fluidSource = ClampToBoard(activeLevel.startInside);
        destination = ClampToBoard(activeLevel.destinationOnBorder);

        if (IsOnBorder(fluidSource))
            Debug.LogWarning("[Board] Start should be inside, but is on border.");
        if (!IsOnBorder(destination))
            Debug.LogWarning("[Board] Destination should be on border.");

        allTiles  = new BackGroundTile[width, height];
        allStones = new Stone[width, height];

        SetUp();
        EnsureSolvableAfterRefill();
    }

    private void Start()
    {
        if (UnityEngine.Object.FindFirstObjectByType<LevelManager>() != null) return;

        playable = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                playable[x, y] = true;

        allTiles  = new BackGroundTile[width, height];
        allStones = new Stone[width, height];

        if (randomizeEndpointsOnStart)
        {
            int startY = (height >= 3) ? Random.Range(1, height - 1) : 0;
            int startX = Random.Range(0, width);
            fluidSource = new Vector2Int(startX, startY);

            int endY = (Random.value < 0.5f) ? 0 : (height - 1);
            int endX = Random.Range(0, width);
            destination = new Vector2Int(endX, endY);
            if (destination == fluidSource)
                destination = new Vector2Int((endX + 1) % width, endY);
        }

        SetUp();
        EnsureSolvableAfterRefill();
    }

    private void Awake()
    {
        EnsureEventSystemAndRaycaster();
        
        // Register with GameManager
        GameManager.Instance.SetBoard(this);
        
        // Transfer stone prefabs to StoneManager if they're assigned here
        if (stonePrefabsRef != null && stonePrefabsRef.Length > 0)
        {
            StoneManager.Instance.SetStonePrefabs(stonePrefabsRef);
        }
    }

    // ---------- Clear visuals to avoid duplicates ----------
    private void ClearBoardVisuals()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var ch = transform.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(ch.gameObject);
            else Destroy(ch.gameObject);
#else
            Destroy(ch.gameObject);
#endif
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying || isBusy) return;

        // Debug input only - gameplay input is handled by TouchInputController
#if ENABLE_INPUT_SYSTEM
        // ---- New Input System (Keyboard) ----
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.dKey.wasPressedThisFrame)
        {
            DumpBoardToConsole();
            return;
        }

        bool shift = (kb.leftShiftKey?.isPressed ?? false) || (kb.rightShiftKey?.isPressed ?? false);
        if (shift && kb.rKey.wasPressedThisFrame)
        {
            EnsureSolvableAfterRefill(forceAtLeastOnce: true);
            return;
        }

        if (kb.rKey.wasPressedThisFrame)
        {
            ReshuffleStones();
            DumpBoardToConsole();
        }
#else
        // ---- Old Input Manager (fallback) ----
        if (Input.GetKeyDown(KeyCode.D)) { DumpBoardToConsole(); return; }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
            Input.GetKeyDown(KeyCode.R))
        {
            EnsureSolvableAfterRefill(forceAtLeastOnce: true);
            return;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReshuffleStones();
            DumpBoardToConsole();
        }
#endif
    }
#endif

    // ---------- Helpers ----------
    private Vector2Int ClampToBoard(Vector2Int p)
    {
        int cx = Mathf.Clamp(p.x, 0, width - 1);
        int cy = Mathf.Clamp(p.y, 0, height - 1);
        return new Vector2Int(cx, cy);
    }

    private bool IsDestinationCell(int x, int y) => (x == destination.x && y == destination.y);
    private bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
    private bool IsOnBorder(Vector2Int p) => p.x == 0 || p.x == width - 1 || p.y == 0 || p.y == height - 1;
    private bool IsPlayable(int x, int y) => InBounds(x, y) && (playable == null || playable[x, y]);

    // Grid position helpers
    private Vector3 GetWorldPosition(int x, int y)
    {
        Vector3Int cell = new Vector3Int(x, y, 0);
        if (grid != null)
        {
            return grid.GetCellCenterWorld(cell);
        }
        else
        {
            // fallback: old behavior (world units assumed)
            return new Vector3(x, y, 0f);
        }
    }

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

    // ---------- Build ----------
    private void SetUp()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsPlayable(x, y)) { allTiles[x, y] = null; allStones[x, y] = null; continue; }

                var pos = GetWorldPosition(x, y);

                // Create background tile
                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.name = $"({x}_{y})";
                var bgTile = cell.GetComponent<BackGroundTile>();
                if (bgTile != null)
                {
                    bgTile.SetTileType(BackGroundTile.BackgroundType.Sand);
                    allTiles[x, y] = bgTile;
                }

                if (IsDestinationCell(x, y))
                {
                    allStones[x, y] = null;
                    continue;
                }

                // Use StoneManager to create stones - select random type
                int idx = Random.Range(0, stonePrefabsRef.Length);
                var stoneType = stonePrefabsRef[idx].Type;
                Debug.Log($"[Board] Creating stone of type {stoneType} at ({x}, {y})");
                var stone = StoneManager.Instance.CreateStone(stoneType, pos, transform);
                if (stone != null)
                {
                    stone.column = x;
                    stone.row = y;
                    stone.name = $"({x}_{y})_stone";
                    allStones[x, y] = stone;
                    Debug.Log($"[Board] Successfully created stone: {stone.name}");
                }
                else
                {
                    Debug.LogError($"[Board] Failed to create stone at ({x}, {y})");
                }
            }
        }
        if (IsPlayable(fluidSource.x, fluidSource.y) && allTiles[fluidSource.x, fluidSource.y] != null)
            allTiles[fluidSource.x, fluidSource.y].SetTileType(BackGroundTile.BackgroundType.Fluid);
    }

    // ---------- Gameplay ----------
    public void RemoveStones(List<Vector2Int> stonePositions)
    {
        if (isBusy) return;
        
        Debug.Log($"[Board] RemoveStones called with {stonePositions.Count} stones to remove");
        
        // Update score using GameObject.Find to avoid compilation issues
        var scoreHandlerObj = GameObject.Find("ScoreHandler");
        if (scoreHandlerObj != null)
        {
            Debug.Log("[Board] Found ScoreHandler GameObject, attempting to update score...");
            var scoreHandler = scoreHandlerObj.GetComponent<MonoBehaviour>();
            var method = scoreHandler.GetType().GetMethod("UpadteScore");
            if (method != null)
            {
                Debug.Log($"[Board] Calling UpadteScore with {stonePositions.Count} stones");
                method.Invoke(scoreHandler, new object[] { stonePositions.Count });
                Debug.Log("[Board] ✓ Score update method called successfully");
            }
            else
            {
                Debug.LogError("[Board] UpadteScore method not found on ScoreHandler!");
            }
        }
        else
        {
            Debug.LogError("[Board] ScoreHandler GameObject not found! Score will not update.");
        }
        
        StartCoroutine(RemoveStonesRoutine(stonePositions));
        
    }

    private IEnumerator RemoveStonesRoutine(List<Vector2Int> stonePositions)
    {
        if (stonePositions == null || stonePositions.Count == 0) yield break;

        isBusy = true;

        foreach (var pos in stonePositions)
        {
            var s = GetStone(pos.x, pos.y);
            if (s == null) continue;

            StoneManager.Instance.DestroyStone(s);
            SetStone(pos.x, pos.y, null);
        }

        FlipBackgroundTiles(stonePositions);
        

        var flooded = FloodFillFluid();

        if (flooded.Contains(destination))
        {
            Debug.LogWarning("[WIN] Fluid reached destination! 🎉");
            onWin?.Invoke();
        }
        else
        {
            Debug.Log("[Board] Not yet: start hasn’t connected to end.");
        }

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

            while (writeY < height && (!IsPlayable(x, writeY) || IsDestinationCell(x, writeY))) writeY++;

            for (int y = 0; y < height; y++)
            {
                if (!IsPlayable(x, y)) continue;
                var s = GetStone(x, y);
                if (s == null) continue;

                while (writeY < height && (!IsPlayable(x, writeY) || IsDestinationCell(x, writeY)))
                    writeY++;

                if (writeY >= height) break;

                if (y != writeY)
                {
                    SetStone(x, writeY, s);
                    SetStone(x, y, null);

                    s.column = x;
                    s.row = writeY;
                    s.transform.position = GetWorldPosition(x, writeY);
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
                if (!IsPlayable(x, y)) continue;
                if (IsDestinationCell(x, y)) continue;
                if (GetStone(x, y) != null) continue;

                // Use StoneManager to create stones - select random type
                int idx = Random.Range(0, stonePrefabsRef.Length);
                var stoneType = stonePrefabsRef[idx].Type;
                var stone = StoneManager.Instance.CreateStone(stoneType, GetWorldPosition(x, y), transform);
                if (stone != null)
                {
                    stone.column = x;
                    stone.row = y;
                    stone.name = $"({x}_{y})_stone";
                    SetStone(x, y, stone);
                }
            }
        }
    }

    private void FlipBackgroundTiles(IEnumerable<Vector2Int> cleared)
    {
        if (allTiles == null) return;
        foreach (var p in cleared)
        {
            if (!IsPlayable(p.x, p.y)) continue;
            var tile = allTiles[p.x, p.y];
            if (tile != null && tile.Type == BackGroundTile.BackgroundType.Sand)
            {
                tile.SetTileType(BackGroundTile.BackgroundType.Empty);
            }
        }
    }

    private HashSet<Vector2Int> FloodFillFluid()
    {
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        if (!IsPlayable(fluidSource.x, fluidSource.y)) return visited;

        queue.Enqueue(fluidSource);
        visited.Add(fluidSource);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            var tile = InBounds(current.x, current.y) ? allTiles[current.x, current.y] : null;
            if (tile != null && tile.Type == BackGroundTile.BackgroundType.Empty)
                tile.SetTileType(BackGroundTile.BackgroundType.Fluid);

            foreach (var dir in directions)
            {
                var next = current + dir;
                if (!IsPlayable(next.x, next.y)) continue;
                if (visited.Contains(next)) continue;

                if (next == destination)
                {
                    var destTile = allTiles[next.x, next.y];
                    if (destTile != null) destTile.SetTileType(BackGroundTile.BackgroundType.Fluid);
                    visited.Add(next);
                    queue.Enqueue(next);
                    continue;
                }

                var nextTile = allTiles[next.x, next.y];
                if (nextTile != null &&
                   (nextTile.Type == BackGroundTile.BackgroundType.Empty ||
                    nextTile.Type == BackGroundTile.BackgroundType.Fluid))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }

    // ---------- Playability / Debug ----------
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
                if (!IsPlayable(x, y)) continue;
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
        if (!IsPlayable(sx, sy)) return 0;
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
                if (!IsPlayable(n.x, n.y)) continue;
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
                if (!IsPlayable(x, y)) continue;
                if (IsDestinationCell(x, y)) continue;

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

        int k = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsPlayable(x, y)) continue;
                if (IsDestinationCell(x, y)) continue;
                if (k >= list.Count) continue;

                var s = list[k++];
                s.column = x;
                s.row = y;
                s.transform.position = GetWorldPosition(x, y);
                SetStone(x, y, s);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Force Reshuffle (Always)")]
    public void ForceReshuffleOnce()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Board] Play Mode only for reshuffle."); return; }
        ReshuffleStones();
        Debug.Log("[Board] Forced reshuffle (once).");
        DumpBoardToConsole();
    }

    [ContextMenu("Debug/Force Reshuffle Until Solvable (Even If Already Solvable)")]
    public void ForceReshuffleUntilSolvable_Always()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Board] Play Mode only for reshuffle."); return; }
        EnsureSolvableAfterRefill(forceAtLeastOnce: true);
    }

    [ContextMenu("Debug/Force Reshuffle Until Solvable")]
    public void ForceReshuffleUntilSolvable()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Board] Play Mode only for reshuffle."); return; }
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
                if (!IsPlayable(x, y)) { sb.Append('#').Append(' '); continue; }
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
}
