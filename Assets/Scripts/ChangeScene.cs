using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamingProject
{
    public class ChangeScene : MonoBehaviour
    {
    //expand this to NavigationManager, to go from menu to level, from level back to menu
        //This complicated function is used to load a new scene on click event
        public void MoveToScene(int sceneIndex) { 
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
