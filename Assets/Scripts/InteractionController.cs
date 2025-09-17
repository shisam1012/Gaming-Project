using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GamingProject
{
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
        private PointerProbe _pointerHandler;

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
            _pointerHandler = FindFirstObjectByType<PointerProbe>();
            if (_pointerHandler == null)
                throw new UnityException("pointer probe is not found on InteractionController, please add it to the TimeCanvas or delete it forever");
            _pointerHandler.OnSelect += OnTouchSelected;
            _pointerHandler.OnMove += OnTouchMoved;
            TouchInputController.OnTouchStart += OnTouchStart;
            TouchInputController.OnTouchEnd += OnTouchEnd;
            TouchInputController.OnDrag += OnDrag;


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

            if (isSelecting)
            {
                Debug.Log("[InteractionController] Clearing existing selection on new touch");
                ClearSelection();
            }
        }

        private void OnTouchSelected(PointerEventData e)
        {
            Debug.Log("OnSelected is called");

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(e.position);
            worldPos.z = 0f; // match 2D plane

            RaycastHit2D hit = Physics2D.Raycast(worldPos, -Vector2.up);

            Debug.Log("hit +" + hit.collider.gameObject);
        }

        private void OnTouchMoved(PointerEventData e)
        {
            Debug.Log("On Move is called");

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(e.position);
            worldPos.z = 0f; // match 2D plane

            RaycastHit2D hit = Physics2D.Raycast(worldPos, -Vector2.up);

            Debug.Log("MOVE hit +" + hit.collider.gameObject);
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
                // Clear any previous highlighting from all stones before starting new selection
                ClearAllStoneHighlights();
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

            var stoneComponent = stone.GetComponent<Stone>();
            if (stoneComponent == null)
            {
                Debug.Log($"[InteractionController] Stone {stone.name} has no Stone component, skipping");
                return;
            }

            // Check if this stone is already in the selection (for drag-back functionality)
            int existingIndex = selectedStones.IndexOf(stone);
            if (existingIndex != -1)
            {
                // If it's the last stone in the selection, ignore (no change)
                if (existingIndex == selectedStones.Count - 1)
                {
                    Debug.Log($"[InteractionController] Stone {stone.name} is already the last selected stone, ignoring");
                    return;
                }

                // If it's not the last stone, we're going backwards - remove all stones after this one
                Debug.Log($"[InteractionController] Going backwards - removing stones after index {existingIndex}");

                // Reset highlighting on stones that will be removed
                for (int i = selectedStones.Count - 1; i > existingIndex; i--)
                {
                    if (selectedStones[i] != null)
                    {
                        var stoneToRemove = selectedStones[i].GetComponent<Stone>();
                        if (stoneToRemove != null)
                        {
                            stoneToRemove.ResetHighlight();
                        }
                    }
                    Debug.Log($"[InteractionController] Removing stone at index {i}: {selectedStones[i].name}");
                    selectedStones.RemoveAt(i);
                    selectedPositions.RemoveAt(i);
                }

                // Update the last selected stone
                lastSelectedStone = selectedStones.Count > 0 ? selectedStones[selectedStones.Count - 1] : null;
                UpdateSelectionVisual();
                return;
            }

            // Stone is not in selection, try to add it
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
            // Reset highlighting on all previously selected stones
            for (int i = 0; i < selectedStones.Count; i++)
            {
                if (selectedStones[i] != null)
                {
                    var stone = selectedStones[i].GetComponent<Stone>();
                    if (stone != null)
                    {
                        stone.ResetHighlight();
                    }
                }
            }

            selectedStones.Clear();
            selectedPositions.Clear();
            lastSelectedStone = null;
            isSelecting = false;

            // Ensure ALL stones are unhighlighted, not just the selected ones
            ClearAllStoneHighlights();

            UpdateSelectionVisual();
        }

        private void ClearAllStoneHighlights()
        {
            // Find all Stone components in the scene and reset their highlights
            Stone[] allStones = FindObjectsByType<Stone>(FindObjectsSortMode.None);
            foreach (var stone in allStones)
            {
                if (stone != null)
                {
                    stone.ResetHighlight();
                }
            }
        }

        private void UpdateSelectionVisual()
        {
            // Disable the line renderer - we'll use stone highlighting instead
            if (selectionLineRenderer != null)
            {
                selectionLineRenderer.positionCount = 0;
            }

            // Highlight all selected stones
            for (int i = 0; i < selectedStones.Count; i++)
            {
                if (selectedStones[i] != null)
                {
                    var stone = selectedStones[i].GetComponent<Stone>();
                    if (stone != null)
                    {
                        stone.Highlight();
                    }
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

            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 worldPos = cam.ScreenToWorldPoint(screenPosition);
                return new Vector2(worldPos.x, worldPos.y);
            }

            return Vector2.zero;
        }

        public bool IsSelecting => isSelecting;
        public int SelectedCount => selectedStones.Count;
        public List<GameObject> SelectedStones => new List<GameObject>(selectedStones);
        public List<Vector2Int> SelectedPositions => new List<Vector2Int>(selectedPositions);
    }
}