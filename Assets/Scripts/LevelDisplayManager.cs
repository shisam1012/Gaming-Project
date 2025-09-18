using UnityEngine;
using TMPro;
using GamingProject;

public class LevelDisplayManager : MonoBehaviour
{
    private TMP_Text levelText;
    private LevelManager levelManager;
    private GameOverUI gameOverUI;
    private bool isGameOver = false;
    
    void Start()
    {
        // Find the LevelManager
        levelManager = FindFirstObjectByType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogWarning("[LevelDisplayManager] LevelManager not found!");
            return;
        }
        
        // Find the GameOverUI to listen for game over events
        gameOverUI = FindFirstObjectByType<GameOverUI>();
        if (gameOverUI == null)
        {
            Debug.LogWarning("[LevelDisplayManager] GameOverUI not found!");
        }
        
        // Update level display initially
        UpdateLevelDisplay();
        
        Debug.Log("[LevelDisplayManager] Level display manager initialized");
    }
    
    void Update()
    {
        // Check if game is over
        CheckGameOverState();
        
        // Update level display periodically only if game is not over
        if (levelManager != null && levelText != null && !isGameOver)
        {
            UpdateLevelDisplay();
        }
    }
    
    private void CheckGameOverState()
    {
        if (gameOverUI != null)
        {
            // Use the GameOverUI's IsShowing property for reliable detection
            bool gameOverActive = gameOverUI.IsShowing;
            
            if (gameOverActive && !isGameOver)
            {
                // Game just became over - hide level text
                isGameOver = true;
                HideLevelText();
                Debug.Log("[LevelDisplayManager] Game over detected - hiding level text");
            }
            else if (!gameOverActive && isGameOver)
            {
                // Game over ended - show level text again
                isGameOver = false;
                ShowLevelText();
                Debug.Log("[LevelDisplayManager] Game over ended - showing level text");
            }
        }
    }
    
    public void SetLevelText(TMP_Text text)
    {
        levelText = text;
        Debug.Log("[LevelDisplayManager] Level text component assigned");
    }
    
    private void HideLevelText()
    {
        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
            Debug.Log("[LevelDisplayManager] Level text hidden");
        }
    }
    
    private void ShowLevelText()
    {
        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            Debug.Log("[LevelDisplayManager] Level text shown");
        }
    }
    
    // Public methods that can be called externally
    public void OnGameOver()
    {
        isGameOver = true;
        HideLevelText();
        Debug.Log("[LevelDisplayManager] OnGameOver called - level text hidden");
    }
    
    public void OnGameRestart()
    {
        isGameOver = false;
        ShowLevelText();
        Debug.Log("[LevelDisplayManager] OnGameRestart called - level text shown");
    }
    
    private void UpdateLevelDisplay()
    {
        if (levelText == null || levelManager == null) return;
        
        int currentLevel = levelManager.GetCurrentLevelNumber();
        
        // Show "Level: X" format instead of just the number
        levelText.text = "Level: " + currentLevel.ToString();
        
        // Add some debug logging to understand what's happening
        if (Time.frameCount % 300 == 0) // Log every 5 seconds
        {
            Debug.Log($"[LevelDisplayManager] Displaying level: Level: {currentLevel}");
        }
    }
}