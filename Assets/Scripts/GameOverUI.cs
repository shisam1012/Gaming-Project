using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
        isShowing = false;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // Resume the game
        Time.timeScale = 1f;
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

    // Placeholder method for score - will be connected to actual score logic later
    private int GetCurrentScore()
    {
        // TODO: Connect to actual scoring system when implemented
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
        Debug.Log("[GameOverUI] Try Again button pressed");

        // Hide the game over screen
        HideGameOver();

        // Reset everything to level 1
        RestartGame();
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
        // Reset level manager to first level
        if (levelManager != null)
        {
            levelManager.ResetToFirstLevel();
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
    
    private void PlayGameOverSound()
    {
        Debug.Log("[GameOverUI] Game Over sound disabled - add back later if needed");
        // Sound effects removed for now
    }
    }
}