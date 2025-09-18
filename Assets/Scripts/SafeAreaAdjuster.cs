using UnityEngine;
using UnityEngine.UI;

namespace GamingProject
{
    /// <summary>
    /// Simple script to add safe area padding to the main game canvas.
    /// This ensures UI elements don't get too close to screen edges on tall phones.
    /// </summary>
    public class SafeAreaAdjuster : MonoBehaviour
    {
        [Header("Safe Area Settings")]
        [SerializeField] private bool adjustOnStart = true;
        [SerializeField] private float topPadding = 80f; // Pixels to add at top
        [SerializeField] private float bottomPadding = 40f; // Pixels to add at bottom
        [SerializeField] private float sidePadding = 20f; // Pixels to add on sides
        
        [Header("Tall Screen Detection")]
        [SerializeField] private bool onlyAdjustTallScreens = true;
        [SerializeField] private float tallScreenRatio = 1.8f; // Height/Width ratio
        
        private RectTransform rectTransform;
        private Vector2 originalOffsetMin;
        private Vector2 originalOffsetMax;
        
        void Start()
        {
            if (adjustOnStart)
            {
                AdjustSafeArea();
            }
        }
        
        [ContextMenu("Adjust Safe Area")]
        public void AdjustSafeArea()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("[SafeAreaAdjuster] No RectTransform found on this GameObject!");
                return;
            }
            
            // Store original values
            if (originalOffsetMin == Vector2.zero && originalOffsetMax == Vector2.zero)
            {
                originalOffsetMin = rectTransform.offsetMin;
                originalOffsetMax = rectTransform.offsetMax;
            }
            
            // Check if we should adjust for this screen
            float screenRatio = (float)Screen.height / Screen.width;
            bool isTallScreen = screenRatio > tallScreenRatio;
            
            Debug.Log($"[SafeAreaAdjuster] Screen: {Screen.width}x{Screen.height}, Ratio: {screenRatio:F2}, Tall: {isTallScreen}");
            
            if (onlyAdjustTallScreens && !isTallScreen)
            {
                Debug.Log("[SafeAreaAdjuster] Not a tall screen, skipping adjustment");
                return;
            }
            
            // Apply safe area adjustments
            Vector2 newOffsetMin = originalOffsetMin;
            Vector2 newOffsetMax = originalOffsetMax;
            
            // Add padding (offsetMin affects left and bottom, offsetMax affects right and top)
            newOffsetMin.x += sidePadding; // Left padding
            newOffsetMin.y += bottomPadding; // Bottom padding
            newOffsetMax.x -= sidePadding; // Right padding (negative because it's from the right edge)
            newOffsetMax.y -= topPadding; // Top padding (negative because it's from the top edge)
            
            rectTransform.offsetMin = newOffsetMin;
            rectTransform.offsetMax = newOffsetMax;
            
            Debug.Log($"[SafeAreaAdjuster] Applied safe area padding - Top: {topPadding}, Bottom: {bottomPadding}, Sides: {sidePadding}");
        }
        
        [ContextMenu("Reset Safe Area")]
        public void ResetSafeArea()
        {
            if (rectTransform != null)
            {
                rectTransform.offsetMin = originalOffsetMin;
                rectTransform.offsetMax = originalOffsetMax;
                Debug.Log("[SafeAreaAdjuster] Reset to original safe area");
            }
        }
        
        [ContextMenu("Increase Top Padding")]
        public void IncreaseTopPadding()
        {
            topPadding += 20f;
            AdjustSafeArea();
            Debug.Log($"[SafeAreaAdjuster] Increased top padding to {topPadding}");
        }
        
        [ContextMenu("Decrease Top Padding")]
        public void DecreaseTopPadding()
        {
            topPadding = Mathf.Max(0f, topPadding - 20f);
            AdjustSafeArea();
            Debug.Log($"[SafeAreaAdjuster] Decreased top padding to {topPadding}");
        }
    }
}