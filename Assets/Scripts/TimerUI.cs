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
        // Debug UI component assignments
        Debug.Log($"[TimerUI] Setting up UI - Text: {(timerText != null ? "Found" : "Missing")}, Fill: {(timerFillImage != null ? "Found" : "Missing")}, Panel: {(timerPanel != null ? "Found" : "Missing")}");
        
        if (timerText == null)
        {
            Debug.LogWarning("[TimerUI] Timer text not assigned!");
            // Try to find it automatically
            timerText = GetComponentInChildren<TextMeshProUGUI>();
            if (timerText != null)
            {
                Debug.Log("[TimerUI] Auto-found timer text component");
            }
        }
        
        if (timerFillImage == null)
        {
            Debug.LogWarning("[TimerUI] Timer fill image not assigned! Please assign it in the inspector.");
        }
        
        // Check screen resolution and canvas scaling
        Debug.Log($"[TimerUI] Screen Resolution: {Screen.width}x{Screen.height}, DPI: {Screen.dpi}");
        
        // Ensure UI elements are active and visible
        if (timerFillImage != null)
        {
            timerFillImage.gameObject.SetActive(true);
            Debug.Log($"[TimerUI] Timer fill image - Active: {timerFillImage.gameObject.activeInHierarchy}, Alpha: {timerFillImage.color.a}");
        }
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }
    }
    
    private void CreateTimerFillImage()
    {
        Debug.Log("[TimerUI] Creating timer fill image automatically...");
        
        if (timerText == null)
        {
            Debug.LogError("[TimerUI] Cannot create timer bar - timer text is null!");
            return;
        }
        
        // Get the timer text's parent (should be the container)
        Transform textParent = timerText.transform.parent;
        if (textParent == null)
        {
            textParent = timerText.transform;
        }
        
        // Create timer bar as a sibling of the timer text (same parent)
        GameObject timerBarObj = new GameObject("TimerBar");
        timerBarObj.transform.SetParent(textParent, false);
        
        // Add RectTransform and set it up
        RectTransform barRect = timerBarObj.AddComponent<RectTransform>();
        
        // Position it directly below the timer text
        RectTransform textRect = timerText.rectTransform;
        
        // Copy the timer text's positioning but offset downward
        barRect.anchorMin = textRect.anchorMin;
        barRect.anchorMax = textRect.anchorMax;
        barRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textRect.anchoredPosition.y - 30f);
        
        // Make it same width as text, but specific height
        barRect.sizeDelta = new Vector2(textRect.sizeDelta.x > 0 ? textRect.sizeDelta.x : 200f, 10f);
        
        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(timerBarObj.transform, false);
        RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark background
        
        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(timerBarObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        timerFillImage = fillObj.AddComponent<Image>();
        timerFillImage.color = Color.green; // Green for visibility
        timerFillImage.type = Image.Type.Filled;
        timerFillImage.fillMethod = Image.FillMethod.Horizontal;
        timerFillImage.fillAmount = 1f;
        
        Debug.Log($"[TimerUI] Created timer fill image at position {barRect.anchoredPosition} relative to text at {textRect.anchoredPosition}");
    }
    
    [ContextMenu("EMERGENCY: Create Big Visible Timer Bar")]
    public void EmergencyCreateTimerBar()
    {
        Debug.Log("[TimerUI] EMERGENCY: Creating big visible timer bar...");
        
        if (timerText == null)
        {
            Debug.LogError("[TimerUI] Cannot create emergency timer bar - timer text is null!");
            return;
        }
        
        // Get the timer text's exact parent and position
        Transform textParent = timerText.transform.parent;
        RectTransform textRect = timerText.rectTransform;
        
        // Create emergency bar in the SAME container as the timer text
        GameObject emergencyBar = new GameObject("EMERGENCY_TIMER_BAR");
        emergencyBar.transform.SetParent(textParent, false);
        
        RectTransform barRect = emergencyBar.AddComponent<RectTransform>();
        
        // Position it EXACTLY below the timer text, same anchoring
        barRect.anchorMin = textRect.anchorMin;
        barRect.anchorMax = textRect.anchorMax;
        barRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textRect.anchoredPosition.y - 25f);
        
        // Make it same width as text container but taller for visibility
        float barWidth = textRect.sizeDelta.x > 0 ? textRect.sizeDelta.x : 300f;
        barRect.sizeDelta = new Vector2(barWidth, 15f);
        
        // Background
        Image background = emergencyBar.AddComponent<Image>();
        background.color = Color.black;
        
        // Fill bar
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(emergencyBar.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        timerFillImage = fillObj.AddComponent<Image>();
        timerFillImage.color = Color.red; // Very visible red
        timerFillImage.type = Image.Type.Filled;
        timerFillImage.fillMethod = Image.FillMethod.Horizontal;
        timerFillImage.fillAmount = 0.8f; // Set to 80% for testing
        
        Debug.Log($"[TimerUI] EMERGENCY timer bar created directly below text at {barRect.anchoredPosition}!");
    }

    [ContextMenu("Make Timer Bar Bigger")]
    public void MakeTimerBarBigger()
    {
        if (timerFillImage != null)
        {
            RectTransform barRect = timerFillImage.transform.parent?.GetComponent<RectTransform>();
            if (barRect != null)
            {
                // Make it bigger and more visible
                barRect.sizeDelta = new Vector2(300f, 40f);
                
                // Change color to be more visible
                timerFillImage.color = Color.green;
                
                // Position it more prominently
                barRect.anchoredPosition = new Vector2(0, -80f);
                
                Debug.Log("[TimerUI] Made timer bar bigger and more visible");
            }
        }
        else
        {
            Debug.LogWarning("[TimerUI] No timer fill image found to resize");
        }
    }

    [ContextMenu("Force Create Timer Bar")]
    public void ForceCreateTimerBar()
    {
        Debug.Log("[TimerUI] Force creating timer bar...");
        
        // Remove existing timer fill if any
        if (timerFillImage != null)
        {
            if (timerFillImage.transform.parent != null && timerFillImage.transform.parent.name == "TimerBar")
            {
                DestroyImmediate(timerFillImage.transform.parent.gameObject);
            }
            timerFillImage = null;
        }
        
        CreateTimerFillImage();
        
        // Test the timer bar
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = 0.5f; // Set to half for testing
            Debug.Log("[TimerUI] Timer bar created and set to 50% for testing");
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
        if (timerFillImage == null) 
        {
            Debug.LogWarning("[TimerUI] Timer fill image is null in UpdateTimerFill!");
            return;
        }
        
        float fillAmount = totalTime > 0 ? timeLeft / totalTime : 0;
        timerFillImage.fillAmount = fillAmount;
        
        // Debug on mobile/tablet platforms
        #if UNITY_ANDROID || UNITY_IOS
        if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
        {
            Debug.Log($"[TimerUI] Fill amount: {fillAmount}, Image active: {timerFillImage.gameObject.activeInHierarchy}, Enabled: {timerFillImage.enabled}, Alpha: {timerFillImage.color.a}");
        }
        #endif
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
    
    [ContextMenu("Force Timer Fill Visible")]
    public void ForceTimerFillVisible()
    {
        if (timerFillImage != null)
        {
            // Ensure the image is active and visible
            timerFillImage.gameObject.SetActive(true);
            timerFillImage.enabled = true;
            
            // Set to full visibility
            Color color = timerFillImage.color;
            color.a = 1f;
            timerFillImage.color = color;
            
            // Set to half fill for testing
            timerFillImage.fillAmount = 0.5f;
            
            Debug.Log($"[TimerUI] Forced timer fill visible - Active: {timerFillImage.gameObject.activeInHierarchy}, Enabled: {timerFillImage.enabled}, Alpha: {timerFillImage.color.a}, Fill: {timerFillImage.fillAmount}");
        }
        else
        {
            Debug.LogError("[TimerUI] Timer fill image is null! Please assign it in the inspector.");
        }
    }
    
    [ContextMenu("Debug Timer UI State")]
    public void DebugTimerUIState()
    {
        Debug.Log($"[TimerUI] === Timer UI Debug Info ===");
        Debug.Log($"Screen: {Screen.width}x{Screen.height}, DPI: {Screen.dpi}");
        Debug.Log($"Timer Text: {(timerText != null ? "Assigned" : "Missing")}");
        Debug.Log($"Timer Fill: {(timerFillImage != null ? "Assigned" : "Missing")}");
        Debug.Log($"Timer Panel: {(timerPanel != null ? "Assigned" : "Missing")}");
        
        if (timerFillImage != null)
        {
            Debug.Log($"Fill Image - Active: {timerFillImage.gameObject.activeInHierarchy}, Enabled: {timerFillImage.enabled}, Alpha: {timerFillImage.color.a}, Fill: {timerFillImage.fillAmount}");
            Debug.Log($"Fill Image Transform: {timerFillImage.transform.position}, Size: {timerFillImage.rectTransform.sizeDelta}");
        }
    }

    [ContextMenu("Setup UI For Tablet")]
    public void SetupUIForTablet()
    {
        Debug.Log("[TimerUI] Setting up UI for tablet compatibility...");
        
        // Check screen dimensions
        float screenRatio = (float)Screen.width / Screen.height;
        bool isTablet = screenRatio > 1.5f || Screen.dpi < 200;
        
        Debug.Log($"[TimerUI] Screen: {Screen.width}x{Screen.height}, Ratio: {screenRatio:F2}, DPI: {Screen.dpi}, Detected as: {(isTablet ? "Tablet" : "Phone/Desktop")}");
        
        if (timerFillImage != null)
        {
            // Ensure the fill image is properly configured
            RectTransform fillRect = timerFillImage.rectTransform;
            
            // Make sure it's not too small on tablets
            if (isTablet && fillRect.sizeDelta.magnitude < 10f)
            {
                fillRect.sizeDelta = new Vector2(200f, 20f); // Minimum size for tablets
                Debug.Log("[TimerUI] Adjusted fill image size for tablet");
            }
            
            // Ensure proper anchoring
            fillRect.anchorMin = new Vector2(0, 0.5f);
            fillRect.anchorMax = new Vector2(1, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            
            // Make sure it's active and visible
            timerFillImage.gameObject.SetActive(true);
            timerFillImage.enabled = true;
            
            // Ensure proper color/alpha
            Color color = timerFillImage.color;
            if (color.a < 0.1f)
            {
                color.a = 1f;
                timerFillImage.color = color;
                Debug.Log("[TimerUI] Fixed fill image alpha for visibility");
            }
            
            // Set test fill amount
            timerFillImage.fillAmount = 0.75f;
            
            Debug.Log($"[TimerUI] Fill image setup complete - Size: {fillRect.sizeDelta}, Active: {timerFillImage.gameObject.activeInHierarchy}, Alpha: {timerFillImage.color.a}");
        }
        else
        {
            Debug.LogError("[TimerUI] Timer fill image is null! Please assign it in the inspector.");
        }
        
        // Also setup the panel if available
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
            Debug.Log("[TimerUI] Timer panel activated");
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