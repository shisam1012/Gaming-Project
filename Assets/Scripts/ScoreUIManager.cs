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
        // Find existing ScoreLevelCanvas
        GameObject scoreLevelCanvas = GameObject.Find("ScoreLevelCanvas");
        if (scoreLevelCanvas == null)
        {
            Debug.LogError("[ScoreUIManager] ScoreLevelCanvas not found! Please make sure you have a Canvas named 'ScoreLevelCanvas' in your scene.");
            return;
        }
        
        // Find existing ScoreText
        GameObject scoreTextObj = GameObject.Find("ScoreText");
        if (scoreTextObj == null)
        {
            Debug.LogError("[ScoreUIManager] ScoreText not found! Please make sure you have a Text component named 'ScoreText' in your ScoreLevelCanvas.");
            return;
        }
        
        // Find existing LevelText
        GameObject levelTextObj = GameObject.Find("LevelText");
        if (levelTextObj == null)
        {
            Debug.LogError("[ScoreUIManager] LevelText not found! Please make sure you have a Text component named 'LevelText' in your ScoreLevelCanvas.");
            return;
        }
        
        // Connect ScoreHandler to existing ScoreText
        if (scoreHandler != null)
        {
            TMP_Text scoreTextComponent = scoreTextObj.GetComponent<TMP_Text>();
            if (scoreTextComponent != null)
            {
                scoreHandler.SetScoreText(scoreTextComponent);
                Debug.Log("[ScoreUIManager] Connected ScoreHandler to existing ScoreText");
            }
            else
            {
                Debug.LogError("[ScoreUIManager] ScoreText does not have a TMP_Text component!");
            }
        }
        
        // Connect LevelDisplayManager to existing LevelText
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
            Debug.Log("[ScoreUIManager] Connected LevelDisplayManager to existing LevelText");
        }
        else
        {
            Debug.LogError("[ScoreUIManager] LevelText does not have a TMP_Text component!");
        }
    }
}