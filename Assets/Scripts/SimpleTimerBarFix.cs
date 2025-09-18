using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamingProject
{
    /// <summary>
    /// Simple script to create a timer bar that definitely works.
    /// Just add this to ANY object in your scene and it will create a visible timer bar.
    /// </summary>
    public class SimpleTimerBarFix : MonoBehaviour
    {
    [Header("Settings")]
    [SerializeField] private bool autoStart = false; // Changed to false to disable auto creation
    [SerializeField] private Vector2 position = new Vector2(0, -200);
    [SerializeField] private Vector2 size = new Vector2(400, 30);
    [SerializeField] private Color barColor = Color.green;        private Image timerBar;
        private LevelManager levelManager;
        
        void Start()
        {
            if (autoStart)
            {
                CreateSimpleTimerBar();
            }
        }
        
        [ContextMenu("Create Simple Timer Bar")]
        public void CreateSimpleTimerBar()
        {
            Debug.Log("[SimpleTimerBarFix] Creating simple timer bar...");
            
            // Find canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SimpleTimerBarFix] No canvas found!");
                return;
            }
            
            // Find level manager
            levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager == null)
            {
                Debug.LogError("[SimpleTimerBarFix] No LevelManager found!");
                return;
            }
            
            // Create timer bar container
            GameObject timerBarObj = new GameObject("SimpleTimerBar");
            timerBarObj.transform.SetParent(canvas.transform, false);
            
            // Set position and size
            RectTransform rect = timerBarObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f); // Top center
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            
            // Add background
            Image background = timerBarObj.AddComponent<Image>();
            background.color = Color.black;
            
            // Create fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(timerBarObj.transform, false);
            
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            timerBar = fillObj.AddComponent<Image>();
            timerBar.color = barColor;
            timerBar.type = Image.Type.Filled;
            timerBar.fillMethod = Image.FillMethod.Horizontal;
            timerBar.fillAmount = 1f;
            
            // Start updating
            InvokeRepeating(nameof(UpdateBar), 0.1f, 0.1f);
            
            Debug.Log($"[SimpleTimerBarFix] Timer bar created at {position}");
        }
        
        private void UpdateBar()
        {
            if (timerBar == null || levelManager == null) return;
            
            float timeLeft = levelManager.TimeLeft;
            float totalTime = levelManager.GetCurrentLevelTimeLimit();
            
            if (totalTime > 0)
            {
                timerBar.fillAmount = timeLeft / totalTime;
                
                // Change color based on time
                float ratio = timeLeft / totalTime;
                if (ratio > 0.5f)
                    timerBar.color = Color.green;
                else if (ratio > 0.25f)
                    timerBar.color = Color.yellow;
                else
                    timerBar.color = Color.red;
            }
        }
    }
}