// FILE: Stone.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]  // make sure raycasts can hit this
public class Stone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Board board;

    public enum StoneType
    {
        Type1,
        Type2,
        Type3,
        Type4
    }

    [SerializeField] private StoneType type;
    public StoneType Type => type;

    [HideInInspector] public int column;
    [HideInInspector] public int row;

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

        if (spriteRenderer && boxCollider && boxCollider.size.sqrMagnitude < 0.0001f)
        {
            boxCollider.size = spriteRenderer.bounds.size;
            boxCollider.offset = Vector2.zero;
        }
    }

    private void Start()
    {
        column = Mathf.RoundToInt(transform.position.x);
        row    = Mathf.RoundToInt(transform.position.y);

        // keep z=0 for raycasts
        var p = transform.position;
        if (Mathf.Abs(p.z) > 0.0001f)
            transform.position = new Vector3(p.x, p.y, 0f);
    }

    public void Init(Board boardRef) => board = boardRef;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (board == null || board.IsBusy) return;

        isDragging = true;
        hasActiveDragType = false;

        ResetDraggedHighlights();
        draggedStones.Clear();
        draggedStoneObjects.Clear();

        AddStoneToPath(column, row);

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

        if (draggedStones.Contains(newPos)) return;

        if (draggedStones.Count > 0)
        {
            Vector2Int lastPos = draggedStones[draggedStones.Count - 1];
            int dx = Mathf.Abs(lastPos.x - x);
            int dy = Mathf.Abs(lastPos.y - y);
            if (dx + dy != 1) return; 
        }
        // ----------------------------------

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

    private void AddStoneToPath(int x, int y)
    {
        if (board == null) return;

        var stone = board.GetStone(x, y);
        if (!stone) return; 
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
