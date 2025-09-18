using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace GamingProject
{
    public class EndScreenUI : MonoBehaviour
    {
        private enum EndScreenType
        {
            GameOver,
            LevelComplete
        }

        [Header("UI References")]
        [SerializeField] private GameObject endScreenPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI levelReachedText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button actionButton;
        [SerializeField] private Button quitButton;
        //[SerializeField] private LevelManager levelManager;
        
        [Header("UI Text Settings")]
        [SerializeField] private string gameOverMessage = "TIME'S UP!";
        [SerializeField] private string levelCompleteMessage = "WELL DONE!";
        [SerializeField] private string levelFormat = "Level Reached: {0}";
        [SerializeField] private string nextLevelFormat = "Next Level: {0}";
        [SerializeField] private string scoreFormat = "Score: {0}";
        [SerializeField] private string tryAgainText = "Try Again?";
        [SerializeField] private string nextLevelText = "Next Level";

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private CanvasGroup canvasGroup;

        private LevelManager levelManager;
        private bool isShowing = false;
        private EndScreenType currentScreenType = EndScreenType.GameOver;
        
        private float betweenLevelDelay = 0.3f;
    
        private void Awake()
        {
            if (endScreenPanel != null)
            {
                endScreenPanel.SetActive(false);
            }
            else
            {
                Debug.LogError("[EndScreenUI] endScreenPanel is NULL!");
            }

            levelManager = FindFirstObjectByType<LevelManager>();

            // Setup canvas group
            if (canvasGroup == null && endScreenPanel != null)
            {
                canvasGroup = endScreenPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = endScreenPanel.AddComponent<CanvasGroup>();
                }
            }

            // Button listeners
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(OnActionButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuit);
            }

            HideEndScreen();
        }

        private void OnDestroy()
        {
            if (actionButton != null)
                actionButton.onClick.RemoveListener(OnActionButtonClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuit);
        }

        public void ShowEndScreen(bool isLevelComplete)
        {
            if (isShowing) return;

            isShowing = true;
            currentScreenType = isLevelComplete ? EndScreenType.LevelComplete : EndScreenType.GameOver;

            UpdateEndScreenText();

            if (endScreenPanel != null)
            {
                endScreenPanel.SetActive(true);
            }

            StartCoroutine(FadeIn());

            Time.timeScale = 0f;
        }

        public void HideEndScreen()
        {
            isShowing = false;

            if (endScreenPanel != null)
                endScreenPanel.SetActive(false);

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            Time.timeScale = 1f;
        }

        private void UpdateEndScreenText()
        {
            // Title
            if (titleText != null)
            {
                titleText.text = currentScreenType == EndScreenType.LevelComplete ? levelCompleteMessage : gameOverMessage;
            }

            // Level
            if (levelReachedText != null && levelManager != null)
            {
                int level = levelManager.GetCurrentLevelNumber();
                if (currentScreenType == EndScreenType.LevelComplete)
                {
                    // Show "Next Level" text and number
                    levelReachedText.text = string.Format(nextLevelFormat, level + 1);
                }
                else // Game over
                {
                    // Show "Level Reached" text and number
                    levelReachedText.text = string.Format(levelFormat, level);
                }
            }

            // Score
            if (scoreText != null)
            {
                int score = GetCurrentScore();
                scoreText.text = string.Format(scoreFormat, score);
            }

            // Button text
            if (actionButton != null)
            {
                var buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = currentScreenType == EndScreenType.LevelComplete ? nextLevelText : tryAgainText;
                }
            }
        }

        private int GetCurrentScore()
        {
            if (ScoreHandler.instance != null)
            {
                return ScoreHandler.instance.GetCurrentScore();
            }

            var scoreObj = GameObject.Find("ScoreHandler");
            if (scoreObj != null)
            {
                var scoreHandler = scoreObj.GetComponent<MonoBehaviour>();
                var method = scoreHandler?.GetType().GetMethod("GetCurrentScore");
                if (method != null)
                {
                    return (int)method.Invoke(scoreHandler, null);
                }
            }

            return 0;
        }

        private System.Collections.IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private void OnActionButtonClicked()
        {
            HideEndScreen();

            if (currentScreenType == EndScreenType.LevelComplete)
            {
                GoToNextLevel();
            }
            else
            {
                RestartGame();
            }
        }

        private void GoToNextLevel()
        {
            Debug.Log("[EndScreenUI] Going to next level");
            if (levelManager != null)
            {
                levelManager.StartCoroutine(levelManager.AdvanceAfterDelay(betweenLevelDelay));
                //levelManager.AdvanceToNextLevel();

            }
            else
            {
                Debug.LogError("[EndScreenUI] LevelManager not found");
            }
        }

        private void RestartGame()
        {
            Debug.Log("[EndScreenUI] Restarting game");

            if (ScoreHandler.instance != null)
            {
                ScoreHandler.instance.SetScore(0);
            }

            if (levelManager != null)
            {
                levelManager.ResetToFirstLevel();
            }
            else
            {
                Debug.LogWarning("[EndScreenUI] LevelManager not found");
            }
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Manual trigger
        public void ShowGameOver() => ShowEndScreen(false);
        public void ShowVictory() => ShowEndScreen(true);
        public bool IsShowing => isShowing;
    }
}
