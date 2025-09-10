using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{
    //This complicated function is used to load a new scene on click event
    public void MoveToScene(int sceneIndex) { 
        SceneManager.LoadScene(sceneIndex);
    }
}
