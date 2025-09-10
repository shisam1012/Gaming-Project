// FILE: Stone.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]  // make sure raycasts can hit this
public class Stone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Board board;

    public enum StoneType { Orange, Blue, Green, Pink }

    [SerializeField] private StoneType type;
    public StoneType Type => type;

    [HideInInspector] public int column;
    [HideInInspector] public int row;

    // Lock a whole drag to the first stone's type
    private StoneType activeDragType;
    private bool hasActiveDragType = false;

    private bool isDragging = false;
    private readonly List<Vector2Int> draggedStones = new List<Vector2Int>();
    private readonly List<Stone> draggedStoneObjects = new List<Stone>();

    [HideInInspector] public Color originalColor;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private const int MinToDragOver = 3;

    private void Reset()
    {
        // auto-size collider to sprite on add
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (spriteRenderer && boxCollider)
        {
            boxCollider.isTrigger = false;
            boxCollider.size = spriteRenderer.bounds.size;
            boxCollider.offset = Vector2.zero;
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (spriteRenderer) originalColor = spriteRenderer.color;

        // safety: if sprite changes at runtime, keep collider in sync once
        if (spriteRenderer && boxCollider && boxCollider.size.sqrMagnitude < 0.0001f)
        {
            boxCollider.size = spriteRenderer.bounds.size;
            boxCollider.offset = Vector2.zero;
        }
    }

    private void Start()
    {
        // Ensure we’re snapped to grid indices
        column = Mathf.RoundToInt(transform.position.x);
        row    = Mathf.RoundToInt(transform.position.y);

        // Make sure z=0 so raycasts are clean with an ortho camera
        var p = transform.position;
        if (Mathf.Abs(p.z) > 0.0001f)
            transform.position = new Vector3(p.x, p.y, 0f);
    }

    public void Init(Board boardRef) => board = boardRef;

    // ----------------------------
    // Touch / Pointer input hooks
    // ----------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        if (board == null || board.IsBusy) return;

        isDragging = true;
        hasActiveDragType = false;

        ResetDraggedHighlights();
        draggedStones.Clear();
        draggedStoneObjects.Clear();

        // Start from this stone
        AddStoneToPath(column, row);

        // Lock drag to first stone type
        if (draggedStoneObjects.Count > 0)
        {
            activeDragType = draggedStoneObjects[0].Type;
            hasActiveDragType = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || board == null || board.IsBusy) return;

        var cam = Camera.main;
        if (cam == null) return;

        Vector2 worldPos = cam.ScreenToWorldPoint(eventData.position);
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x < 0 || x >= board.Width || y < 0 || y >= board.Height) return;

        Vector2Int newPos = new Vector2Int(x, y);

        // Backtrack one step
        if (draggedStones.Count > 1 && newPos == draggedStones[draggedStones.Count - 2])
        {
            var lastStoneObj = draggedStoneObjects[draggedStoneObjects.Count - 1];
            if (lastStoneObj) lastStoneObj.ResetHighlight();

            draggedStones.RemoveAt(draggedStones.Count - 1);
            draggedStoneObjects.RemoveAt(draggedStoneObjects.Count - 1);
            return;
        }

        // Already in the path? ignore
        if (draggedStones.Contains(newPos)) return;

        // 8-way adjacency (diagonals allowed). For 4-way only, require dx+dy==1.
        if (draggedStones.Count > 0)
        {
            Vector2Int lastPos = draggedStones[draggedStones.Count - 1];
            int dx = Mathf.Abs(lastPos.x - x);
            int dy = Mathf.Abs(lastPos.y - y);
            if (dx > 1 || dy > 1) return;
        }

        // TYPE LOCK: only allow tiles of the starting type
        var currStone = board.GetStone(x, y);
        if (currStone == null) return;
        if (hasActiveDragType && currStone.Type != activeDragType) return;

        AddStoneToPath(x, y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if (draggedStones.Count >= MinToDragOver && board != null && !board.IsBusy)
        {
            board.RemoveStones(draggedStones);
        }

        ResetDraggedHighlights();
        draggedStones.Clear();
        draggedStoneObjects.Clear();
        hasActiveDragType = false;
    }

    // ----------------------------
    // Path helpers
    // ----------------------------
    private void AddStoneToPath(int x, int y)
    {
        if (board == null) return;

        var stone = board.GetStone(x, y);
        if (!stone) return; // ignore empty cells (during/after gravity)

        // if it's the first tile, lock the type here too (extra safety)
        if (!hasActiveDragType)
        {
            activeDragType = stone.Type;
            hasActiveDragType = true;
        }

        stone.Highlight();
        draggedStoneObjects.Add(stone);
        draggedStones.Add(new Vector2Int(x, y));
    }

    private void ResetDraggedHighlights()
    {
        for (int i = 0; i < draggedStoneObjects.Count; i++)
        {
            var s = draggedStoneObjects[i];
            if (s) s.ResetHighlight();
        }
    }

    public void Highlight()
    {
        if (spriteRenderer) spriteRenderer.color = originalColor * 1.5f;
    }

    public void ResetHighlight()
    {
        if (spriteRenderer) spriteRenderer.color = originalColor;
    }
}
