using UnityEngine;
using TMPro;

public class ScoreHandler : MonoBehaviour
{
    [SerializeField] private  TMP_Text scoreText;

    public static ScoreHandler instance;
    public  TMP_Text ScoreText => scoreText;
    private  int currentScore;

    void Start () {
       currentScore = 0;
        //UpadteScore(8);
   
    }

private void Awake(){
    instance = this;
}
    public void UpadteScore(int amount){
        if (amount <= 4)
            currentScore += 2*amount;
        else 
            currentScore += 3*amount;
        scoreText.text = "Score: " + currentScore;
    }

}
