using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamingProject
{
    public class DirectTimerBarCreator : MonoBehaviour
    {
    [Header("Auto-Create Timer Bar")]
    [SerializeField] private bool createOnStart = false; // Changed to false to disable auto creation
    [SerializeField] private bool forceVisible = true;        private Image timerBarFill;
        private TextMeshProUGUI timerText;
        private LevelManager levelManager;
        
        void Start()
        {
            if (createOnStart)
            {
                // Wait a frame for everything to load
                Invoke(nameof(CreateTimerBarDirectly), 0.1f);
            }
        }
        
        [ContextMenu("Create Timer Bar Directly")]
        public void CreateTimerBarDirectly()
        {
            Debug.Log("[DirectTimerBarCreator] Creating timer bar directly...");
            
            // Find the timer text by searching for it
            FindTimerText();
            
            if (timerText == null)
            {
                Debug.LogError("[DirectTimerBarCreator] Could not find timer text!");
                return;
            }
            
            // Find level manager
            levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager == null)
            {
                Debug.LogError("[DirectTimerBarCreator] Could not find LevelManager!");
                return;
            }
            
            CreateVisibleTimerBar();
            
            // Start updating the timer bar
            InvokeRepeating(nameof(UpdateTimerBar), 0.1f, 0.1f);
        }
        
        private void FindTimerText()
        {
            // Search for timer text in multiple ways
            
            // Method 1: Look for TimerUI component
            TimerUI timerUI = FindFirstObjectByType<TimerUI>();
            if (timerUI != null)
            {
                // Use reflection to get the timerText field
                var field = typeof(TimerUI).GetField("timerText", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    timerText = field.GetValue(timerUI) as TextMeshProUGUI;
                    if (timerText != null)
                    {
                        Debug.Log("[DirectTimerBarCreator] Found timer text via TimerUI component");
                        return;
                    }
                }
            }
            
            // Method 2: Search all TextMeshProUGUI components for one that looks like a timer
            var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var text in allTexts)
            {
                if (text.text.Contains(":") && (text.text.Contains("00") || text.text.Length <= 6))
                {
                    timerText = text;
                    Debug.Log($"[DirectTimerBarCreator] Found timer text by content: '{text.text}'");
                    return;
                }
            }
            
            // Method 3: Look for specific names
            foreach (var text in allTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("timer") || name.Contains("time") || name.Contains("clock"))
                {
                    timerText = text;
                    Debug.Log($"[DirectTimerBarCreator] Found timer text by name: '{text.name}'");
                    return;
                }
            }
            
            Debug.LogWarning("[DirectTimerBarCreator] Could not find timer text automatically");
        }
        
        private void CreateVisibleTimerBar()
        {
            if (timerText == null) return;
            
            // Get the timer text's container
            Transform container = timerText.transform.parent ?? timerText.transform;
            RectTransform textRect = timerText.rectTransform;
            
            Debug.Log($"[DirectTimerBarCreator] Creating timer bar in container: {container.name}");
            Debug.Log($"[DirectTimerBarCreator] Timer text position: {textRect.anchoredPosition}");
            Debug.Log($"[DirectTimerBarCreator] Timer text size: {textRect.sizeDelta}");
            
            // Create timer bar container
            GameObject timerBarContainer = new GameObject("DirectTimerBar");
            timerBarContainer.transform.SetParent(container, false);
            
            RectTransform containerRect = timerBarContainer.AddComponent<RectTransform>();
            
            // Position relative to timer text
            containerRect.anchorMin = textRect.anchorMin;
            containerRect.anchorMax = textRect.anchorMax;
            containerRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textRect.anchoredPosition.y - 30f);
            
            // Size
            float width = Mathf.Max(textRect.sizeDelta.x, 200f);
            if (width <= 0) width = 200f;
            containerRect.sizeDelta = new Vector2(width, 20f);
            
            // Create background
            Image background = timerBarContainer.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Almost black background
            
            // Create fill bar
            GameObject fillBar = new GameObject("FillBar");
            fillBar.transform.SetParent(timerBarContainer.transform, false);
            
            RectTransform fillRect = fillBar.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            timerBarFill = fillBar.AddComponent<Image>();
            timerBarFill.color = Color.yellow; // Bright yellow for maximum visibility
            timerBarFill.type = Image.Type.Filled;
            timerBarFill.fillMethod = Image.FillMethod.Horizontal;
            timerBarFill.fillAmount = 1f;
            
            Debug.Log($"[DirectTimerBarCreator] Timer bar created at {containerRect.anchoredPosition} with size {containerRect.sizeDelta}");
            
            // Force the objects to be active
            timerBarContainer.SetActive(true);
            fillBar.SetActive(true);
            
            // Set high sorting order to make sure it's visible
            Canvas parentCanvas = container.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Canvas barCanvas = timerBarContainer.AddComponent<Canvas>();
                barCanvas.overrideSorting = true;
                barCanvas.sortingOrder = 100; // High sorting order
                
                GraphicRaycaster raycaster = timerBarContainer.AddComponent<GraphicRaycaster>();
            }
        }
        
        private void UpdateTimerBar()
        {
            if (timerBarFill == null || levelManager == null) return;
            
            float timeLeft = levelManager.TimeLeft;
            float totalTime = levelManager.GetCurrentLevelTimeLimit();
            
            if (totalTime > 0)
            {
                float fillAmount = timeLeft / totalTime;
                timerBarFill.fillAmount = fillAmount;
                
                // Change color based on time remaining
                if (fillAmount > 0.5f)
                    timerBarFill.color = Color.green;
                else if (fillAmount > 0.25f)
                    timerBarFill.color = Color.yellow;
                else
                    timerBarFill.color = Color.red;
            }
        }
        
        [ContextMenu("Force Timer Bar Visible")]
        public void ForceTimerBarVisible()
        {
            if (timerBarFill != null)
            {
                timerBarFill.gameObject.SetActive(true);
                timerBarFill.transform.parent.gameObject.SetActive(true);
                timerBarFill.color = Color.magenta; // Very visible color
                timerBarFill.fillAmount = 0.75f;
                
                Debug.Log("[DirectTimerBarCreator] Forced timer bar visible with magenta color");
            }
        }
        
        [ContextMenu("Debug Timer Setup")]
        public void DebugTimerSetup()
        {
            Debug.Log("=== Timer Setup Debug ===");
            
            // Find all text components
            var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            Debug.Log($"Found {allTexts.Length} text components:");
            
            foreach (var text in allTexts)
            {
                Debug.Log($"Text: '{text.text}' on object '{text.name}' at position {text.rectTransform.anchoredPosition}");
            }
            
            // Find TimerUI
            TimerUI timerUI = FindFirstObjectByType<TimerUI>();
            if (timerUI != null)
            {
                Debug.Log($"Found TimerUI on object: {timerUI.name}");
            }
            else
            {
                Debug.Log("No TimerUI component found");
            }
            
            // Find LevelManager
            LevelManager lm = FindFirstObjectByType<LevelManager>();
            if (lm != null)
            {
                Debug.Log($"Found LevelManager with time left: {lm.TimeLeft}");
            }
            else
            {
                Debug.Log("No LevelManager found");
            }
        }
    }
}