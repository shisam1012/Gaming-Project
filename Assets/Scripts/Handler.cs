using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handler manages detection and interaction with game objects.
/// Place this in the scene to handle all object interactions with Layer Mask filtering.
/// Replaces the old PointProbe functionality with clearer responsibilities.
/// </summary>
public class Handler : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask detectionLayerMask = -1;
    [SerializeField] private float detectionRadius = 0.5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightAlpha = 0.5f;
    
    private Camera mainCamera;
    private List<GameObject> currentlyHighlighted = new List<GameObject>();
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();
    }
    
    private void Start()
    {
        Debug.Log("[Handler] Starting up and subscribing to TouchInputController events");
        
        // Subscribe to input events
        TouchInputController.OnObjectTouched += OnObjectTouched;
        TouchInputController.OnObjectReleased += OnObjectReleased;
        TouchInputController.OnDrag += OnDrag;
        
        Debug.Log("[Handler] Successfully subscribed to events");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        TouchInputController.OnObjectTouched -= OnObjectTouched;
        TouchInputController.OnObjectReleased -= OnObjectReleased;
        TouchInputController.OnDrag -= OnDrag;
    }
    
    private void OnObjectTouched(GameObject touchedObject)
    {
        Debug.Log($"[Handler] OnObjectTouched called with: {(touchedObject ? touchedObject.name : "null")}");
        
        if (touchedObject == null) return;
        
        // Check if object is on the correct layer
        if (!IsOnDetectionLayer(touchedObject))
        {
            Debug.Log($"[Handler] Object {touchedObject.name} is not on detection layer (layer: {touchedObject.layer})");
            return;
        }
        
        Debug.Log($"[Handler] Object {touchedObject.name} passed layer check");
        
        // Notify the interaction controller
        var interactionController = FindFirstObjectByType<InteractionController>();
        if (interactionController != null)
        {
            Debug.Log($"[Handler] Notifying InteractionController about {touchedObject.name}");
            interactionController.OnObjectSelected(touchedObject);
        }
        else
        {
            Debug.LogError("[Handler] InteractionController not found!");
        }
    }
    
    private void OnObjectReleased(GameObject releasedObject)
    {
        if (releasedObject == null) return;
        
        // Remove highlight
        UnhighlightObject(releasedObject);
        
        // Notify the interaction controller
        var interactionController = FindFirstObjectByType<InteractionController>();
        if (interactionController != null)
        {
            interactionController.OnObjectReleased(releasedObject);
        }
    }
    
    private void OnDrag(Vector2 startPos, Vector2 currentPos)
    {
        Debug.Log($"[Handler] OnDrag called - Start: {startPos}, Current: {currentPos}");
        
        // Handle drag interactions
        var interactionController = FindFirstObjectByType<InteractionController>();
        if (interactionController != null)
        {
            Debug.Log("[Handler] Forwarding drag to InteractionController");
            interactionController.OnDrag(startPos, currentPos);
        }
        else
        {
            Debug.LogError("[Handler] InteractionController not found for drag!");
        }
    }
    
    public List<GameObject> DetectObjectsAtPosition(Vector2 worldPosition)
    {
        List<GameObject> detectedObjects = new List<GameObject>();
        
        // Use OverlapCircleAll for more reliable detection
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, detectionRadius, detectionLayerMask);
        
        foreach (var collider in colliders)
        {
            if (collider != null && collider.gameObject != null)
            {
                detectedObjects.Add(collider.gameObject);
            }
        }
        
        return detectedObjects;
    }
    
    public GameObject DetectObjectAtPosition(Vector2 worldPosition)
    {
        Collider2D hit = Physics2D.OverlapCircle(worldPosition, detectionRadius, detectionLayerMask);
        return hit != null ? hit.gameObject : null;
    }
    
    private bool IsOnDetectionLayer(GameObject obj)
    {
        return ((1 << obj.layer) & detectionLayerMask) != 0;
    }
    
    private void HighlightObject(GameObject obj)
    {
        if (obj == null || currentlyHighlighted.Contains(obj))
            return;
            
        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Store original color if not already stored
            var highlightComponent = obj.GetComponent<HighlightHelper>();
            if (highlightComponent == null)
            {
                highlightComponent = obj.AddComponent<HighlightHelper>();
                highlightComponent.originalColor = renderer.color;
            }
            
            // Apply highlight
            Color highlightedColor = highlightColor;
            highlightedColor.a = highlightAlpha;
            renderer.color = Color.Lerp(highlightComponent.originalColor, highlightedColor, 0.5f);
            
            currentlyHighlighted.Add(obj);
        }
    }
    
    private void UnhighlightObject(GameObject obj)
    {
        if (obj == null)
            return;
            
        var renderer = obj.GetComponent<SpriteRenderer>();
        var highlightComponent = obj.GetComponent<HighlightHelper>();
        
        if (renderer != null && highlightComponent != null)
        {
            // Restore original color
            renderer.color = highlightComponent.originalColor;
            
            // Clean up
            DestroyImmediate(highlightComponent);
        }
        
        currentlyHighlighted.Remove(obj);
    }
    
    public void ClearAllHighlights()
    {
        var objectsToUnhighlight = new List<GameObject>(currentlyHighlighted);
        foreach (var obj in objectsToUnhighlight)
        {
            UnhighlightObject(obj);
        }
        currentlyHighlighted.Clear();
    }
    
    // Helper component to store original colors
    private class HighlightHelper : MonoBehaviour
    {
        public Color originalColor;
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}