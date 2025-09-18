using UnityEngine;

namespace GamingProject
{
    /// <summary>
    /// Adjusts the board position to prevent the top row from being too close to the screen edge.
    /// This is especially useful for tall screens (phones in portrait mode).
    /// </summary>
    public class BoardPositionAdjuster : MonoBehaviour
    {
        [Header("Position Adjustment")]
        [SerializeField] private float topMarginPercent = 15f; // Percentage of screen height to use as top margin
        [SerializeField] private bool adjustOnStart = true;
        [SerializeField] private bool adjustForTallScreens = true;
        [SerializeField] private float tallScreenThreshold = 1.8f; // Height/Width ratio threshold
        
        [Header("Manual Adjustment")]
        [SerializeField] private Vector3 manualOffset = Vector3.zero;
        
        private Transform boardTransform;
        private Vector3 originalPosition;
        
        void Start()
        {
            if (adjustOnStart)
            {
                AdjustBoardPosition();
            }
        }
        
        [ContextMenu("Adjust Board Position")]
        public void AdjustBoardPosition()
        {
            // Find the board (Grid or Board component)
            if (boardTransform == null)
            {
                // Try to find Grid first
                Grid grid = FindFirstObjectByType<Grid>();
                if (grid != null)
                {
                    boardTransform = grid.transform;
                }
                else
                {
                    // Try to find Board component
                    Board board = FindFirstObjectByType<Board>();
                    if (board != null)
                    {
                        boardTransform = board.transform;
                    }
                }
                
                if (boardTransform == null)
                {
                    Debug.LogError("[BoardPositionAdjuster] Could not find Grid or Board to adjust!");
                    return;
                }
                
                // Store original position
                originalPosition = boardTransform.position;
            }
            
            // Calculate screen aspect ratio
            float screenRatio = (float)Screen.height / Screen.width;
            bool isTallScreen = screenRatio > tallScreenThreshold;
            
            Debug.Log($"[BoardPositionAdjuster] Screen: {Screen.width}x{Screen.height}, Ratio: {screenRatio:F2}, Tall Screen: {isTallScreen}");
            
            if (adjustForTallScreens && !isTallScreen)
            {
                Debug.Log("[BoardPositionAdjuster] Not a tall screen, skipping adjustment");
                return;
            }
            
            // Calculate the offset needed
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[BoardPositionAdjuster] No main camera found!");
                return;
            }
            
            // Get screen bounds in world space
            Vector3 topOfScreen = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height, mainCamera.nearClipPlane));
            Vector3 bottomOfScreen = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, mainCamera.nearClipPlane));
            
            float screenHeight = topOfScreen.y - bottomOfScreen.y;
            float marginOffset = screenHeight * (topMarginPercent / 100f);
            
            // Apply the offset
            Vector3 newPosition = originalPosition;
            newPosition.y -= marginOffset; // Move down from top
            newPosition += manualOffset; // Add any manual adjustments
            
            boardTransform.position = newPosition;
            
            Debug.Log($"[BoardPositionAdjuster] Moved board from {originalPosition} to {newPosition} (offset: {marginOffset:F2})");
        }
        
        [ContextMenu("Reset Board Position")]
        public void ResetBoardPosition()
        {
            if (boardTransform != null)
            {
                boardTransform.position = originalPosition;
                Debug.Log($"[BoardPositionAdjuster] Reset board position to {originalPosition}");
            }
        }
        
        [ContextMenu("Increase Top Margin")]
        public void IncreaseTopMargin()
        {
            topMarginPercent += 5f;
            AdjustBoardPosition();
            Debug.Log($"[BoardPositionAdjuster] Increased top margin to {topMarginPercent}%");
        }
        
        [ContextMenu("Decrease Top Margin")]
        public void DecreaseTopMargin()
        {
            topMarginPercent = Mathf.Max(0f, topMarginPercent - 5f);
            AdjustBoardPosition();
            Debug.Log($"[BoardPositionAdjuster] Decreased top margin to {topMarginPercent}%");
        }
        
        // For different screen orientations
        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && adjustOnStart)
            {
                // Re-adjust when app regains focus (in case orientation changed)
                Invoke(nameof(AdjustBoardPosition), 0.1f);
            }
        }
    }
}