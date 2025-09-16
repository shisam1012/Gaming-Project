using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamingProject
{
    public class TimerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private GameObject timerPanel;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float warningThreshold = 30f;
    [SerializeField] private float dangerThreshold = 10f;
    
    private LevelManager levelManager;
    private float totalTime;
    private bool isActive = false;
    
    private void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogWarning("[TimerUI] LevelManager not found!");
            return;
        }
        
        SetupUI();
    }
    
    private void Update()
    {
        if (!isActive || levelManager == null) return;
        
        UpdateTimerDisplay();
    }
    
    private void SetupUI()
    {
        if (timerText == null)
        {
            Debug.LogWarning("[TimerUI] Timer text not assigned!");
        }
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }
        
        isActive = true;
    }
    
    private void UpdateTimerDisplay()
    {
        float timeLeft = levelManager.TimeLeft;
        
        if (totalTime <= 0)
        {
            totalTime = timeLeft;
        }
        
        UpdateTimerText(timeLeft);
        UpdateTimerFill(timeLeft);
        UpdateTimerColor(timeLeft);
    }
    
    private void UpdateTimerText(float timeLeft)
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
    
    private void UpdateTimerFill(float timeLeft)
    {
        if (timerFillImage == null) return;
        
        float fillAmount = totalTime > 0 ? timeLeft / totalTime : 0;
        timerFillImage.fillAmount = fillAmount;
    }
    
    private void UpdateTimerColor(float timeLeft)
    {
        Color targetColor = normalColor;
        
        if (timeLeft <= dangerThreshold)
        {
            targetColor = dangerColor;
        }
        else if (timeLeft <= warningThreshold)
        {
            targetColor = warningColor;
        }
        
        if (timerText != null)
        {
            timerText.color = targetColor;
        }
        
        if (timerFillImage != null)
        {
            timerFillImage.color = targetColor;
        }
    }
    
    public void OnLevelStart(float levelTime)
    {
        totalTime = levelTime;
        isActive = true;
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }
    }
    
    public void OnLevelEnd()
    {
        isActive = false;
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
    
    public void SetTimerActive(bool active)
    {
        isActive = active;
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(active);
        }
    }
}
}