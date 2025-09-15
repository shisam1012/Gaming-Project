using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Centralized input controller that handles all player input (touch/mouse)
/// and translates gestures into actions for other systems to consume.
/// </summary>
public class TouchInputController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private LayerMask interactableLayerMask = -1;
    [SerializeField] private float dragThreshold = 10f;
    
    // Events that other systems can subscribe to
    public static event Action<Vector2> OnTouchStart;
    public static event Action<Vector2> OnTouchEnd;
    public static event Action<Vector2, Vector2> OnDrag;
    public static event Action<GameObject> OnObjectTouched;
    public static event Action<GameObject> OnObjectReleased;
    
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private GameObject touchedObject;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
#if ENABLE_INPUT_SYSTEM
        HandleNewInputSystem();
#else
        HandleLegacyInput();
#endif
    }
    
#if ENABLE_INPUT_SYSTEM
    private void HandleNewInputSystem()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;
        
        bool isPressed = pointer.press.isPressed;
        bool wasPressed = pointer.press.wasPressedThisFrame;
        bool wasReleased = pointer.press.wasReleasedThisFrame;
        
        Vector2 screenPosition = pointer.position.ReadValue();
        
        if (wasPressed)
        {
            Debug.Log($"[TouchInputController] New Input - Touch start at screen: {screenPosition}");
            HandleTouchStart(screenPosition);
        }
        else if (isPressed)
        {
            // Call HandleTouchDrag for any pressed state, let it determine if dragging should start
            Debug.Log($"[TouchInputController] New Input - Touch drag at screen: {screenPosition}");
            HandleTouchDrag(screenPosition);
        }
        else if (wasReleased)
        {
            Debug.Log($"[TouchInputController] New Input - Touch end at screen: {screenPosition}");
            HandleTouchEnd(screenPosition);
        }
    }
#else
    private void HandleLegacyInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[TouchInputController] Legacy Input - Mouse down at screen: {Input.mousePosition}");
            HandleTouchStart(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            // Call HandleTouchDrag for any held state, let it determine if dragging should start
            HandleTouchDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log($"[TouchInputController] Legacy Input - Mouse up at screen: {Input.mousePosition}");
            HandleTouchEnd(Input.mousePosition);
        }
    }
#endif
    
    private void HandleTouchStart(Vector2 screenPosition)
    {
        Debug.Log($"[TouchInputController] HandleTouchStart called at: {screenPosition}");
        
        // TEMPORARY: Disable UI check to test if this is the issue
        /*
        // Check if touching UI - but be more specific about what counts as UI
        bool isOverUI = false;
        if (EventSystem.current != null)
        {
#if ENABLE_INPUT_SYSTEM
            // For new input system, check if over UI more carefully
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            // Only block if we hit actual UI elements (not game objects)
            foreach (var result in results)
            {
                if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    isOverUI = true;
                    Debug.Log($"[TouchInputController] Touch blocked by UI element: {result.gameObject.name}");
                    break;
                }
            }
#else
            // For legacy input, use the simpler check
            isOverUI = EventSystem.current.IsPointerOverGameObject();
#endif
        }
        
        if (isOverUI)
        {
            Debug.Log("[TouchInputController] Touch ignored - pointer over actual UI");
            return;
        }
        */
            
        startTouchPosition = screenPosition;
        currentTouchPosition = screenPosition;
        isDragging = false;
        
        // Raycast to find touched object
        touchedObject = GetTouchedObject(screenPosition);
        Debug.Log($"[TouchInputController] GetTouchedObject returned: {(touchedObject ? touchedObject.name : "null")}");
        
        // Fire events
        Debug.Log($"[TouchInputController] Firing OnTouchStart event");
        OnTouchStart?.Invoke(screenPosition);
        if (touchedObject != null)
        {
            Debug.Log($"[TouchInputController] Firing OnObjectTouched event for: {touchedObject.name}");
            Debug.Log($"[TouchInputController] OnObjectTouched subscribers: {(OnObjectTouched?.GetInvocationList()?.Length ?? 0)}");
            OnObjectTouched?.Invoke(touchedObject);
        }
        else
        {
            Debug.Log("[TouchInputController] No object touched - OnObjectTouched not fired");
        }
    }
    
    private void HandleTouchDrag(Vector2 screenPosition)
    {
        currentTouchPosition = screenPosition;
        
        // Check if we've moved enough to start dragging
        if (!isDragging)
        {
            float distance = Vector2.Distance(startTouchPosition, currentTouchPosition);
            Debug.Log($"[TouchInputController] HandleTouchDrag - Distance: {distance}, Threshold: {dragThreshold}");
            
            if (distance > dragThreshold)
            {
                isDragging = true;
                Debug.Log("[TouchInputController] Started dragging - threshold exceeded");
            }
        }
        
        if (isDragging)
        {
            Debug.Log($"[TouchInputController] HandleTouchDrag - Firing OnDrag event - Start: {startTouchPosition}, Current: {currentTouchPosition}");
            OnDrag?.Invoke(startTouchPosition, currentTouchPosition);
            
            // Also check for objects under the current drag position
            GameObject draggedOverObject = GetTouchedObject(screenPosition);
            if (draggedOverObject != null && draggedOverObject != touchedObject)
            {
                Debug.Log($"[TouchInputController] Dragged over new object: {draggedOverObject.name}");
                OnObjectTouched?.Invoke(draggedOverObject);
                // Don't update touchedObject here as it should remain the originally touched object
            }
        }
    }
    
    private void HandleTouchEnd(Vector2 screenPosition)
    {
        OnTouchEnd?.Invoke(screenPosition);
        
        if (touchedObject != null)
        {
            OnObjectReleased?.Invoke(touchedObject);
        }
        
        // Reset state
        isDragging = false;
        touchedObject = null;
    }
    
    private GameObject GetTouchedObject(Vector2 screenPosition)
    {
        if (mainCamera == null) 
        {
            Debug.LogError("[TouchInputController] Main camera is null!");
            return null;
        }
        
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f; // For 2D games
        
        Debug.Log($"[TouchInputController] Screen {screenPosition} -> World {worldPosition}");
        Debug.Log($"[TouchInputController] Interactable Layer Mask: {interactableLayerMask.value}");
        
        // Use layer mask to filter interactions
        Collider2D hit = Physics2D.OverlapPoint(worldPosition, interactableLayerMask);
        
        if (hit != null)
        {
            Debug.Log($"[TouchInputController] Hit object: {hit.gameObject.name} on layer {hit.gameObject.layer}");
        }
        else
        {
            Debug.Log($"[TouchInputController] No object hit at world position {worldPosition}");
            
            // Debug: Check if there are ANY colliders at this position (without layer mask)
            Collider2D anyHit = Physics2D.OverlapPoint(worldPosition);
            if (anyHit != null)
            {
                Debug.Log($"[TouchInputController] Found object {anyHit.gameObject.name} on layer {anyHit.gameObject.layer}, but layer mask filtered it out");
            }
            else
            {
                Debug.Log($"[TouchInputController] No colliders at all at this position");
            }
        }
        
        return hit != null ? hit.gameObject : null;
    }
    
    public Vector2 GetWorldPosition(Vector2 screenPosition)
    {
        if (mainCamera == null) return Vector2.zero;
        
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        return new Vector2(worldPos.x, worldPos.y);
    }
    
    // Utility methods for other systems
    public bool IsDragging => isDragging;
    public Vector2 StartTouchPosition => startTouchPosition;
    public Vector2 CurrentTouchPosition => currentTouchPosition;
    public GameObject TouchedObject => touchedObject;
}