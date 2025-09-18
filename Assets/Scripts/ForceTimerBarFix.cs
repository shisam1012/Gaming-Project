using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamingProject
{
    public class ForceTimerBarFix : MonoBehaviour
    {
        [Header("Timer Bar Settings")]
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private Vector2 barSize = new Vector2(400f, 50f);
        [SerializeField] private Vector2 barPosition = new Vector2(0f, -150f);
        [SerializeField] private Color barColor = Color.green;
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        private Image timerFillImage;
        private GameObject timerBarParent;
        private float testFillAmount = 1f;
        
        void Start()
        {
            if (createOnStart)
            {
                CreateVisibleTimerBar();
                InvokeRepeating(nameof(AnimateTestBar), 1f, 0.1f);
            }
        }
        
        [ContextMenu("Create Visible Timer Bar")]
        public void CreateVisibleTimerBar()
        {
            Debug.Log("[ForceTimerBarFix] Creating highly visible timer bar...");
            
            // Find Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ForceTimerBarFix] No Canvas found!");
                return;
            }
            
            // Remove existing bar if any
            if (timerBarParent != null)
            {
                DestroyImmediate(timerBarParent);
            }
            
            // Create main container
            timerBarParent = new GameObject("ForceTimerBar");
            timerBarParent.transform.SetParent(canvas.transform, false);
            
            RectTransform parentRect = timerBarParent.AddComponent<RectTransform>();
            parentRect.anchorMin = new Vector2(0.5f, 1f); // Top center
            parentRect.anchorMax = new Vector2(0.5f, 1f);
            parentRect.anchoredPosition = barPosition;
            parentRect.sizeDelta = barSize;
            
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(timerBarParent.transform, false);
            
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
            
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = backgroundColor;
            
            // Create fill (timer bar)
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(timerBarParent.transform, false);
            
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            timerFillImage = fill.AddComponent<Image>();
            timerFillImage.color = barColor;
            timerFillImage.type = Image.Type.Filled;
            timerFillImage.fillMethod = Image.FillMethod.Horizontal;
            timerFillImage.fillAmount = 1f;
            
            // Create label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(timerBarParent.transform, false);
            
            RectTransform labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
            labelText.text = "TIMER BAR (TESTING)";
            labelText.fontSize = 24;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
            
            Debug.Log($"[ForceTimerBarFix] Created timer bar at position {barPosition} with size {barSize}");
            
            // Now try to connect it to the existing TimerUI
            ConnectToTimerUI();
        }
        
        private void ConnectToTimerUI()
        {
            TimerUI timerUI = FindFirstObjectByType<TimerUI>();
            if (timerUI != null)
            {
                // Use reflection to set the timerFillImage field
                var field = typeof(TimerUI).GetField("timerFillImage", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(timerUI, timerFillImage);
                    Debug.Log("[ForceTimerBarFix] Connected timer bar to TimerUI component!");
                }
                else
                {
                    Debug.LogWarning("[ForceTimerBarFix] Could not find timerFillImage field in TimerUI");
                }
            }
            else
            {
                Debug.LogWarning("[ForceTimerBarFix] No TimerUI component found");
            }
        }
        
        private void AnimateTestBar()
        {
            if (timerFillImage != null)
            {
                testFillAmount -= 0.01f;
                if (testFillAmount <= 0f)
                {
                    testFillAmount = 1f;
                }
                timerFillImage.fillAmount = testFillAmount;
            }
        }
        
        [ContextMenu("Test Timer Bar Animation")]
        public void TestTimerBarAnimation()
        {
            if (timerFillImage == null)
            {
                CreateVisibleTimerBar();
            }
            
            StartCoroutine(TestAnimation());
        }
        
        private System.Collections.IEnumerator TestAnimation()
        {
            for (float t = 1f; t >= 0f; t -= 0.01f)
            {
                if (timerFillImage != null)
                {
                    timerFillImage.fillAmount = t;
                }
                yield return new WaitForSeconds(0.05f);
            }
            
            // Reset to full
            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = 1f;
            }
        }
        
        public void UpdateTimerBar(float fillAmount)
        {
            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = fillAmount;
            }
        }
    }
}