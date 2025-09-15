using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [Header("Levels Order (will advance in this order)")]
    public List<LevelConfig> levels = new List<LevelConfig>();
    [SerializeField] private int currentIndex = 0;

    [Header("References")]
    public Board board;
    public UnityEvent onTimeUp;

    [Header("Timing")]
    [SerializeField] private float betweenLevelDelay = 0.75f;

    private float timeLeft;
    private bool running;

    [SerializeField] private GameWinScreen gameWinScreen;
    public GameWinScreen GameWinScreen => gameWinScreen;

    private void Awake()
    {
        if (board == null)
        {
            board = FindObjectOfType<Board>();
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
        LoadLevel(currentIndex);
    }

    private void Update()
    {
        if (!running) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            running = false;
            Debug.LogWarning("[LevelManager] Time up!");
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
        Debug.Log($"[LevelManager] Loaded level {index+1}/{levels.Count}: {cfg.name}");
    }


    private void OnBoardWin(int finalScore)
    {
        float timeRatio = timeLeft / levels[currentIndex].timeLimitSeconds;
        float bonus = 1;

        if (timeRatio >= 0.75f)
        {
            bonus = 1.75f;
        }
        else if (timeRatio >= 0.5f)
        {
            bonus = 1.5f;
        }
        else if (timeRatio >= 0.25f)
        {
            bonus = 1.1f;
        }

        int totalScore = Mathf.RoundToInt(finalScore * bonus);
        ScoreHandler.instance.SetScore(totalScore);
        Debug.LogWarning("------total score" + totalScore);
        //GameWinScreen.instance.SetUp(totalScore);
        GameWinScreen.SetUp(totalScore);
        running = false;
        //StartCoroutine(AdvanceAfterDelay(betweenLevelDelay));
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

    public float TimeLeft => Mathf.Max(0f, timeLeft);
}
