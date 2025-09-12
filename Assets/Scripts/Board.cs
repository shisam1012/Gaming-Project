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

    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject cellPrefab; 
    [SerializeField] private Stone[] stonePrefabsRef;
    [SerializeField] private EventSystem es;

    [SerializeField] private Stone[,] allStones;
    private BackGroundTile[,] allTiles;

    public int Width  => width;
    public int Height => height;


    private bool isBusy = false;
    public bool IsBusy => isBusy;

    //-- 4-WAY ONLY (no diagonals) --
    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // up
        new Vector2Int(0, -1),  // down
        new Vector2Int(-1, 0),  // left
        new Vector2Int(1, 0)    // right
    };


    //[System.Obsolete]
    private void Awake()
    {
        EnsureEventSystemAndRaycaster();
    }

    private void Start()
    {
        allTiles  = new BackGroundTile[width, height];
        allStones = new Stone[width, height];
        SetUp();
    }


    private void SetUp()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2(x, y);

                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.name = $"( {x}_{y} )";
                var bgTile = cell.GetComponent<BackGroundTile>();
                if (bgTile != null)
                {
                    //bgTile.SetTileSprite(0);
                    bgTile.SetTileType(BackGroundTile.BackgroundType.Sand);
                    allTiles[x, y] = bgTile;
                }
              
                int idx = Random.Range(0, stonePrefabsRef.Length);
                var stone = Instantiate(stonePrefabsRef[idx], pos, Quaternion.identity, transform);
                stone.Init(this);
                stone.column = x;
                stone.row = y;
                stone.name = $"( {x}_{y} )_stone";
                allStones[x, y] = stone;
            }
        }
        allTiles[width -1, height-1].SetTileType(BackGroundTile.BackgroundType.Fluid);

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
                stone.name = $"( {x}_{y} )_stone";
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
            //tile?.SetTileSprite(2);
            if (tile != null && tile.Type == BackGroundTile.BackgroundType.Sand)
            {
                tile.SetTileType(BackGroundTile.BackgroundType.Empty); 
            }
        }
    }

    private void FloodFillFluid()
    {
        var start = new Vector2Int(width - 1, height - 1); //starting point of the fluid in the board

        var queue = new Queue<Vector2Int>(); //for indices to explore
        var visited = new HashSet<Vector2Int>(); //for visited indices

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var tile = allTiles[current.x, current.y];

            // fill with fluid only the empty tiles
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
                // enqueue neighbors if they are empty or already with fluid to allow the spreading
                if (nextTile.Type == BackGroundTile.BackgroundType.Empty || nextTile.Type == BackGroundTile.BackgroundType.Fluid)
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
    }


    //[System.Obsolete]
    private void EnsureEventSystemAndRaycaster()
    {
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
        {
            cam.gameObject.AddComponent<Physics2DRaycaster>();
        }

       //var es = FindObjectOfType<EventSystem>();
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
