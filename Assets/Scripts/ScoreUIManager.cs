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
        // ALWAYS find existing UI elements instead of creating new ones
        Debug.Log("[ScoreUIManager] Looking for existing ScoreLevelCanvas and UI elements...");
        EnsureScoreUIExists();
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
        // Look for existing ScoreHandler or create one if needed
        scoreHandlerObj = GameObject.Find("ScoreHandler");
        
        if (scoreHandlerObj == null)
        {
            Debug.Log("[ScoreUIManager] ScoreHandler not found, creating one...");
            
            // Use existing persistent ScoreHandler if available
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
        }
        else
        {
            Debug.Log("[ScoreUIManager] Found existing ScoreHandler, using it");
            scoreHandler = scoreHandlerObj.GetComponent<ScoreHandler>();
        }
        
        // Find existing UI elements instead of creating new ones
        ConnectToExistingUI();
        
        Debug.Log("[ScoreUIManager] Connected to existing UI elements");
    }
    
    private void ConnectToExistingUI()
    {
        // Try to find existing ScoreLevelCanvas first
        GameObject scoreLevelCanvas = GameObject.Find("ScoreLevelCanvas");
        if (scoreLevelCanvas == null)
        {
            Debug.LogWarning("[ScoreUIManager] ScoreLevelCanvas not found! Trying to find any Canvas...");
            
            // Fallback: find any Canvas to attach our UI to
            Canvas anyCanvas = FindFirstObjectByType<Canvas>();
            if (anyCanvas != null)
            {
                Debug.Log("[ScoreUIManager] Found Canvas: " + anyCanvas.name + ", creating ScoreText and LevelText on it");
                CreateUIElementsOnCanvas(anyCanvas);
                return;
            }
            else
            {
                Debug.LogError("[ScoreUIManager] No Canvas found at all! Creating a new Canvas...");
                CreateNewCanvasWithUI();
                return;
            }
        }
        
        // Try to find existing ScoreText
        GameObject scoreTextObj = GameObject.Find("ScoreText");
        if (scoreTextObj == null)
        {
            Debug.LogWarning("[ScoreUIManager] ScoreText not found! Creating it on ScoreLevelCanvas...");
            Canvas canvas = scoreLevelCanvas.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ScoreUIManager] ScoreLevelCanvas does not have a Canvas component!");
                return;
            }
            CreateUIElementsOnCanvas(canvas);
            return;
        }
        
        // Try to find existing LevelText
        GameObject levelTextObj = GameObject.Find("LevelText");
        if (levelTextObj == null)
        {
            Debug.LogWarning("[ScoreUIManager] LevelText not found! Creating it on ScoreLevelCanvas...");
            Canvas canvas = scoreLevelCanvas.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ScoreUIManager] ScoreLevelCanvas does not have a Canvas component!");
                return;
            }
            CreateUIElementsOnCanvas(canvas);
            return;
        }
        
        // Connect to existing UI elements
        ConnectToFoundUIElements(scoreTextObj, levelTextObj);
    }
    
    private void CreateNewCanvasWithUI()
    {
        // Create new Canvas
        GameObject canvasObj = new GameObject("ScoreLevelCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        Debug.Log("[ScoreUIManager] Created new ScoreLevelCanvas");
        
        CreateUIElementsOnCanvas(canvas);
    }
    
    private void CreateUIElementsOnCanvas(Canvas canvas)
    {
        // Create ScoreText
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(canvas.transform, false);
        TMP_Text scoreTextComponent = scoreTextObj.AddComponent<TextMeshProUGUI>();
        scoreTextComponent.text = "0";
        scoreTextComponent.fontSize = 36;
        scoreTextComponent.color = Color.yellow;
        scoreTextComponent.alignment = TextAlignmentOptions.Center;
        
        // Position ScoreText at top left
        RectTransform scoreRect = scoreTextObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0f, 1f);
        scoreRect.anchorMax = new Vector2(0f, 1f);
        scoreRect.anchoredPosition = new Vector2(100, -50);
        scoreRect.sizeDelta = new Vector2(200, 50);
        
        // Create LevelText
        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(canvas.transform, false);
        TMP_Text levelTextComponent = levelTextObj.AddComponent<TextMeshProUGUI>();
        levelTextComponent.text = "1";
        levelTextComponent.fontSize = 36;
        levelTextComponent.color = Color.cyan;
        levelTextComponent.alignment = TextAlignmentOptions.Center;
        
        // Position LevelText at top right
        RectTransform levelRect = levelTextObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(1f, 1f);
        levelRect.anchorMax = new Vector2(1f, 1f);
        levelRect.anchoredPosition = new Vector2(-100, -50);
        levelRect.sizeDelta = new Vector2(200, 50);
        
        Debug.Log("[ScoreUIManager] Created ScoreText and LevelText on Canvas: " + canvas.name);
        
        // Connect to the newly created elements
        ConnectToFoundUIElements(scoreTextObj, levelTextObj);
    }
    
    private void ConnectToFoundUIElements(GameObject scoreTextObj, GameObject levelTextObj)
    {
        // Connect ScoreHandler to ScoreText
        if (scoreHandler != null)
        {
            TMP_Text scoreTextComponent = scoreTextObj.GetComponent<TMP_Text>();
            if (scoreTextComponent != null)
            {
                scoreHandler.SetScoreText(scoreTextComponent);
                Debug.Log("[ScoreUIManager] Connected ScoreHandler to ScoreText: " + scoreTextObj.name);
            }
            else
            {
                Debug.LogError("[ScoreUIManager] ScoreText does not have a TMP_Text component!");
            }
        }
        
        // Connect LevelDisplayManager to LevelText
        TMP_Text levelTextComponent = levelTextObj.GetComponent<TMP_Text>();
        if (levelTextComponent != null)
        {
            // Find or create LevelDisplayManager
            LevelDisplayManager levelDisplayManager = FindFirstObjectByType<LevelDisplayManager>();
            if (levelDisplayManager == null)
            {
                GameObject levelManagerObj = new GameObject("LevelDisplayManager");
                levelDisplayManager = levelManagerObj.AddComponent<LevelDisplayManager>();
            }
            levelDisplayManager.SetLevelText(levelTextComponent);
            Debug.Log("[ScoreUIManager] Connected LevelDisplayManager to LevelText: " + levelTextObj.name);
        }
        else
        {
            Debug.LogError("[ScoreUIManager] LevelText does not have a TMP_Text component!");
        }
    }
}