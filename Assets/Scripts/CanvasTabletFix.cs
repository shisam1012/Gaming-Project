using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CanvasTabletFix : MonoBehaviour
{
    [Header("Tablet Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float tabletThresholdRatio = 1.5f; // Width/Height ratio to detect tablets
    
    private Canvas canvas;
    private CanvasScaler canvasScaler;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixCanvasForTablet();
        }
    }
    
    [ContextMenu("Fix Canvas For Tablet")]
    public void FixCanvasForTablet()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        
        if (canvas == null)
        {
            Debug.LogError("[CanvasTabletFix] No Canvas component found!");
            return;
        }
        
        float screenRatio = (float)Screen.width / Screen.height;
        bool isTablet = screenRatio > tabletThresholdRatio || Screen.dpi < 200; // Tablets usually have lower DPI
        
        Debug.Log($"[CanvasTabletFix] Screen: {Screen.width}x{Screen.height}, Ratio: {screenRatio:F2}, DPI: {Screen.dpi}, Is Tablet: {isTablet}");
        
        if (canvasScaler == null)
        {
            Debug.LogWarning("[CanvasTabletFix] No CanvasScaler found, adding one...");
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
        }
        
        if (isTablet)
        {
            // Tablet-specific settings
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080); // Common tablet resolution
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            
            Debug.Log("[CanvasTabletFix] Applied tablet-specific Canvas scaling");
        }
        else
        {
            // Phone/desktop settings
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // Common phone resolution
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0f; // Match width for phones
            
            Debug.Log("[CanvasTabletFix] Applied phone/desktop Canvas scaling");
        }
        
        // Ensure canvas renders properly
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        
        // Force refresh
        Canvas.ForceUpdateCanvases();
    }
    
    [ContextMenu("Debug Canvas Info")]
    public void DebugCanvasInfo()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        
        Debug.Log($"[CanvasTabletFix] === Canvas Debug Info ===");
        Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
        Debug.Log($"Screen DPI: {Screen.dpi}");
        Debug.Log($"Canvas Render Mode: {canvas?.renderMode}");
        Debug.Log($"Canvas Pixel Perfect: {canvas?.pixelPerfect}");
        
        if (canvasScaler != null)
        {
            Debug.Log($"UI Scale Mode: {canvasScaler.uiScaleMode}");
            Debug.Log($"Reference Resolution: {canvasScaler.referenceResolution}");
            Debug.Log($"Screen Match Mode: {canvasScaler.screenMatchMode}");
            Debug.Log($"Match Width Or Height: {canvasScaler.matchWidthOrHeight}");
            Debug.Log($"Scale Factor: {canvasScaler.scaleFactor}");
        }
        else
        {
            Debug.LogWarning("No CanvasScaler found!");
        }
    }
}