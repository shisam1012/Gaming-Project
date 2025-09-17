using UnityEngine;
using TMPro;

namespace GamingProject
{
    public class ScoreHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public static ScoreHandler instance;
    public TMP_Text ScoreText => scoreText;
    private int currentScore;

    void Start()
    {
        currentScore = 0;
    }

    private void Awake()
    {
        instance = this;
    }

    public void UpadteScore(int amount)
    {
        if (amount <= 4)
            currentScore += 2 * amount;
        else
            currentScore += 3 * amount;
        scoreText.text = "Score: " + currentScore;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void SetScore(int score)
    {
        currentScore = score;
        scoreText.text = "Score: " + currentScore;
    }

    public int CalculateTimeBonus(float timeRatio)
    {
        float bonus = 1;

        if (timeRatio >= 0.9f)
        {
            bonus = 10f;
        }
        if (timeRatio >= 0.75f)
        {
            bonus = 2.75f;
        }
        else if (timeRatio >= 0.5f)
        {
            bonus = 1.5f;
        }
        else if (timeRatio >= 0.25f)
        {
            bonus = 1.1f;
        }

        return Mathf.RoundToInt((currentScore + 100) * bonus);

    }

    public void ShowScore(bool active){
        scoreText.gameObject.SetActive(active);
    }

}
}
