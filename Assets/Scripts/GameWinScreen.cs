using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameWinScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private LevelManager levelManager;
    public TMP_Text ScoreText => scoreText;

private float betweenLevelDelay = 0.3f;
    public void SetUp(int score)
    {
        gameObject.SetActive(true);
        ScoreText.text = "Your score: " + score;
    }

    /*public void MainMenuButton()
    { //not working... returns to the same level
        // SceneManager.LoadScene("Main Menu");
        gameObject.SetActive(false);
        mainMenuUI.SetActive(true);
    }*/
    public void NextLevelButton()
    {
        gameObject.SetActive(false);
        ScoreHandler.instance.SetScore(0);
        levelManager.StartCoroutine(levelManager.AdvanceAfterDelay(betweenLevelDelay));
    }
}