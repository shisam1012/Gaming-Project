using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GamingProject
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Levels Order (will advance in this order)")]
        public List<LevelConfig> levels = new List<LevelConfig>();
        [SerializeField] private int currentIndex = 0;

        [Header("References")]
        public Board board;
    public UnityEvent onTimeUp;
    public UnityEvent<float> onTimerStart; 
    public UnityEvent onTimerEnd;

    [Header("Timing")]
    [SerializeField] private float betweenLevelDelay = 0.75f;

    private float timeLeft;
    private bool running;

    [SerializeField] private ResultScreen resultScreen;
    public ResultScreen ResultScreen => resultScreen;

    private void Awake()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
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
            
            // Get current score for time-out screen (from main branch)
            int totalScore = ScoreHandler.instance.GetCurrentScore();
            ResultScreen.SetUpTimeOut(totalScore);
            
            // Timer events for UI updates (from feat/timer branch)
            Debug.Log($"[LevelManager] Invoking onTimerEnd and onTimeUp events");
            onTimerEnd?.Invoke();
            onTimeUp?.Invoke();
        }
    }

    // ===== Level flow =====
    private void LoadLevel(int index)
    {
        var cfg = levels[index];
        timeLeft = Mathf.Max(1f, cfg.timeLimitSeconds);
        board.ApplyLevel(cfg);
        running = true;
        
        onTimerStart?.Invoke(timeLeft);
        
        Debug.Log($"[LevelManager] Loaded level {index+1}/{levels.Count}: {cfg.name} (Time: {timeLeft}s)");
    }


    private void OnBoardWin()
    {
        float timeRatio = timeLeft / levels[currentIndex].timeLimitSeconds;
        int totalScore = ScoreHandler.instance.CalculateTimeBonus(timeRatio);

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
        ResultScreen.SetUp(totalScore, "You Won!");
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
