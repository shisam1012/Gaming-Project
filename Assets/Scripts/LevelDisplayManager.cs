using UnityEngine;
using TMPro;
using GamingProject;

public class LevelDisplayManager : MonoBehaviour
{
    private TMP_Text levelText;
    private LevelManager levelManager;
    
    void Start()
    {
        // Find the LevelManager
        levelManager = FindFirstObjectByType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogWarning("[LevelDisplayManager] LevelManager not found!");
            return;
        }
        
        // Update level display initially
        UpdateLevelDisplay();
        
        Debug.Log("[LevelDisplayManager] Level display manager initialized");
    }
    
    void Update()
    {
        // Update level display periodically
        if (levelManager != null && levelText != null)
        {
            UpdateLevelDisplay();
        }
    }
    
    public void SetLevelText(TMP_Text text)
    {
        levelText = text;
        Debug.Log("[LevelDisplayManager] Level text component assigned");
    }
    
    private void UpdateLevelDisplay()
    {
        if (levelText == null || levelManager == null) return;
        
        int currentLevel = levelManager.GetCurrentLevelNumber();
        
        // Just show the current level number, no fractions
        levelText.text = currentLevel.ToString();
        
        // Add some debug logging to understand what's happening
        if (Time.frameCount % 300 == 0) // Log every 5 seconds
        {
            Debug.Log($"[LevelDisplayManager] Displaying level: {currentLevel}");
        }
    }
}