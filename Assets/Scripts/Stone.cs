using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    private Board board;

    public int column;
    public int row;

    private bool isDragging = false;
    private List<Vector2Int> draggedStones = new List<Vector2Int>();
    private List<Stone> draggedStoneObjects = new List<Stone>();

    [HideInInspector]
    public Color originalColor;
    private SpriteRenderer spriteRenderer;

    private const int MinToDragOver = 3;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        board = FindObjectOfType<Board>();
        column = Mathf.RoundToInt(transform.position.x);
        row = Mathf.RoundToInt(transform.position.y);
    }

    private void OnMouseDown()
    {
        isDragging = true;
        ResetDraggedHighlights();
        draggedStones.Clear();
        draggedStoneObjects.Clear();
        AddStoneToPath(column, row);
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x < 0 || x >= board.width || y < 0 || y >= board.height)
            return;

        Vector2Int newPos = new Vector2Int(x, y);

        // backtrack
        if (draggedStones.Count > 1 && newPos == draggedStones[draggedStones.Count - 2])
        {
            Stone lastStone = draggedStoneObjects[draggedStoneObjects.Count - 1];
            lastStone.ResetHighlight();
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

            if (dx > 1 || dy > 1) return;

            Stone lastStone = board.allStones[lastPos.x, lastPos.y].GetComponent<Stone>();
            Stone currentStone = board.allStones[x, y].GetComponent<Stone>();
            if (lastStone.originalColor != currentStone.originalColor) return;
        }

        AddStoneToPath(x, y);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        Debug.Log("Path length: " + draggedStones.Count);
        if (draggedStones.Count >= MinToDragOver)
        {
            Debug.Log("calling remove stones.....");
            board.RemoveStones(draggedStones);
        }

        ResetDraggedHighlights();
        draggedStones.Clear();
        draggedStoneObjects.Clear();
    }

    private void AddStoneToPath(int x, int y)
    {
        draggedStones.Add(new Vector2Int(x, y));
        Stone stone = board.allStones[x, y].GetComponent<Stone>();
        if (stone != null)
        {
            stone.Highlight();
            draggedStoneObjects.Add(stone);
        }
    }

    private void ResetDraggedHighlights()
    {
        foreach (var s in draggedStoneObjects)
            s.ResetHighlight();
    }

    public void Highlight()
    {
        spriteRenderer.color = originalColor * 1.5f;
    }

    public void ResetHighlight()
    {
        spriteRenderer.color = originalColor;
    }
}
