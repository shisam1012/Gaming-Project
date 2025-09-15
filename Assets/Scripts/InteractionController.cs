using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InteractionController handles all player → board interactions.
/// Coordinates between input, stones, and board logic.
/// This is where the game rules and interaction logic live.
/// </summary>
public class InteractionController : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private int minimumSelectionSize = 3;
    [SerializeField] private bool allowDiagonalSelection = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color selectionColor = Color.green;
    [SerializeField] private LineRenderer selectionLineRenderer;
    
    private List<GameObject> selectedStones = new List<GameObject>();
    private List<Vector2Int> selectedPositions = new List<Vector2Int>();
    private GameObject lastSelectedStone;
    private bool isSelecting = false;
    
    // Static directions for neighbor checking
    private static readonly Vector2Int[] orthogonalDirections = 
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };
    
    private static readonly Vector2Int[] allDirections = 
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };
    
    private void Start()
    {
        // Subscribe to input events
        TouchInputController.OnTouchStart += OnTouchStart;
        TouchInputController.OnTouchEnd += OnTouchEnd;
        TouchInputController.OnDrag += OnDrag;
        
        // Setup line renderer if not assigned
        if (selectionLineRenderer == null)
        {
            selectionLineRenderer = GetComponent<LineRenderer>();
            if (selectionLineRenderer == null)
            {
                var lineObj = new GameObject("SelectionLine");
                lineObj.transform.SetParent(transform);
                selectionLineRenderer = lineObj.AddComponent<LineRenderer>();
                SetupLineRenderer();
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        TouchInputController.OnTouchStart -= OnTouchStart;
        TouchInputController.OnTouchEnd -= OnTouchEnd;
        TouchInputController.OnDrag -= OnDrag;
    }
    
    private void SetupLineRenderer()
    {
        selectionLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        selectionLineRenderer.material.color = selectionColor;
        selectionLineRenderer.startWidth = 0.1f;
        selectionLineRenderer.endWidth = 0.1f;
        selectionLineRenderer.positionCount = 0;
        selectionLineRenderer.sortingOrder = 10;
        selectionLineRenderer.useWorldSpace = true;
    }
    
    private void OnTouchStart(Vector2 screenPosition)
    {
        Debug.Log($"[InteractionController] OnTouchStart called at: {screenPosition}");
        // Only clear any existing selection, don't start a new one
        // Let OnObjectSelected handle the actual selection start
        if (isSelecting)
        {
            Debug.Log("[InteractionController] Clearing existing selection on new touch");
            ClearSelection();
        }
    }
    
    private void OnTouchEnd(Vector2 screenPosition)
    {
        Debug.Log($"[InteractionController] OnTouchEnd called at: {screenPosition}");
        
        if (isSelecting)
        {
            Debug.Log($"[InteractionController] Touch ended while selecting {selectedStones.Count} stones");
            ProcessSelection();
        }
        
        ClearSelection();
    }
    
    public void OnDrag(Vector2 startPos, Vector2 currentPos)
    {
        Debug.Log($"[InteractionController] OnDrag called - Start: {startPos}, Current: {currentPos}, IsSelecting: {isSelecting}");
        
        if (!isSelecting) 
        {
            Debug.Log("[InteractionController] Not selecting, ignoring drag");
            return;
        }
        
        Vector2 worldPosition = GetWorldPosition(currentPos);
        GameObject hoveredStone = GetStoneAtPosition(worldPosition);
        
        Debug.Log($"[InteractionController] Drag world position: {worldPosition}, Hovered stone: {hoveredStone?.name}");
        
        if (hoveredStone != null && hoveredStone != lastSelectedStone)
        {
            Debug.Log($"[InteractionController] Trying to add stone {hoveredStone.name} to selection");
            TryAddStoneToSelection(hoveredStone);
        }
        else if (hoveredStone == null)
        {
            Debug.Log("[InteractionController] No stone at drag position");
        }
        else if (hoveredStone == lastSelectedStone)
        {
            Debug.Log($"[InteractionController] Stone {hoveredStone.name} already selected");
        }
    }
    
    public void OnObjectSelected(GameObject selectedObject)
    {
        Debug.Log($"[InteractionController] OnObjectSelected called with: {selectedObject?.name}");
        
        if (!isSelecting)
        {
            Debug.Log("[InteractionController] Starting new selection");
            StartSelection(selectedObject);
        }
        else
        {
            Debug.Log("[InteractionController] Adding to existing selection");
            TryAddStoneToSelection(selectedObject);
        }
    }
    
    public void OnObjectReleased(GameObject releasedObject)
    {
        Debug.Log($"[InteractionController] OnObjectReleased called with: {releasedObject?.name}");
        
        if (isSelecting)
        {
            Debug.Log("[InteractionController] Processing selection on release");
            ProcessSelection();
        }
        
        ClearSelection();
    }
    
    private void StartSelection(GameObject firstStone)
    {
        Debug.Log($"[InteractionController] StartSelection called with: {firstStone?.name}");
        
        if (firstStone == null) 
        {
            Debug.LogWarning("[InteractionController] StartSelection: firstStone is null");
            return;
        }
        
        var stoneComponent = firstStone.GetComponent<Stone>();
        if (stoneComponent == null) 
        {
            Debug.LogWarning($"[InteractionController] StartSelection: {firstStone.name} has no Stone component");
            return;
        }
        
        ClearSelection();
        
        isSelecting = true;
        selectedStones.Add(firstStone);
        selectedPositions.Add(new Vector2Int(stoneComponent.column, stoneComponent.row));
        lastSelectedStone = firstStone;
        
        Debug.Log($"[InteractionController] Started selection with stone at ({stoneComponent.column}, {stoneComponent.row})");
        
        UpdateSelectionVisual();
    }
    
    private void TryAddStoneToSelection(GameObject stone)
    {
        Debug.Log($"[InteractionController] TryAddStoneToSelection called with: {stone?.name}");
        
        if (stone == null)
        {
            Debug.Log("[InteractionController] Stone is null, skipping");
            return;
        }
        
        if (selectedStones.Contains(stone))
        {
            Debug.Log($"[InteractionController] Stone {stone.name} already in selection, skipping");
            return;
        }
        
        var stoneComponent = stone.GetComponent<Stone>();
        if (stoneComponent == null) 
        {
            Debug.Log($"[InteractionController] Stone {stone.name} has no Stone component, skipping");
            return;
        }
        
        // Check if this stone is adjacent to the last selected stone
        if (lastSelectedStone != null)
        {
            var lastStoneComponent = lastSelectedStone.GetComponent<Stone>();
            
            Debug.Log($"[InteractionController] Checking adjacency between {lastSelectedStone.name} ({lastStoneComponent.column}, {lastStoneComponent.row}) and {stone.name} ({stoneComponent.column}, {stoneComponent.row})");
            
            if (!AreAdjacent(lastStoneComponent, stoneComponent)) 
            {
                Debug.Log("[InteractionController] Stones are not adjacent, skipping");
                return;
            }
            
            // Check if stone type matches (for match-3 logic)
            if (stoneComponent.Type != lastStoneComponent.Type) 
            {
                Debug.Log($"[InteractionController] Stone types don't match: {stoneComponent.Type} != {lastStoneComponent.Type}, skipping");
                return;
            }
        }
        
        Debug.Log($"[InteractionController] Adding stone {stone.name} to selection");
        selectedStones.Add(stone);
        selectedPositions.Add(new Vector2Int(stoneComponent.column, stoneComponent.row));
        lastSelectedStone = stone;
        
        UpdateSelectionVisual();
    }
    
    private bool AreAdjacent(Stone stone1, Stone stone2)
    {
        Vector2Int pos1 = new Vector2Int(stone1.column, stone1.row);
        Vector2Int pos2 = new Vector2Int(stone2.column, stone2.row);
        
        Vector2Int diff = pos2 - pos1;
        
        Vector2Int[] directionsToCheck = allowDiagonalSelection ? allDirections : orthogonalDirections;
        
        foreach (var direction in directionsToCheck)
        {
            if (diff == direction) return true;
        }
        
        return false;
    }
    
    private void ProcessSelection()
    {
        Debug.Log($"[InteractionController] ProcessSelection called with {selectedStones.Count} stones");
        
        if (selectedStones.Count < minimumSelectionSize)
        {
            Debug.Log($"[InteractionController] Selection too small ({selectedStones.Count} < {minimumSelectionSize}), cancelling");
            return;
        }
        
        // Send the selection to the board for processing
        var gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.Board != null)
        {
            Debug.Log($"[InteractionController] Sending {selectedPositions.Count} positions to Board.RemoveStones");
            gameManager.Board.RemoveStones(selectedPositions);
        }
        else
        {
            Debug.LogError("[InteractionController] GameManager or Board is null!");
        }
    }
    
    private void ClearSelection()
    {
        selectedStones.Clear();
        selectedPositions.Clear();
        lastSelectedStone = null;
        isSelecting = false;
        
        UpdateSelectionVisual();
    }
    
    private void UpdateSelectionVisual()
    {
        if (selectionLineRenderer == null) return;
        
        selectionLineRenderer.positionCount = selectedStones.Count;
        
        for (int i = 0; i < selectedStones.Count; i++)
        {
            if (selectedStones[i] != null)
            {
                Vector3 pos = selectedStones[i].transform.position;
                pos.z = -1f; // Ensure line is visible
                selectionLineRenderer.SetPosition(i, pos);
            }
        }
    }
    
    private GameObject GetStoneAtPosition(Vector2 worldPosition)
    {
        var handler = FindFirstObjectByType<Handler>();
        if (handler != null)
        {
            return handler.DetectObjectAtPosition(worldPosition);
        }
        
        // Fallback to direct raycast
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);
        if (hit != null && hit.GetComponent<Stone>() != null)
        {
            return hit.gameObject;
        }
        
        return null;
    }
    
    private Vector2 GetWorldPosition(Vector2 screenPosition)
    {
        var touchController = FindFirstObjectByType<TouchInputController>();
        if (touchController != null)
        {
            return touchController.GetWorldPosition(screenPosition);
        }
        
        // Fallback
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPosition);
            return new Vector2(worldPos.x, worldPos.y);
        }
        
        return Vector2.zero;
    }
    
    // Public methods for external access
    public bool IsSelecting => isSelecting;
    public int SelectedCount => selectedStones.Count;
    public List<GameObject> SelectedStones => new List<GameObject>(selectedStones);
    public List<Vector2Int> SelectedPositions => new List<Vector2Int>(selectedPositions);
}