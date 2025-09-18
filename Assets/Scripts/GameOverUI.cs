using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace GamingProject
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI timeUpText;
        [SerializeField] private TextMeshProUGUI levelReachedText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button quitButton;
        
        [Header("UI Text Settings")]
        [SerializeField] private string timeUpMessage = "TIME'S UP!";
        [SerializeField] private string levelFormat = "Level Reached: {0}";
        [SerializeField] private string scoreFormat = "Score: {0}";
        [SerializeField] private string tryAgainText = "Try Again?";
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private LevelManager levelManager;
    private bool isShowing = false;
    
    private void Awake()
    {
        Debug.Log("[GameOverUI] Awake called - initializing GameOverUI");
        
        // FIRST: Explicitly hide the Game Over panel immediately
        if (gameOverPanel != null)
        {
            Debug.Log($"[GameOverUI] Found gameOverPanel, hiding it immediately. Current state: {gameOverPanel.activeInHierarchy}");
            gameOverPanel.SetActive(false);
            Debug.Log($"[GameOverUI] gameOverPanel hidden. New state: {gameOverPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[GameOverUI] gameOverPanel is NULL! Please assign it in the Inspector!");
        }
        
        levelManager = FindFirstObjectByType<LevelManager>();
        Debug.Log($"[GameOverUI] LevelManager found: {levelManager != null}");
        
        // Setup canvas group for fading
        if (canvasGroup == null)
        {
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
        }
        
        Debug.Log("[GameOverUI] GameOverUI initialized successfully");

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(OnTryAgain);
            var buttonText = tryAgainButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = tryAgainText;
            }
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuit);
        }
        
        HideGameOver();
    }
    
    private void Start()
    {
        Debug.Log("[GameOverUI] Start called - subscribing to events");
        
        // Subscribe to level manager events
        if (levelManager != null)
        {
            levelManager.onTimeUp.AddListener(OnTimeUp);
            Debug.Log("[GameOverUI] Successfully subscribed to onTimeUp event");
        }
        else
        {
            Debug.LogError("[GameOverUI] LevelManager is null! Cannot subscribe to events.");
        }
    }
    
    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.onTimeUp.RemoveListener(OnTimeUp);
        }
        
        // Remove button listeners
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveListener(OnTryAgain);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuit);
        }
    }
    
    public void OnTimeUp()
    {
        Debug.Log("[GameOverUI] OnTimeUp called - showing game over screen");
        ShowGameOver();
    }
    
    public void ShowGameOver()
    {
        Debug.Log($"[GameOverUI] ShowGameOver called - isShowing: {isShowing}");
        
        if (isShowing) 
        {
            Debug.LogWarning("[GameOverUI] Already showing, skipping");
            return;
        }
        
        isShowing = true;
        
        // Hide level text when game over occurs
        var levelDisplayManager = FindFirstObjectByType<LevelDisplayManager>();
        if (levelDisplayManager != null)
        {
            levelDisplayManager.OnGameOver();
        }
        
        // Hide timer when game over occurs
        HideTimer();
        
        // Play game over sound
        PlayGameOverSound();
        
        // Update UI text
        UpdateGameOverText();
        
        // Show panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("[GameOverUI] Game over panel activated");
        }
        else
        {
            Debug.LogError("[GameOverUI] Game over panel is null!");
        }
        
        // Fade in animation
        StartCoroutine(FadeIn());
        
        // Pause the game
        Time.timeScale = 0f;
        
        Debug.Log("[GameOverUI] Game Over screen displayed");
    }
    
    public void HideGameOver()
    {
        Debug.Log("[GameOverUI] === HIDING GAME OVER ===");
        isShowing = false;
        
        // Show level text again when game over is hidden
        var levelDisplayManager = FindFirstObjectByType<LevelDisplayManager>();
        if (levelDisplayManager != null)
        {
            Debug.Log("[GameOverUI] Calling levelDisplayManager.OnGameRestart()");
            levelDisplayManager.OnGameRestart();
        }
        else
        {
            Debug.LogWarning("[GameOverUI] No LevelDisplayManager found");
        }
        
        // Show timer again when game restarts
        Debug.Log("[GameOverUI] Calling ShowTimer()...");
        ShowTimer();
        
        if (gameOverPanel != null)
        {
            Debug.Log("[GameOverUI] Deactivating game over panel");
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[GameOverUI] gameOverPanel is null");
        }
        
        if (canvasGroup != null)
        {
            Debug.Log("[GameOverUI] Setting canvas group alpha to 0");
            canvasGroup.alpha = 0f;
        }
        
        // Resume the game
        Debug.Log("[GameOverUI] Resuming game (Time.timeScale = 1f)");
        Time.timeScale = 1f;
        
        Debug.Log("[GameOverUI] === GAME OVER HIDDEN ===");
    }
    
    private void UpdateGameOverText()
    {
        // Time's up message
        if (timeUpText != null)
        {
            timeUpText.text = timeUpMessage;
        }

        // Level reached display
        if (levelReachedText != null)
        {
            int levelReached = levelManager != null ? levelManager.GetCurrentLevelNumber() : 0;
            levelReachedText.text = string.Format(levelFormat, levelReached);
        }

        // Score display (placeholder - returns 0 for now)
        if (scoreText != null)
        {
            int currentScore = GetCurrentScore();
            scoreText.text = string.Format(scoreFormat, currentScore);
        }
    }

    // Get current score from ScoreHandler
    private int GetCurrentScore()
    {
        // First try to get score from ScoreHandler.instance
        if (ScoreHandler.instance != null)
        {
            int score = ScoreHandler.instance.GetCurrentScore();
            Debug.Log($"[GameOverUI] Got score from ScoreHandler.instance: {score}");
            return score;
        }
        
        // Fallback: Try to find ScoreHandler GameObject (same approach as Board.cs)
        var scoreHandlerObj = GameObject.Find("ScoreHandler");
        if (scoreHandlerObj != null)
        {
            var scoreHandler = scoreHandlerObj.GetComponent<MonoBehaviour>();
            var method = scoreHandler.GetType().GetMethod("GetCurrentScore");
            if (method != null)
            {
                int score = (int)method.Invoke(scoreHandler, null);
                Debug.Log($"[GameOverUI] Got score from ScoreHandler GameObject: {score}");
                return score;
            }
        }
        
        Debug.LogWarning("[GameOverUI] Could not find ScoreHandler to get current score, returning 0");
        return 0;
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        float startAlpha = 0f;
        
        canvasGroup.alpha = startAlpha;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time since we paused the game
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    public void OnTryAgain()
    {
        Debug.Log("[GameOverUI] === TRY AGAIN BUTTON PRESSED ===");

        // First reset the game (this will reload the level and start the timer)
        Debug.Log("[GameOverUI] Step 1: Restarting game...");
        RestartGame();
        
        // Then hide the game over screen (this calls ShowTimer)
        Debug.Log("[GameOverUI] Step 2: Hiding game over screen...");
        HideGameOver();
        
        Debug.Log("[GameOverUI] === TRY AGAIN COMPLETE ===");
    }
    
    public void OnQuit()
    {
        Debug.Log("[GameOverUI] Quit button pressed");
        
        // If in editor, stop playing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If in build, quit application
        Application.Quit();
        #endif
    }
    
    private void RestartGame()
    {
        Debug.Log("[GameOverUI] Restarting game and resetting score");
        
        // Reset score to 0 when trying again
        if (ScoreHandler.instance != null)
        {
            ScoreHandler.instance.SetScore(0);
            Debug.Log("[GameOverUI] Score reset to 0");
        }
        else
        {
            Debug.LogWarning("[GameOverUI] ScoreHandler.instance is null, cannot reset score");
        }
        
        // Reset level manager to first level
        if (levelManager != null)
        {
            levelManager.ResetToFirstLevel();
            Debug.Log("[GameOverUI] Level manager reset to first level");
        }
        else
        {
            Debug.LogError("[GameOverUI] LevelManager is null, cannot reset!");
        }
        
        // Alternative: Reload the entire scene (more thorough reset)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Public methods for manual control
    public void ShowGameOverManual(int level)
    {
        // Manual override for showing game over with specific level
        if (levelReachedText != null)
        {
            levelReachedText.text = string.Format(levelFormat, level);
        }
        
        ShowGameOver();
    }
    
    public bool IsShowing => isShowing;
    
    // Public methods to control timer visibility
    public void SetTimerVisible(bool visible)
    {
        if (visible)
        {
            ShowTimer();
        }
        else
        {
            HideTimer();
        }
    }
    
    // Manual test method - you can call this from Inspector or other scripts
    [ContextMenu("Force Show Timer (Test)")]
    public void ForceShowTimerTest()
    {
        Debug.Log("[GameOverUI] === MANUAL TIMER TEST ===");
        ShowTimer();
        
        // Also try to manually find and activate any hidden timer objects
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("timer"))
            {
                Debug.Log($"[GameOverUI] Found timer object: {obj.name} - Active: {obj.activeSelf}");
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                    Debug.Log($"[GameOverUI] Activated timer object: {obj.name}");
                }
            }
        }
    }
    
    private void PlayGameOverSound()
    {
        Debug.Log("[GameOverUI] Game Over sound disabled - add back later if needed");
        // Sound effects removed for now
    }
    
    private void HideTimer()
    {
        Debug.Log("[GameOverUI] Hiding timer UI");
        
        // Store reference to timer components for easier restoration later
        var timerUISlider = FindFirstObjectByType<TimerUI_Slider>();
        if (timerUISlider != null)
        {
            // Just hide the TimerUI_Slider component itself, not its parent
            timerUISlider.gameObject.SetActive(false);
            Debug.Log($"[GameOverUI] TimerUI_Slider hidden: {timerUISlider.name}");
        }
        else
        {
            Debug.LogWarning("[GameOverUI] No TimerUI_Slider found to hide");
        }
        
        // Hide regular TimerUI if it exists
        var timerUI = FindFirstObjectByType<TimerUI>();
        if (timerUI != null)
        {
            timerUI.gameObject.SetActive(false);
            Debug.Log($"[GameOverUI] TimerUI hidden: {timerUI.name}");
        }
        
        Debug.Log("[GameOverUI] Timer hiding complete");
    }
    
    private void ShowTimer()
    {
        Debug.Log("[GameOverUI] Showing timer UI");
        
        // Find and show TimerUI_Slider
        var timerUISlider = FindFirstObjectByType<TimerUI_Slider>();
        if (timerUISlider != null)
        {
            timerUISlider.gameObject.SetActive(true);
            Debug.Log("[GameOverUI] TimerUI_Slider shown");
        }
        else
        {
            Debug.LogWarning("[GameOverUI] No TimerUI_Slider found to show");
        }
        
        // Find and show regular TimerUI if it exists
        var timerUI = FindFirstObjectByType<TimerUI>();
        if (timerUI != null)
        {
            timerUI.gameObject.SetActive(true);
            Debug.Log("[GameOverUI] TimerUI shown");
        }
    }
    
    private void ForceTimerRefresh()
    {
        // Removed - timer should work automatically after restart
        Debug.Log("[GameOverUI] ForceTimerRefresh - doing nothing, timer should work automatically");
    }
    }
}