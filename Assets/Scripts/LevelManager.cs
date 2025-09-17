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
            // Try to find ScoreLevelCanvas directly if scoreText is null
            GameObject scoreLevelCanvas = GameObject.Find("ScoreLevelCanvas");
            if (scoreLevelCanvas != null)
            {
                Canvas canvas = scoreLevelCanvas.GetComponent<Canvas>();
                if (canvas != null)
                {
                    if (canvas.gameObject.scene.name != "DontDestroyOnLoad")
                    {
                        DontDestroyOnLoad(canvas.gameObject);
                        Debug.Log("[LevelManager] Made ScoreLevelCanvas persistent directly: " + canvas.name);
                    }
                    else
                    {
                        Debug.Log("[LevelManager] ScoreLevelCanvas is already persistent: " + canvas.name);
                    }
                }
                else
                {
                    Debug.LogWarning("[LevelManager] ScoreLevelCanvas found but has no Canvas component!");
                }
            }
            else
            {
                Debug.LogWarning("[LevelManager] scoreText is null and ScoreLevelCanvas not found - cannot make Score UI persistent yet");
            }
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
        
        // IMMEDIATELY FIND AND SETUP UI
        Debug.Log("[LevelManager] === FINDING UI ELEMENTS ===");
        
        // Find ScoreText anywhere
        GameObject scoreObj = GameObject.Find("ScoreText");
        if (scoreObj != null)
        {
            scoreText = scoreObj.GetComponent<TMP_Text>();
            scoreText.text = "Score: 0";
            scoreText.gameObject.SetActive(true);
            Debug.Log("[LevelManager] ✓ FOUND and SET ScoreText: " + scoreText.text);
        }
        else
        {
            Debug.LogError("[LevelManager] ✗ SCORETEXT NOT FOUND!");
        }
        
        // Find LevelText anywhere
        GameObject levelObj = GameObject.Find("LevelText");
        if (levelObj != null)
        {
            levelText = levelObj.GetComponent<TMP_Text>();
            levelText.text = "Level: " + (currentIndex + 1).ToString();
            levelText.gameObject.SetActive(true);
            Debug.Log("[LevelManager] ✓ FOUND and SET LevelText: " + levelText.text);
        }
        else
        {
            Debug.LogError("[LevelManager] ✗ LEVELTEXT NOT FOUND!");
        }
        
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
        Debug.Log("[LevelManager] === Connecting ScoreHandler ===");
        
        // Just connect ScoreHandler to our found UI elements
        if (ScoreHandler.instance != null && scoreText != null)
        {
            ScoreHandler.instance.SetScoreText(scoreText);
            Debug.Log("[LevelManager] ✓ Connected ScoreHandler to scoreText");
        }
        else
        {
            Debug.LogWarning("[LevelManager] Cannot connect ScoreHandler - ScoreHandler.instance: " + (ScoreHandler.instance != null) + ", scoreText: " + (scoreText != null));
        }
    }

    private void InitializeUI()
    {
        Debug.Log("[LevelManager] Looking for your ScoreLevelCanvas with ScoreText and LevelText...");
        
        // Find your ScoreLevelCanvas
        GameObject scoreLevelCanvas = GameObject.Find("ScoreLevelCanvas");
        if (scoreLevelCanvas != null)
        {
            Debug.Log("[LevelManager] Found ScoreLevelCanvas!");
            
            // Look for ScoreText within the canvas
            if (scoreText == null)
            {
                Transform scoreTransform = scoreLevelCanvas.transform.Find("ScoreText");
                if (scoreTransform != null)
                {
                    scoreText = scoreTransform.GetComponent<TMP_Text>();
                    Debug.Log("[LevelManager] Found ScoreText in ScoreLevelCanvas: " + scoreTransform.name);
                }
                else
                {
                    Debug.LogError("[LevelManager] ScoreText not found as child of ScoreLevelCanvas!");
                }
            }
            
            // Look for LevelText within the canvas
            if (levelText == null)
            {
                Transform levelTransform = scoreLevelCanvas.transform.Find("LevelText");
                if (levelTransform != null)
                {
                    levelText = levelTransform.GetComponent<TMP_Text>();
                    Debug.Log("[LevelManager] Found LevelText in ScoreLevelCanvas: " + levelTransform.name);
                }
                else
                {
                    Debug.LogError("[LevelManager] LevelText not found as child of ScoreLevelCanvas!");
                }
            }
        }
        else
        {
            Debug.LogError("[LevelManager] ScoreLevelCanvas not found! Make sure you have a GameObject named 'ScoreLevelCanvas' in your scene.");
        }
        
        // Set initial values for UI elements (don't touch their positioning)
        if (levelText != null)
        {
            levelText.text = "Level: " + (currentIndex + 1).ToString();
            levelText.gameObject.SetActive(true);
            Debug.Log("[LevelManager] Set level text to: " + levelText.text);
        }
        else
        {
            Debug.LogError("[LevelManager] LevelText is null - cannot set level text!");
        }
        
        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
            scoreText.gameObject.SetActive(true);
            Debug.Log("[LevelManager] Set score text to: " + scoreText.text);
        }
        else
        {
            Debug.LogError("[LevelManager] ScoreText is null - cannot set score text!");
        }
        
        // Fix UI positioning to ensure they're visible
        FixUIPositioning();
    }
    
    private void FixUIPositioning()
    {
        Debug.Log("[LevelManager] Fixing UI positioning to be centered above timer bar...");
        
        // Fix ScoreText positioning (bottom-center-left)
        if (scoreText != null)
        {
            RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
            if (scoreRect != null)
            {
                // Anchor to bottom-center
                scoreRect.anchorMin = new Vector2(0.5f, 0);
                scoreRect.anchorMax = new Vector2(0.5f, 0);
                scoreRect.pivot = new Vector2(1, 0); // Right-aligned so it sits to the left of center
                
                // Position to the left of center, above timer (120px from bottom)
                scoreRect.anchoredPosition = new Vector2(-20, 120);
                
                Debug.Log($"[LevelManager] Fixed ScoreText position: {scoreRect.anchoredPosition}");
            }
        }
        
        // Fix LevelText positioning (bottom-center-right)
        if (levelText != null)
        {
            RectTransform levelRect = levelText.GetComponent<RectTransform>();
            if (levelRect != null)
            {
                // Anchor to bottom-center
                levelRect.anchorMin = new Vector2(0.5f, 0);
                levelRect.anchorMax = new Vector2(0.5f, 0);
                levelRect.pivot = new Vector2(0, 0); // Left-aligned so it sits to the right of center
                
                // Position to the right of center, above timer (120px from bottom)
                levelRect.anchoredPosition = new Vector2(20, 120);
                
                Debug.Log($"[LevelManager] Fixed LevelText position: {levelRect.anchoredPosition}");
            }
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
            levelText.text = "Level: " + (currentIndex + 1).ToString(); // Level: X format
            Debug.Log("[LevelManager] Updated level text to: " + levelText.text);
        }
        
        // Update score text and connect ScoreHandler
        if (scoreText != null)
        {
            if (ScoreHandler.instance != null)
            {
                ScoreHandler.instance.SetScoreText(scoreText);
                scoreText.text = "Score: " + ScoreHandler.instance.GetCurrentScore().ToString(); // Score: X format
                Debug.Log("[LevelManager] Connected ScoreHandler to scoreText, current score: " + ScoreHandler.instance.GetCurrentScore());
            }
            else
            {
                scoreText.text = "Score: 0"; // Score: 0 format
                Debug.Log("[LevelManager] ScoreHandler not found, setting score to Score: 0");
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
    
    private void DebugUIVisibility(TMP_Text textComponent, string name)
    {
        if (textComponent == null)
        {
            Debug.LogError($"[DebugUI] {name} is null!");
            return;
        }
        
        Debug.Log($"[DebugUI] === {name} Visibility Debug ===");
        Debug.Log($"[DebugUI] {name}.text: '{textComponent.text}'");
        Debug.Log($"[DebugUI] {name}.gameObject.activeSelf: {textComponent.gameObject.activeSelf}");
        Debug.Log($"[DebugUI] {name}.gameObject.activeInHierarchy: {textComponent.gameObject.activeInHierarchy}");
        Debug.Log($"[DebugUI] {name}.enabled: {textComponent.enabled}");
        Debug.Log($"[DebugUI] {name}.color: {textComponent.color}");
        Debug.Log($"[DebugUI] {name}.fontSize: {textComponent.fontSize}");
        
        // Check RectTransform
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Debug.Log($"[DebugUI] {name}.anchoredPosition: {rectTransform.anchoredPosition}");
            Debug.Log($"[DebugUI] {name}.sizeDelta: {rectTransform.sizeDelta}");
            Debug.Log($"[DebugUI] {name}.anchorMin: {rectTransform.anchorMin}");
            Debug.Log($"[DebugUI] {name}.anchorMax: {rectTransform.anchorMax}");
        }
        
        // Check Canvas
        Canvas parentCanvas = textComponent.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Debug.Log($"[DebugUI] Parent Canvas: {parentCanvas.name}");
            Debug.Log($"[DebugUI] Canvas.enabled: {parentCanvas.enabled}");
            Debug.Log($"[DebugUI] Canvas.gameObject.activeSelf: {parentCanvas.gameObject.activeSelf}");
            Debug.Log($"[DebugUI] Canvas.renderMode: {parentCanvas.renderMode}");
            Debug.Log($"[DebugUI] Canvas.sortingOrder: {parentCanvas.sortingOrder}");
        }
        else
        {
            Debug.LogError($"[DebugUI] {name} has no parent Canvas!");
        }
        
        Debug.Log($"[DebugUI] === End {name} Debug ===");
    }
    }
}
