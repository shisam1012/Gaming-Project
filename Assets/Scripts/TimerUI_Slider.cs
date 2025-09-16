using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamingProject
{
    public class TimerUI_Slider : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private LevelManager levelManager;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI timeLabel;

    private float totalTime = 60f;
    private bool ticking;

    private void Awake()
    {
        if (!levelManager) levelManager = FindFirstObjectByType<LevelManager>();
    }

    private void OnEnable()
    {
        if (!levelManager) return;
        levelManager.onTimerStart.AddListener(HandleTimerStart);
        levelManager.onTimerEnd.AddListener(HandleTimerEnd);
        levelManager.onTimeUp.AddListener(HandleTimeUp);
    }
    private void OnDisable()
    {
        if (!levelManager) return;
        levelManager.onTimerStart.RemoveListener(HandleTimerStart);
        levelManager.onTimerEnd.RemoveListener(HandleTimerEnd);
        levelManager.onTimeUp.RemoveListener(HandleTimeUp);
    }

    private void Update()
    {
        if (!ticking || levelManager == null || slider == null) return;

        float left = levelManager.TimeLeft;
        slider.value = Mathf.Clamp(left, 0f, totalTime);

        if (timeLabel)
        {
            int secs = Mathf.CeilToInt(left);
            int mm = secs / 60;
            int ss = secs % 60;
            timeLabel.text = $"{mm:00}:{ss:00}";
        }
    }

    public void HandleTimerStart(float initial)
    {
        totalTime = Mathf.Max(1f, initial);
        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = totalTime;
            slider.value = totalTime;
        }
        ticking = true;
        Update();
    }

    public void HandleTimerEnd()
    {
        ticking = false;
        if (slider) slider.value = Mathf.Clamp(levelManager.TimeLeft, 0f, totalTime);
    }

    public void HandleTimeUp()
    {
        ticking = false;
        if (slider) 
        {
            slider.value = 0f;
        }
        
        // Show Game Over panel
        ShowGameOverPanel();
    }
    
    private void ShowGameOverPanel()
    {
        GameObject gameOverPanel = null;
        
        // First, try to find nested GameOverPanel inside GameOverCanvas
        GameObject gameOverCanvas = GameObject.Find("GameOverCanvas");
        if (gameOverCanvas != null)
        {
            Transform panelTransform = gameOverCanvas.transform.Find("GameOverPanel");
            if (panelTransform != null)
            {
                gameOverPanel = panelTransform.gameObject;
            }
        }
        
        // If not found, try other canvas names with nested panels
        if (gameOverPanel == null)
        {
            string[] canvasNames = { "GameOverCanvas", "UI_Canvas", "Canvas", "GameOver_Canvas" };
            string[] panelNames = { "GameOverPanel", "GameOver", "Panel_GameOver", "GameOverUI" };
            
            foreach (string canvasName in canvasNames)
            {
                GameObject canvas = GameObject.Find(canvasName);
                if (canvas != null)
                {
                    foreach (string panelName in panelNames)
                    {
                        Transform panelTransform = canvas.transform.Find(panelName);
                        if (panelTransform != null)
                        {
                            gameOverPanel = panelTransform.gameObject;
                            break;
                        }
                    }
                    if (gameOverPanel != null) break;
                }
            }
        }
        
        // Fallback: Try direct GameObject.Find (for top-level objects)
        if (gameOverPanel == null)
        {
            string[] possibleNames = { "GameOverPanel", "GameOver", "Game Over Panel", "GameOverUI", "Game Over UI", "Panel_GameOver" };
            
            foreach (string name in possibleNames)
            {
                gameOverPanel = GameObject.Find(name);
                if (gameOverPanel != null)
                {
                    break;
                }
            }
        }
        
        // Final fallback: Try to find any GameOverUI component
        if (gameOverPanel == null)
        {
            GameOverUI gameOverUI = FindFirstObjectByType<GameOverUI>();
            if (gameOverUI != null)
            {
                gameOverPanel = gameOverUI.gameObject;
            }
        }
        
        if (gameOverPanel != null)
        {
            // Try to get CanvasGroup and set alpha
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                // If no CanvasGroup, just activate the GameObject
                gameOverPanel.SetActive(true);
            }
            
            // Try to trigger GameOverUI component if it exists
            GameOverUI gameOverUIComponent = gameOverPanel.GetComponent<GameOverUI>();
            if (gameOverUIComponent != null)
            {
                // Call OnTimeUp directly if the event system isn't working
                gameOverUIComponent.OnTimeUp();
            }
            
            // Update any text elements in the panel
            UpdateGameOverText(gameOverPanel);
        }
        else
        {
            Debug.LogError("[TimerUI_Slider] Game Over panel not found! Check that GameOverCanvas/GameOverPanel exists in the scene.");
        }
    }    private void UpdateGameOverText(GameObject gameOverPanel)
    {
        // Find and update common text elements
        var timeUpText = gameOverPanel.transform.Find("TimeUpText");
        if (timeUpText != null)
        {
            var textComponent = timeUpText.GetComponent<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = "TIME'S UP!";
        }
        
        // You can add more text updates here if needed
    }
    }
}
