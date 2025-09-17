using UnityEngine;

namespace GamingProject
{
    public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Core Systems")]
    [SerializeField] private Board board;
    [SerializeField] private LevelManager levelManager;
    
    [Header("Controllers")]
    [SerializeField] private TouchInputController inputController;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private Handler handler;
    
    [Header("Game State")]
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private bool isGameActive = true;
    
    // Public properties for easy access
    public Board Board => board;
    public LevelManager LevelManager => levelManager;
    public TouchInputController InputController => inputController;
    public InteractionController InteractionController => interactionController;
    public Handler Handler => handler;
    public StoneManager StoneManager => StoneManager.Instance;
    
    // Game state properties
    public bool IsGamePaused => isGamePaused;
    public bool IsGameActive => isGameActive;
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeManagers();
    }
    
    private void Start()
    {
        FindAndCacheReferences();
        SetupEventListeners();
    }
    
    private void InitializeManagers()
    {
        // Ensure StoneManager exists
        var stoneManager = StoneManager.Instance;
        
        // Initialize other systems if needed
        Debug.Log("[GameManager] Initialized");
    }
    
    private void FindAndCacheReferences()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
        }
        
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }
        
        if (inputController == null)
        {
            inputController = FindFirstObjectByType<TouchInputController>();
        }
        
        if (interactionController == null)
        {
            interactionController = FindFirstObjectByType<InteractionController>();
        }
        
        if (handler == null)
        {
            handler = FindFirstObjectByType<Handler>();
        }
        
        LogSystemStatus();
    }
    
    private void SetupEventListeners()
    {
        if (board != null && board.onWin != null)
        {
            board.onWin.AddListener(OnGameWon);
        }
    }
    
    private void OnGameWon()
    {
        Debug.Log("[GameManager] Game Won!");
        
        // Level advancement is handled by LevelManager, not here
        // This prevents double advancement that was skipping levels
    }
    
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        Debug.Log("[GameManager] Game Paused");
    }
    
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("[GameManager] Game Resumed");
    }
    
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        
        if (inputController != null)
        {
            inputController.enabled = active;
        }
        
        if (interactionController != null)
        {
            interactionController.enabled = active;
        }
    }
    
    public bool CanPlayerInteract()
    {
        return isGameActive && !isGamePaused && board != null && !board.IsBusy;
    }
    
    public void RestartCurrentLevel()
    {
        if (levelManager != null)
        {
            levelManager.RestartCurrentLevel();
        }
        else if (board != null)
        {
            var currentLevel = board.ActiveLevel;
            if (currentLevel != null)
            {
                board.ApplyLevel(currentLevel);
            }
        }
    }
    
    public void LoadLevel(int levelIndex)
    {
        Debug.Log($"[GameManager] LoadLevel({levelIndex}) called");
        if (levelManager != null)
        {
            Debug.Log("[GameManager] LevelManager found, calling LoadLevelPublic");
            levelManager.LoadLevelPublic(levelIndex);
        }
        else
        {
            Debug.LogError("[GameManager] LevelManager is null! Cannot load level.");
        }
    }
    
    // Method for UI start button to begin the game
    public void StartGame()
    {
        Debug.Log("[GameManager] StartGame called - starting first level");
        LoadLevel(0); // Start with level 0 (first level)
    }
    
    public Stone GetStoneAt(int x, int y)
    {
        return board != null ? board.GetStone(x, y) : null;
    }
    
    public bool IsValidPosition(int x, int y)
    {
        return board != null && board.Width > x && board.Height > y && x >= 0 && y >= 0;
    }
    
    public void RemoveStones(System.Collections.Generic.List<Vector2Int> positions)
    {
        if (board != null && CanPlayerInteract())
        {
            board.RemoveStones(positions);
        }
    }
    
    private void LogSystemStatus()
    {
        Debug.Log($"[GameManager] Systems Status:");
        Debug.Log($"  Board: {(board != null ? "Found" : "Missing")}");
        Debug.Log($"  LevelManager: {(levelManager != null ? "Found" : "Missing")}");
        Debug.Log($"  InputController: {(inputController != null ? "Found" : "Missing")}");
        Debug.Log($"  InteractionController: {(interactionController != null ? "Found" : "Missing")}");
        Debug.Log($"  Handler: {(handler != null ? "Found" : "Missing")}");
        Debug.Log($"  StoneManager: {(StoneManager.Instance != null ? "Found" : "Missing")}");
    }
    
    public void LogGameState()
    {
        Debug.Log($"[GameManager] Game State - Active: {isGameActive}, Paused: {isGamePaused}, Can Interact: {CanPlayerInteract()}");
    }
    
    public void SetBoard(Board newBoard)
    {
        board = newBoard;
    }
    
    public void SetLevelManager(LevelManager newLevelManager)
    {
        levelManager = newLevelManager;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        if (board != null && board.onWin != null)
        {
            board.onWin.RemoveListener(OnGameWon);
        }
    }
}
}