using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreUIManager : MonoBehaviour
{
    [Header("Auto-create UI if missing")]
    public bool autoCreateUI = true;
    
    public static ScoreUIManager instance; // Singleton instance
    private Canvas gameCanvas;
    private GameObject scoreHandlerObj;
    private ScoreHandler scoreHandler;
    
    void Awake()
    {
        // Singleton pattern - only allow one ScoreUIManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ScoreUIManager] ScoreUIManager instance created and marked as persistent");
        }
        else
        {
            Debug.Log("[ScoreUIManager] Duplicate ScoreUIManager found, destroying...");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Don't auto-create UI on Start - wait for game to actually begin
        Debug.Log("[ScoreUIManager] ScoreUIManager ready, waiting for game start to create UI");
    }
    
    // Public method to trigger UI creation when game starts
    public void CreateUIOnGameStart()
    {
        // ALWAYS check if UI actually exists in the scene, regardless of flags
        GameObject existingContainer = GameObject.Find("Score Container");
        
        if (existingContainer == null && autoCreateUI)
        {
            // No UI exists, create it
            Debug.Log("[ScoreUIManager] No Score Container found, creating UI");
            EnsureScoreUIExists();
        }
        else if (existingContainer != null)
        {
            Debug.Log("[ScoreUIManager] Score Container exists, ensuring ScoreHandler is connected");
            // UI exists, make sure ScoreHandler is connected
            GameObject scoreTextObj = GameObject.Find("Score Text");
            if (scoreTextObj != null && ScoreHandler.instance != null)
            {
                var scoreTextComponent = scoreTextObj.GetComponent<TMP_Text>();
                if (scoreTextComponent != null)
                {
                    ScoreHandler.instance.SetScoreText(scoreTextComponent);
                    Debug.Log("[ScoreUIManager] Reconnected ScoreHandler to existing UI");
                }
            }
        }
        else
        {
            Debug.LogWarning("[ScoreUIManager] autoCreateUI is false, no UI will be created");
        }
    }
    
    private void ReconnectExistingUI()
    {
        // Find existing score text in the scene and reconnect it to ScoreHandler
        if (ScoreHandler.instance != null)
        {
            GameObject scoreTextObj = GameObject.Find("Score Text");
            if (scoreTextObj != null)
            {
                var scoreText = scoreTextObj.GetComponent<TMP_Text>();
                if (scoreText != null)
                {
                    ScoreHandler.instance.ReconnectUI(scoreText);
                    Debug.Log("[ScoreUIManager] Reconnected existing UI to persistent ScoreHandler");
                    return;
                }
            }
            
            // If we couldn't find the score text, the UI might be broken - recreate it
            Debug.LogWarning("[ScoreUIManager] Could not find Score Text, recreating UI...");
            EnsureScoreUIExists();
        }
        else
        {
            Debug.LogError("[ScoreUIManager] No ScoreHandler instance found for reconnection");
        }
    }
    
    private void EnsureScoreUIExists()
    {
        // Check if ScoreHandler GameObject exists
        scoreHandlerObj = GameObject.Find("ScoreHandler");
        
        if (scoreHandlerObj == null)
        {
            Debug.Log("[ScoreUIManager] ScoreHandler not found, creating complete UI system...");
            CreateScoreUI();
        }
        else
        {
            Debug.Log("[ScoreUIManager] ScoreHandler found, using existing persistent ScoreHandler");
            scoreHandler = scoreHandlerObj.GetComponent<ScoreHandler>();
            
            // Use existing persistent ScoreHandler if available
            if (ScoreHandler.instance != null)
            {
                scoreHandler = ScoreHandler.instance;
                scoreHandlerObj = scoreHandler.gameObject;
                Debug.Log("[ScoreUIManager] Using existing persistent ScoreHandler");
            }
            
            // Create fresh UI and connect it to the persistent ScoreHandler
            CreateScoreTextOnly();
        }
        
        Debug.Log("[ScoreUIManager] Score UI setup complete");
    }
    
    private void CreateScoreUI()
    {
        // Find or create Canvas
        gameCanvas = FindFirstObjectByType<Canvas>();
        if (gameCanvas == null)
        {
            GameObject canvasObj = new GameObject("Game Canvas");
            gameCanvas = canvasObj.AddComponent<Canvas>();
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[ScoreUIManager] Created Game Canvas");
        }
        
        // Check if ScoreHandler instance already exists (persistent)
        if (ScoreHandler.instance != null)
        {
            scoreHandler = ScoreHandler.instance;
            scoreHandlerObj = scoreHandler.gameObject;
            Debug.Log("[ScoreUIManager] Using existing persistent ScoreHandler");
        }
        else
        {
            // Create new ScoreHandler GameObject
            scoreHandlerObj = new GameObject("ScoreHandler");
            scoreHandler = scoreHandlerObj.AddComponent<ScoreHandler>();
            Debug.Log("[ScoreUIManager] Created new ScoreHandler");
        }
        
        // Create Score Text UI
        CreateScoreTextUI();
        
        Debug.Log("[ScoreUIManager] Created complete ScoreHandler UI system");
    }
    
    private void CreateScoreTextOnly()
    {
        // Find canvas for existing ScoreHandler
        gameCanvas = FindFirstObjectByType<Canvas>();
        if (gameCanvas == null)
        {
            Debug.LogError("[ScoreUIManager] No Canvas found to attach score text!");
            return;
        }
        
        // Use existing persistent ScoreHandler if available
        if (ScoreHandler.instance != null)
        {
            scoreHandler = ScoreHandler.instance;
            scoreHandlerObj = scoreHandler.gameObject;
            Debug.Log("[ScoreUIManager] Using existing persistent ScoreHandler");
        }
        
        CreateScoreTextUI();
    }
    
    private void CreateScoreTextUI()
    {
        // Create container for score and level info - positioned above timer area
        GameObject scoreContainer = new GameObject("Score Container");
        scoreContainer.transform.SetParent(gameCanvas.transform, false);
        
        // Add a subtle background panel
        Image backgroundPanel = scoreContainer.AddComponent<Image>();
        backgroundPanel.color = new Color(0, 0, 0, 0.8f); // More opaque background
        
        // Container positioning (top area, above everything else)
        RectTransform containerRect = scoreContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f);  // Top center anchor
        containerRect.anchorMax = new Vector2(0.5f, 1f);  // Top center anchor
        containerRect.anchoredPosition = new Vector2(0, -120);  // 120 pixels down from top
        containerRect.sizeDelta = new Vector2(800, 200);  // Even bigger: 800x200
        
        // Set sorting order to be on top
        Canvas scoreCanvas = scoreContainer.AddComponent<Canvas>();
        scoreCanvas.overrideSorting = true;
        scoreCanvas.sortingOrder = 1000;  // High sort order to appear on top
        
        // Create Score Text GameObject
        GameObject scoreTextObj = new GameObject("Score Text");
        scoreTextObj.transform.SetParent(scoreContainer.transform, false);
        
        // Add TextMeshPro component for score - just the number
        TMP_Text scoreText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "0";  // Just show the number
        scoreText.fontSize = 48;  // Big readable size
        scoreText.color = Color.yellow;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontStyle = FontStyles.Bold;
        
        // Position score text at top of container
        RectTransform scoreRect = scoreTextObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);  // Left half, top half
        scoreRect.offsetMin = Vector2.zero;
        scoreRect.offsetMax = Vector2.zero;
        
        // Create Level Text GameObject  
        GameObject levelTextObj = new GameObject("Level Text");
        levelTextObj.transform.SetParent(scoreContainer.transform, false);
        
        // Add TextMeshPro component for level - just the number
        TMP_Text levelText = levelTextObj.AddComponent<TextMeshProUGUI>();
        levelText.text = "1";  // Just show the number
        levelText.fontSize = 48;  // Big readable size
        levelText.color = Color.cyan;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.fontStyle = FontStyles.Bold;
        
        // Position level text next to score
        RectTransform levelRect = levelTextObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.5f, 0.5f);
        levelRect.anchorMax = new Vector2(1f, 1f);  // Right half, top half
        levelRect.offsetMin = Vector2.zero;
        levelRect.offsetMax = Vector2.zero;
        
        // Create labels for the numbers - these will show "Score" and "Level"
        GameObject scoreLabel = new GameObject("Score Label");
        scoreLabel.transform.SetParent(scoreContainer.transform, false);
        
        TMP_Text scoreLabelText = scoreLabel.AddComponent<TextMeshProUGUI>();
        scoreLabelText.text = "Score";
        scoreLabelText.fontSize = 32;  // Bigger labels: 32 instead of 18
        scoreLabelText.color = Color.white;
        scoreLabelText.alignment = TextAlignmentOptions.Center;
        
        RectTransform scoreLabelRect = scoreLabel.GetComponent<RectTransform>();
        scoreLabelRect.anchorMin = new Vector2(0, 0f);
        scoreLabelRect.anchorMax = new Vector2(0.5f, 0.5f);  // Left half, bottom half
        scoreLabelRect.offsetMin = Vector2.zero;
        scoreLabelRect.offsetMax = Vector2.zero;
        
        GameObject levelLabel = new GameObject("Level Label");
        levelLabel.transform.SetParent(scoreContainer.transform, false);
        
        TMP_Text levelLabelText = levelLabel.AddComponent<TextMeshProUGUI>();
        levelLabelText.text = "Level";
        levelLabelText.fontSize = 32;  // Bigger labels: 32 instead of 18
        levelLabelText.color = Color.white;
        levelLabelText.alignment = TextAlignmentOptions.Center;
        
        RectTransform levelLabelRect = levelLabel.GetComponent<RectTransform>();
        levelLabelRect.anchorMin = new Vector2(0.5f, 0f);
        levelLabelRect.anchorMax = new Vector2(1f, 0.5f);  // Right half, bottom half
        levelLabelRect.offsetMin = Vector2.zero;
        levelLabelRect.offsetMax = Vector2.zero;
        
        // DIRECT assignment - no reflection nonsense
        if (scoreHandler != null)
        {
            // Direct assignment to make sure it works
            scoreHandler.SetScoreText(scoreText);
            Debug.Log("[ScoreUIManager] Directly assigned score text to ScoreHandler");
        }
        
        // Create and assign level display manager
        GameObject levelManagerObj = new GameObject("LevelDisplayManager");
        LevelDisplayManager levelDisplayManager = levelManagerObj.AddComponent<LevelDisplayManager>();
        levelDisplayManager.SetLevelText(levelText);
        
        Debug.Log("[ScoreUIManager] Created score UI at top of screen with high sorting order");
    }
}