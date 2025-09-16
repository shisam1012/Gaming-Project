using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TMP_Text scoreText;
    public TMP_Text ScoreText => scoreText;

    [SerializeField] private TMP_Text messageText;
    public TMP_Text MessageText => messageText;

    [SerializeField] private TMP_Text buttonText;
    public TMP_Text ButtonText => buttonText;

    [SerializeField] private Button levelButton;
    public Button LevelButton => levelButton;

    private float betweenLevelDelay = 0.3f;
    
    public void SetUp(int score, string message)
    {
        gameObject.SetActive(true);
        ScoreHandler.instance.ShowScore(false);
        ScoreText.text = "Your score: " + score;
        MessageText.text = message;
         ButtonText.text = "next level";
        LevelButton.onClick.RemoveAllListeners();
        LevelButton.onClick.AddListener(NextLevel);
    }

    public void SetUpTimeOut(int score)
    {
        gameObject.SetActive(true);
        ScoreHandler.instance.ShowScore(false);
        ScoreText.text = "Your score: " + score;
        MessageText.text = "Time's out!";
        ButtonText.text = "try again";
        LevelButton.onClick.RemoveAllListeners();
        LevelButton.onClick.AddListener(RepeatLevel);
    }


    /*public void MainMenuButton()
    { //not working... returns to the same level
        // SceneManager.LoadScene("Main Menu");
        gameObject.SetActive(false);
        mainMenuUI.SetActive(true);
    }*/

    public void RepeatLevel(){
        gameObject.SetActive(false);
        ScoreHandler.instance.SetScore(0);
        ScoreHandler.instance.ShowScore(true);
        levelManager.StartCoroutine(levelManager.RepeatAfterDelay(betweenLevelDelay));
        levelButton.OnDeselect(null); 
        EventSystem.current.SetSelectedGameObject(null);

    }

    public void NextLevel()
    {
        gameObject.SetActive(false);
        ScoreHandler.instance.SetScore(0);
        ScoreHandler.instance.ShowScore(true);
        levelManager.StartCoroutine(levelManager.AdvanceAfterDelay(betweenLevelDelay));
        levelButton.OnDeselect(null);
        EventSystem.current.SetSelectedGameObject(null);
    }
}