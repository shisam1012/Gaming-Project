using UnityEngine;

/// <summary>
/// GameManager keeps references to key systems and provides centralized access.
/// Other systems can access the board and other managers through this central hub.
/// </summary>
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
        // Find board if not assigned
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
        }
        
        // Find level manager if not assigned
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }
        
        // Find controllers if not assigned
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
        // Subscribe to board events if available
        if (board != null && board.onWin != null)
        {
            board.onWin.AddListener(OnGameWon);
        }
    }
    
    private void OnGameWon()
    {
        Debug.Log("[GameManager] Game Won!");
        
        // Handle win logic
        if (levelManager != null)
        {
            levelManager.AdvanceToNextLevel();
        }
    }
    
    // Game state management
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
        
        // Enable/disable input based on game state
        if (inputController != null)
        {
            inputController.enabled = active;
        }
        
        if (interactionController != null)
        {
            interactionController.enabled = active;
        }
    }
    
    // Utility methods for other systems
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
            // Fallback: regenerate board
            var currentLevel = board.ActiveLevel;
            if (currentLevel != null)
            {
                board.ApplyLevel(currentLevel);
            }
        }
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelManager != null)
        {
            levelManager.LoadLevelPublic(levelIndex);
        }
    }
    
    // Board access methods
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
    
    // Debug and utility
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
    
    // Manual assignment methods for inspector
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
        
        // Unsubscribe from events
        if (board != null && board.onWin != null)
        {
            board.onWin.RemoveListener(OnGameWon);
        }
    }
}