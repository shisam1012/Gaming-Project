using UnityEngine;
using UnityEngine.SceneManagement;

public class GameWinScreen : MonoBehaviour
{
   public void SetUp()
    {
        gameObject.SetActive(true);
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("Main Menu");
    }
}