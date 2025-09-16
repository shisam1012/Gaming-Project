using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GamingProject
{
    public class TouchInputController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private LayerMask interactableLayerMask = -1;
    [SerializeField] private float dragThreshold = 10f;
    
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
        
        startTouchPosition = screenPosition;
        currentTouchPosition = screenPosition;
        isDragging = false;
        
        touchedObject = GetTouchedObject(screenPosition);
        Debug.Log($"[TouchInputController] GetTouchedObject returned: {(touchedObject ? touchedObject.name : "null")}");
        
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
            
            GameObject draggedOverObject = GetTouchedObject(screenPosition);
            if (draggedOverObject != null && draggedOverObject != touchedObject)
            {
                Debug.Log($"[TouchInputController] Dragged over new object: {draggedOverObject.name}");
                OnObjectTouched?.Invoke(draggedOverObject);
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
        worldPosition.z = 0f;
        
        Debug.Log($"[TouchInputController] Screen {screenPosition} -> World {worldPosition}");
        Debug.Log($"[TouchInputController] Interactable Layer Mask: {interactableLayerMask.value}");
        
        Collider2D hit = Physics2D.OverlapPoint(worldPosition, interactableLayerMask);
        
        if (hit != null)
        {
            Debug.Log($"[TouchInputController] Hit object: {hit.gameObject.name} on layer {hit.gameObject.layer}");
        }
        else
        {
            Debug.Log($"[TouchInputController] No object hit at world position {worldPosition}");
            
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
    
    public bool IsDragging => isDragging;
    public Vector2 StartTouchPosition => startTouchPosition;
    public Vector2 CurrentTouchPosition => currentTouchPosition;
    public GameObject TouchedObject => touchedObject;
    }
}