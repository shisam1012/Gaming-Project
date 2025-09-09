using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    private Board board;

    public enum StoneType { Orange, Blue, Green, Pink }

    [SerializeField] private StoneType type;
    public StoneType Type => type;

    public int column;
    public int row;

    // NEW: lock the whole drag to one type
    private StoneType activeDragType;
    private bool hasActiveDragType = false;

    private bool isDragging = false;
    private readonly List<Vector2Int> draggedStones = new List<Vector2Int>();
    private readonly List<Stone> draggedStoneObjects = new List<Stone>();

    [HideInInspector] public Color originalColor;
    private SpriteRenderer spriteRenderer;

    private const int MinToDragOver = 3;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        column = Mathf.RoundToInt(transform.position.x);
        row    = Mathf.RoundToInt(transform.position.y);
    }

    public void Init(Board boardRef) { board = boardRef; }

    private void OnMouseDown()
    {
        if (board == null || board.IsBusy) return;

        isDragging = true;
        hasActiveDragType = false;
        ResetDraggedHighlights();
        draggedStones.Clear();
        draggedStoneObjects.Clear();

        // first tile sets the lock type
        AddStoneToPath(column, row);
        if (draggedStoneObjects.Count > 0)
        {
            activeDragType = draggedStoneObjects[0].Type;
            hasActiveDragType = true;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging || board == null || board.IsBusy) return;

        var cam = Camera.main;
        if (!cam) return;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x < 0 || x >= board.Width || y < 0 || y >= board.Height) return;

        Vector2Int newPos = new Vector2Int(x, y);

        // backtrack one step
        if (draggedStones.Count > 1 && newPos == draggedStones[draggedStones.Count - 2])
        {
            var lastStoneObj = draggedStoneObjects[draggedStoneObjects.Count - 1];
            if (lastStoneObj) lastStoneObj.ResetHighlight();
            draggedStones.RemoveAt(draggedStones.Count - 1);
            draggedStoneObjects.RemoveAt(draggedStoneObjects.Count - 1);
            return;
        }

        if (draggedStones.Contains(newPos)) return;

        // adjacency (diagonals allowed). If you want 4-way only: if (dx + dy != 1) return;
        if (draggedStones.Count > 0)
        {
            Vector2Int lastPos = draggedStones[draggedStones.Count - 1];
            int dx = Mathf.Abs(lastPos.x - x);
            int dy = Mathf.Abs(lastPos.y - y);
            if (dx > 1 || dy > 1) return;
        }

        // TYPE LOCK: compare against the starting stone's type
        var currStone = board.GetStone(x, y);
        if (currStone == null) return;
        if (hasActiveDragType && currStone.Type != activeDragType) return;

        AddStoneToPath(x, y);
    }

    private void OnMouseUp()
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

    private void AddStoneToPath(int x, int y)
    {
        if (board == null) return;

        var stone = board.GetStone(x, y);
        if (!stone) return;

        // if this is the first tile of the drag, lock the type here too (redundant safety)
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
