using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace GamingProject
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Levels Order (will advance in this order)")]
        public List<LevelConfig> levels = new List<LevelConfig>();
        [SerializeField] private int currentIndex = 0;

    [Header("References")]
    public Board board;
    
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;
    
    [Header("Events")]
    public UnityEvent onTimeUp;
    public UnityEvent<float> onTimerStart; 
    public UnityEvent onTimerEnd;    [Header("Timing")]
    [SerializeField] private float betweenLevelDelay = 0.75f;

    private float timeLeft;
    private bool running;

    // [SerializeField] private GamingProject.ResultScreen resultScreen;
    // public GamingProject.ResultScreen ResultScreen => resultScreen;
    
    // Comment out direct references - use reflection approach that was working
    // [SerializeField] private ResultScreen resultScreen;
    // public ResultScreen ResultScreen => resultScreen;

    private void Awake()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
        }
        
        // Ensure ScoreUIManager exists to create our UI elements
        EnsureScoreUIManagerExists();
        
        // Ensure Result UI exists
        if (FindFirstObjectByType<ResultUIManager>() == null)
        {
            GameObject resultUIManagerObj = new GameObject("ResultUIManager");
            resultUIManagerObj.AddComponent<ResultUIManager>();
            Debug.Log("[LevelManager] Created ResultUIManager to ensure result UI exists");
        }
    }
    
    private void EnsureScoreUIManagerExists()
    {
        // Check if ScoreUIManager exists in scene
        ScoreUIManager scoreUIManager = FindFirstObjectByType<ScoreUIManager>();
        if (scoreUIManager == null)
        {
            Debug.Log("[LevelManager] No ScoreUIManager found, creating one...");
            GameObject scoreUIManagerObj = new GameObject("ScoreUIManager");
            scoreUIManager = scoreUIManagerObj.AddComponent<ScoreUIManager>();
            scoreUIManager.autoCreateUI = true;
            Debug.Log("[LevelManager] Created ScoreUIManager with autoCreateUI enabled");
        }
        else
        {
            Debug.Log("[LevelManager] ScoreUIManager found in scene");
        }
        
        // Trigger UI creation
        scoreUIManager.CreateUIOnGameStart();
    }
    
    private void MakeScoreUIPersistent()
    {
        // Find the Canvas that contains our score UI and make it persistent
        if (scoreText != null)
        {
            // Find the Canvas that contains the scoreText
            Canvas scoreCanvas = scoreText.GetComponentInParent<Canvas>();
            if (scoreCanvas != null)
            {
                // Check if this Canvas is already persistent
                if (scoreCanvas.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    DontDestroyOnLoad(scoreCanvas.gameObject);
                    Debug.Log("[LevelManager] Made Score UI Canvas persistent: " + scoreCanvas.name);
                }
                else
                {
                    Debug.Log("[LevelManager] Score UI Canvas is already persistent: " + scoreCanvas.name);
                }
            }
            else
            {
                Debug.LogWarning("[LevelManager] Could not find Canvas parent for scoreText!");
            }
        }
        else
        {
            Debug.LogWarning("[LevelManager] scoreText is null - cannot make Score UI persistent yet");
        }
    }

    private void OnEnable()
    {
        if (board != null)
            board.onWin.AddListener(OnBoardWin);
    }

    private void OnDisable()
    {
        if (board != null)
            board.onWin.RemoveListener(OnBoardWin);
    }

    private void Start()
    {
        if (board == null)
        {
            Debug.LogError("[LevelManager] Missing Board reference.");
            return;
        }
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("[LevelManager] No levels assigned.");
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, levels.Count - 1);
        
        // Initialize timer to prevent immediate timeout
        running = false;
        timeLeft = 30f; // Default time if level config is missing
        
        // Don't auto-load the level! Wait for explicit start from UI
        // LoadLevel(currentIndex); // REMOVED - this was causing premature timer start
        
        Debug.Log($"[LevelManager] LevelManager initialized with {levels.Count} levels, current index: {currentIndex}");
        Debug.Log($"[LevelManager] Ready to start. Call LoadLevelPublic() to begin the game.");
        
        // Initialize the UI immediately so it's visible
        InitializeUI();
        
        // Force connect ScoreHandler to our UI
        ConnectScoreHandler();
    }
    
    private void ConnectScoreHandler()
    {
        Debug.Log("[LevelManager] === ConnectScoreHandler Debug ===");
        Debug.Log("[LevelManager] ScoreHandler.instance exists: " + (ScoreHandler.instance != null));
        Debug.Log("[LevelManager] scoreText assigned: " + (scoreText != null));
        
        // First, try to find ScoreHandler in scene if instance is null
        if (ScoreHandler.instance == null)
        {
            ScoreHandler foundHandler = FindFirstObjectByType<ScoreHandler>();
            if (foundHandler != null)
            {
                Debug.Log("[LevelManager] Found ScoreHandler in scene: " + foundHandler.name);
            }
            else
            {
                Debug.LogError("[LevelManager] No ScoreHandler found in scene! ScoreUIManager should have created it...");
                // Try to trigger ScoreUIManager creation again
                EnsureScoreUIManagerExists();
                return;
            }
        }
        
        // If scoreText is null, try to find your existing UI elements
        if (scoreText == null)
        {
            Debug.Log("[LevelManager] scoreText is null, searching for existing ScoreText in ScoreLevelCanvas...");
            GameObject scoreTextObj = GameObject.Find("ScoreText");
            if (scoreTextObj != null)
            {
                scoreText = scoreTextObj.GetComponent<TMP_Text>();
                Debug.Log("[LevelManager] Found and assigned ScoreText: " + scoreTextObj.name);
            }
            else
            {
                Debug.LogError("[LevelManager] Could not find ScoreText! Please make sure you have a ScoreText in your ScoreLevelCanvas.");
                return;
            }
        }
        
        // If levelText is null, try to find your existing LevelText
        if (levelText == null)
        {
            Debug.Log("[LevelManager] levelText is null, searching for existing LevelText in ScoreLevelCanvas...");
            GameObject levelTextObj = GameObject.Find("LevelText");
            if (levelTextObj != null)
            {
                levelText = levelTextObj.GetComponent<TMP_Text>();
                Debug.Log("[LevelManager] Found and assigned LevelText: " + levelTextObj.name);
            }
            else
            {
                Debug.LogWarning("[LevelManager] Could not find LevelText!");
            }
        }
        
        // Find or ensure ScoreHandler exists and connect it to our UI
        if (ScoreHandler.instance != null && scoreText != null)
        {
            // Force reconnection to handle level transitions
            ScoreHandler.instance.SetScoreText(scoreText);
            // Update display with current score
            int currentScore = ScoreHandler.instance.GetCurrentScore();
            scoreText.text = currentScore.ToString(); // Just the number
            Debug.Log("[LevelManager] ✓ Successfully connected ScoreHandler to UI. Current score: " + currentScore);
            
            // Now that we have the UI elements connected, make them persistent
            MakeScoreUIPersistent();
        }
        else if (ScoreHandler.instance != null && scoreText == null)
        {
            Debug.LogError("[LevelManager] ScoreHandler exists but scoreText is null! Could not find ScoreText in scene.");
            // Try to force ScoreHandler to find the UI
            ScoreHandler.instance.ForceReconnectUI();
        }
        else if (ScoreHandler.instance == null)
        {
            Debug.LogError("[LevelManager] ScoreHandler.instance is NULL! Make sure ScoreUIManager created it properly.");
        }
        else
        {
            Debug.LogWarning("[LevelManager] Could not connect ScoreHandler - ScoreHandler.instance: " + (ScoreHandler.instance != null ? "EXISTS" : "NULL") + ", scoreText: " + (scoreText != null ? "EXISTS" : "NULL"));
        }
    }
    
    private void InitializeUI()
    {
        // Try to find UI elements if they're not assigned
        if (scoreText == null)
        {
            GameObject scoreTextObj = GameObject.Find("ScoreText");
            if (scoreTextObj != null)
            {
                scoreText = scoreTextObj.GetComponent<TMP_Text>();
                Debug.Log("[LevelManager] Found ScoreText during initialization: " + scoreTextObj.name);
            }
        }
        
        if (levelText == null)
        {
            GameObject levelTextObj = GameObject.Find("LevelText");
            if (levelTextObj != null)
            {
                levelText = levelTextObj.GetComponent<TMP_Text>();
                Debug.Log("[LevelManager] Found LevelText during initialization: " + levelTextObj.name);
            }
        }
        
        // Set initial values for UI elements
        if (levelText != null)
        {
            levelText.text = (currentIndex + 1).ToString(); // Just the number
            levelText.gameObject.SetActive(true); // Make sure it's active
            Debug.Log("[LevelManager] Initialized level text: " + levelText.text);
        }
        else
        {
            Debug.LogError("[LevelManager] levelText is not assigned and could not be found! Make sure you have a LevelText in your ScoreLevelCanvas.");
        }
        
        if (scoreText != null)
        {
            scoreText.text = "0"; // Just the number
            scoreText.gameObject.SetActive(true); // Make sure it's active
            Debug.Log("[LevelManager] Initialized score text: " + scoreText.text);
        }
        else
        {
            Debug.LogError("[LevelManager] scoreText is not assigned and could not be found! Make sure you have a ScoreText in your ScoreLevelCanvas.");
        }
    }

    private void Update()
    {
        if (!running) 
        {
            // Debug when timer is not running
            if (Time.frameCount % 120 == 0) // Log every 2 seconds
            {
                Debug.Log("[LevelManager] Timer not running, waiting for game start");
            }
            return;
        }
        
        timeLeft -= Time.deltaTime;
        
        // Debug logging to help troubleshoot
        if (timeLeft <= 10f && timeLeft > 9.9f)
        {
            Debug.Log($"[LevelManager] Timer countdown: {timeLeft:F2} seconds left");
        }
        else if (timeLeft <= 5f && timeLeft > 4.9f)
        {
            Debug.Log($"[LevelManager] Timer warning: {timeLeft:F2} seconds left");
        }
        
        if (timeLeft <= 0f)
        {
            running = false;
            timeLeft = 0f; 
            Debug.LogWarning($"[LevelManager] Time up! Final time: {timeLeft}");
            
            // Get current score for time-out screen using GameObject.Find (working approach)
            var scoreHandlerObj = GameObject.Find("ScoreHandler");
            if (scoreHandlerObj != null)
            {
                var scoreHandler = scoreHandlerObj.GetComponent<MonoBehaviour>();
                var method = scoreHandler.GetType().GetMethod("GetCurrentScore");
                if (method != null)
                {
                    int totalScore = (int)method.Invoke(scoreHandler, null);
                    
                    var resultScreenObj = GameObject.Find("ResultScreen");
                    if (resultScreenObj != null)
                    {
                        var resultScreen = resultScreenObj.GetComponent<MonoBehaviour>();
                        var setUpMethod = resultScreen.GetType().GetMethod("SetUpTimeOut");
                        if (setUpMethod != null)
                        {
                            setUpMethod.Invoke(resultScreen, new object[] { totalScore });
                        }
                    }
                }
            }
            
            // Timer events for UI updates (from feat/timer branch)
            Debug.Log($"[LevelManager] Invoking onTimerEnd and onTimeUp events");
            onTimerEnd?.Invoke();
            onTimeUp?.Invoke();
        }
    }

    private void LoadLevel(int index)
    {
        var cfg = levels[index];
        timeLeft = Mathf.Max(1f, cfg.timeLimitSeconds);
        board.ApplyLevel(cfg);
        running = true;
        
        // Ensure Score UI is visible and reconnected after level transition
        EnsureScoreUIVisible();
        
        // Update UI directly using the assigned references
        UpdateScoreAndLevelUI();
        
        // Make sure ScoreHandler is connected every level
        ConnectScoreHandler();
        
        onTimerStart?.Invoke(timeLeft);
        
        Debug.Log($"[LevelManager] Loaded level {index+1}/{levels.Count}: {cfg.name} (Time: {timeLeft}s)");
    }
    
    private void EnsureScoreUIVisible()
    {
        // Make sure the persistent Score UI is active and visible
        if (scoreText != null)
        {
            // Ensure the scoreText GameObject and its parents are active
            scoreText.gameObject.SetActive(true);
            
            Transform parent = scoreText.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                parent = parent.parent;
            }
            
            Debug.Log("[LevelManager] Ensured Score UI is visible and active");
        }
        else
        {
            Debug.LogWarning("[LevelManager] scoreText is null, cannot ensure visibility");
        }
    }
    
    private void UpdateScoreAndLevelUI()
    {
        // Update level text
        if (levelText != null)
        {
            levelText.text = (currentIndex + 1).ToString(); // Just the number
            Debug.Log("[LevelManager] Updated level text to: " + (currentIndex + 1));
        }
        
        // Update score text and connect ScoreHandler
        if (scoreText != null)
        {
            if (ScoreHandler.instance != null)
            {
                ScoreHandler.instance.SetScoreText(scoreText);
                scoreText.text = ScoreHandler.instance.GetCurrentScore().ToString(); // Just the number
                Debug.Log("[LevelManager] Connected ScoreHandler to scoreText, current score: " + ScoreHandler.instance.GetCurrentScore());
            }
            else
            {
                scoreText.text = "0"; // Just the number
                Debug.Log("[LevelManager] ScoreHandler not found, setting score to 0");
            }
        }
        else
        {
            Debug.LogError("[LevelManager] scoreText reference is null! Please assign it in the inspector.");
        }
    }


    private void OnBoardWin()
    {
        float timeRatio = timeLeft / levels[currentIndex].timeLimitSeconds;
        
        // Use GameObject.Find approach that was working
        var scoreHandlerObj = GameObject.Find("ScoreHandler");
        int totalScore = 0;
        if (scoreHandlerObj != null)
        {
            var scoreHandler = scoreHandlerObj.GetComponent<MonoBehaviour>();
            var method = scoreHandler.GetType().GetMethod("CalculateTimeBonus");
            if (method != null)
            {
                totalScore = (int)method.Invoke(scoreHandler, new object[] { timeRatio });
            }
        }

        Debug.LogWarning("------total score " + totalScore);

        StartCoroutine(ShowWinAfterDelay(totalScore));
        running = false;
        // Keep timer events for UI consistency
        onTimerEnd?.Invoke();
        StartCoroutine(AdvanceAfterDelay(betweenLevelDelay));
    }

    private IEnumerator ShowWinAfterDelay(int totalScore)
    {
        yield return new WaitForSeconds(betweenLevelDelay);
        
        // Use GameObject.Find approach for ResultScreen
        var resultScreenObj = GameObject.Find("ResultScreen");
        if (resultScreenObj != null)
        {
            var resultScreenComponent = resultScreenObj.GetComponent<MonoBehaviour>();
            var method = resultScreenComponent.GetType().GetMethod("SetUp");
            if (method != null)
            {
                method.Invoke(resultScreenComponent, new object[] { totalScore, "You Won!" });
            }
        }
    }


    public IEnumerator RepeatAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadLevel(currentIndex);
    }

    public IEnumerator AdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentIndex++;
        if (currentIndex >= levels.Count)
        {
            currentIndex = 0;
        }
        LoadLevel(currentIndex);
    }

    private IEnumerator ReloadCurrentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadLevel(currentIndex);
    }

    public void JumpToLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;
        currentIndex = index;
        LoadLevel(currentIndex);
    }

    public void AdvanceToNextLevel()
    {
        currentIndex++;
        if (currentIndex >= levels.Count)
        {
            currentIndex = 0;
        }
        LoadLevel(currentIndex);
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentIndex);
    }

    public void LoadLevelPublic(int index)
    {
        if (index < 0 || index >= levels.Count) return;
        currentIndex = index;
        LoadLevel(currentIndex);
    }

    public float TimeLeft => Mathf.Max(0f, timeLeft);
    
    public float GetCurrentLevelTimeLimit()
    {
        if (currentIndex >= 0 && currentIndex < levels.Count)
        {
            return levels[currentIndex].timeLimitSeconds;
        }
        return 60f;
    }
    
    public bool IsTimerRunning => running;
    
    public float GetTimeProgress()
    {
        float totalTime = GetCurrentLevelTimeLimit();
        return totalTime > 0 ? (totalTime - timeLeft) / totalTime : 0f;
    }
    
    public void PauseTimer()
    {
        running = false;
    }
    
    public void ResumeTimer()
    {
        if (timeLeft > 0)
        {
            running = true;
        }
    }
    
    public void AddTime(float seconds)
    {
        timeLeft += seconds;
        Debug.Log($"[LevelManager] Added {seconds} seconds. New time: {timeLeft}");
    }
    
    public void ResetToFirstLevel()
    {
        currentIndex = 0;
        LoadLevel(currentIndex);
        Debug.Log("[LevelManager] Reset to first level");
    }
    
    public int GetCurrentLevelNumber()
    {
        return currentIndex + 1; // Return 1-based level number for display
    }
    
    public int GetTotalLevels()
    {
        return levels != null ? levels.Count : 0;
    }
    }
}
