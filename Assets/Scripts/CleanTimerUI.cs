using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamingProject
{
    public class CleanTimerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillImage; // Optional - only if you want a timer bar
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
                Debug.LogWarning("[CleanTimerUI] LevelManager not found!");
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
            // Just check if components are assigned - don't create anything
            if (timerText == null)
            {
                Debug.LogWarning("[CleanTimerUI] Timer text not assigned!");
            }
            
            if (timerFillImage == null)
            {
                Debug.Log("[CleanTimerUI] Timer fill image not assigned - timer will only show text");
            }
            
            if (timerPanel != null)
            {
                timerPanel.SetActive(true);
            }
        }
        
        private void UpdateTimerDisplay()
        {
            float timeLeft = levelManager.TimeLeft;
            
            if (totalTime <= 0)
            {
                totalTime = timeLeft;
            }
            
            UpdateTimerText(timeLeft);
            
            // Only update fill if it exists
            if (timerFillImage != null)
            {
                UpdateTimerFill(timeLeft);
                UpdateTimerColor(timeLeft);
            }
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
            if (timerFillImage == null) return;
            
            Color targetColor = normalColor;
            
            if (timeLeft <= dangerThreshold)
            {
                targetColor = dangerColor;
            }
            else if (timeLeft <= warningThreshold)
            {
                targetColor = warningColor;
            }
            
            timerFillImage.color = targetColor;
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
        }
        
        public void SetTimerActive(bool active)
        {
            isActive = active;
            
            if (timerPanel != null)
            {
                timerPanel.SetActive(active);
            }
        }
        
        [ContextMenu("Debug Timer UI")]
        public void DebugTimerUI()
        {
            Debug.Log($"[CleanTimerUI] Timer Text: {(timerText != null ? "Found" : "Missing")}");
            Debug.Log($"[CleanTimerUI] Timer Fill: {(timerFillImage != null ? "Found" : "Missing")}");
            Debug.Log($"[CleanTimerUI] Timer Panel: {(timerPanel != null ? "Found" : "Missing")}");
            Debug.Log($"[CleanTimerUI] Is Active: {isActive}");
        }
    }
}