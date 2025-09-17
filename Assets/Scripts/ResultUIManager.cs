using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using GamingProject;

public class ResultUIManager : MonoBehaviour
{
    [Header("Auto-create UI if missing")]
    public bool autoCreateUI = true;
    
    private Canvas gameCanvas;
    private GameObject resultScreenObj;
    private ResultScreen resultScreen;
    
    void Start()
    {
        if (autoCreateUI)
        {
            EnsureResultUIExists();
        }
    }
    
    private void EnsureResultUIExists()
    {
        // Check if ResultScreen GameObject exists
        resultScreenObj = GameObject.Find("ResultScreen");
        
        if (resultScreenObj == null)
        {
            Debug.Log("[ResultUIManager] ResultScreen not found, creating UI...");
            CreateResultUI();
        }
        else
        {
            Debug.Log("[ResultUIManager] ResultScreen found");
            resultScreen = resultScreenObj.GetComponent<ResultScreen>();
            
            if (resultScreen != null && (resultScreen.ScoreText == null || resultScreen.MessageText == null))
            {
                Debug.LogWarning("[ResultUIManager] ResultScreen exists but UI components are missing. This might need manual setup.");
            }
        }
    }
    
    private void CreateResultUI()
    {
        // Find or create Canvas
        gameCanvas = FindFirstObjectByType<Canvas>();
        if (gameCanvas == null)
        {
            GameObject canvasObj = new GameObject("Game Canvas");
            gameCanvas = canvasObj.AddComponent<Canvas>();
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[ResultUIManager] Created Game Canvas");
        }
        
        // Create ResultScreen GameObject
        resultScreenObj = new GameObject("ResultScreen");
        resultScreen = resultScreenObj.AddComponent<ResultScreen>();
        
        // Create Result UI Panel
        CreateResultPanel();
        
        Debug.Log("[ResultUIManager] Created complete ResultScreen UI system");
    }
    
    private void CreateResultPanel()
    {
        // Create background panel
        GameObject panelObj = new GameObject("Result Panel");
        panelObj.transform.SetParent(gameCanvas.transform, false);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create Score Text
        GameObject scoreTextObj = new GameObject("Score Text");
        scoreTextObj.transform.SetParent(panelObj.transform, false);
        
        TMP_Text scoreText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "Your score: 0";
        scoreText.fontSize = 32;
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.Center;
        
        RectTransform scoreRect = scoreTextObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 0.6f);
        scoreRect.anchorMax = new Vector2(1, 0.8f);
        scoreRect.offsetMin = Vector2.zero;
        scoreRect.offsetMax = Vector2.zero;
        
        // Create Message Text
        GameObject messageTextObj = new GameObject("Message Text");
        messageTextObj.transform.SetParent(panelObj.transform, false);
        
        TMP_Text messageText = messageTextObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "You Won!";
        messageText.fontSize = 36;
        messageText.color = Color.yellow;
        messageText.alignment = TextAlignmentOptions.Center;
        
        RectTransform messageRect = messageTextObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.4f);
        messageRect.anchorMax = new Vector2(1, 0.6f);
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;
        
        // Create Button
        GameObject buttonObj = new GameObject("Level Button");
        buttonObj.transform.SetParent(panelObj.transform, false);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 1f); // Green
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.1f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.3f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Create Button Text
        GameObject buttonTextObj = new GameObject("Button Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        TMP_Text buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Next Level";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        // Initially hide the panel
        panelObj.SetActive(false);
        
        // Get LevelManager reference
        var levelManager = FindFirstObjectByType<LevelManager>();
        
        // Assign components to ResultScreen via reflection to avoid breaking our working approach
        if (resultScreen != null)
        {
            var scoreField = typeof(ResultScreen).GetField("scoreText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (scoreField != null)
            {
                scoreField.SetValue(resultScreen, scoreText);
            }
            
            var messageField = typeof(ResultScreen).GetField("messageText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (messageField != null)
            {
                messageField.SetValue(resultScreen, messageText);
            }
            
            var buttonField = typeof(ResultScreen).GetField("levelButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (buttonField != null)
            {
                buttonField.SetValue(resultScreen, button);
            }
            
            var buttonTextField = typeof(ResultScreen).GetField("buttonText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (buttonTextField != null)
            {
                buttonTextField.SetValue(resultScreen, buttonText);
            }
            
            var levelManagerField = typeof(ResultScreen).GetField("levelManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (levelManagerField != null)
            {
                levelManagerField.SetValue(resultScreen, levelManager);
            }
            
            // Note: gameObject property is read-only, so we don't need to assign it
            // The resultScreen component is already attached to resultScreenObj
            
            Debug.Log("[ResultUIManager] Assigned all UI components to ResultScreen via reflection");
        }
        
        Debug.Log("[ResultUIManager] Created result screen UI panel");
    }
}