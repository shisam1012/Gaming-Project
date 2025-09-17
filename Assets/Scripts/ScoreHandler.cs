using UnityEngine;
using TMPro;

public class ScoreHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public static ScoreHandler instance;
    public TMP_Text ScoreText => scoreText;
    private int currentScore;

    void Start()
    {
        // Only initialize score if this is the first/main instance
        if (instance == this)
        {
            if (currentScore == 0) // Only reset if not already set
            {
                currentScore = 0;
                Debug.Log("[ScoreHandler] ScoreHandler started, initial score: " + currentScore);
            }
            else
            {
                Debug.Log("[ScoreHandler] ScoreHandler restarted, maintaining score: " + currentScore);
            }
            
            UpdateScoreDisplay();
        }
    }
    
    private void UpdateScoreDisplay()
    {
        // Check if scoreText is assigned and still valid (not destroyed)
        if (scoreText == null)
        {
            Debug.LogWarning("[ScoreHandler] scoreText is NULL! Score updates won't be visible. Looking for ScoreText in scene...");
            
            // Try to find ScoreText in the scene first (our new naming convention)
            GameObject scoreTextObj = GameObject.Find("ScoreText");
            if (scoreTextObj != null)
            {
                scoreText = scoreTextObj.GetComponent<TMP_Text>();
                Debug.Log("[ScoreHandler] Found ScoreText in scene and assigned it!");
            }
            else
            {
                // Fallback to old naming convention
                scoreTextObj = GameObject.Find("Score Text");
                if (scoreTextObj != null)
                {
                    scoreText = scoreTextObj.GetComponent<TMP_Text>();
                    Debug.Log("[ScoreHandler] Found Score Text in scene and assigned it!");
                }
                else
                {
                    Debug.LogError("[ScoreHandler] No ScoreText or Score Text found in scene!");
                    return;
                }
            }
        }
        
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
            Debug.Log("[ScoreHandler] Score text updated to: Score: " + currentScore);
        }
    }

    private void Awake()
    {
        // Make this ScoreHandler persist across levels
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ScoreHandler] ScoreHandler instance created and marked as persistent");
        }
        else
        {
            // If another instance already exists, destroy this one
            Debug.Log("[ScoreHandler] Duplicate ScoreHandler found, destroying...");
            Destroy(gameObject);
        }
    }

    public void UpadteScore(int amount)
    {
        Debug.Log($"[ScoreHandler] UpadteScore called with amount: {amount}");
        
        int pointsAdded;
        if (amount <= 4)
        {
            pointsAdded = 2 * amount;
            currentScore += pointsAdded;
            Debug.Log($"[ScoreHandler] Small match bonus: {amount} stones × 2 = +{pointsAdded} points");
        }
        else
        {
            pointsAdded = 3 * amount;
            currentScore += pointsAdded;
            Debug.Log($"[ScoreHandler] Big match bonus: {amount} stones × 3 = +{pointsAdded} points");
        }
            
        Debug.Log($"[ScoreHandler] New total score: {currentScore}");
        
        // Update UI if scoreText exists, or try to reconnect if it doesn't
        UpdateScoreDisplay();
    }
    
    // Method to check if UI connection is still valid
    public bool IsUIConnected()
    {
        return scoreText != null;
    }
    
    // Method to force reconnection to UI
    public void ForceReconnectUI()
    {
        scoreText = null; // Clear the reference
        UpdateScoreDisplay(); // This will trigger the search and reconnection
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreDisplay();
    }
    
    // Method to reconnect UI text when advancing levels
    public void ReconnectUI(TMP_Text newScoreText)
    {
        scoreText = newScoreText;
        UpdateScoreDisplay();
        Debug.Log("[ScoreHandler] UI reconnected, score text updated");
    }
    
    // Simple method to set score text directly
    public void SetScoreText(TMP_Text newScoreText)
    {
        if (newScoreText != null)
        {
            scoreText = newScoreText;
            UpdateScoreDisplay();
            Debug.Log("[ScoreHandler] Score text set directly and updated");
        }
        else
        {
            Debug.LogWarning("[ScoreHandler] SetScoreText called with null reference!");
            // Try to find the UI element automatically
            UpdateScoreDisplay();
        }
    }

    public int CalculateTimeBonus(float timeRatio)
    {
        float bonus = 1;
        string bonusDescription = "No time bonus";

        if (timeRatio >= 0.9f)
        {
            bonus = 10f;
            bonusDescription = "Amazing speed! 10x bonus";
        }
        else if (timeRatio >= 0.75f)
        {
            bonus = 2.75f;
            bonusDescription = "Great speed! 2.75x bonus";
        }
        else if (timeRatio >= 0.5f)
        {
            bonus = 1.5f;
            bonusDescription = "Good speed! 1.5x bonus";
        }
        else if (timeRatio >= 0.25f)
        {
            bonus = 1.1f;
            bonusDescription = "Small speed bonus! 1.1x bonus";
        }

        int finalScore = Mathf.RoundToInt((currentScore + 100) * bonus);
        
        Debug.Log($"[ScoreHandler] Time Bonus Calculation:");
        Debug.Log($"  - Time ratio: {timeRatio:F2} ({timeRatio*100:F0}% time remaining)");
        Debug.Log($"  - Base score: {currentScore}");
        Debug.Log($"  - Completion bonus: +100");
        Debug.Log($"  - {bonusDescription}");
        Debug.Log($"  - Final calculation: ({currentScore} + 100) × {bonus} = {finalScore}");
        
        return finalScore;
    }

    public void ShowScore(bool active){
        scoreText.gameObject.SetActive(active);
    }

}
