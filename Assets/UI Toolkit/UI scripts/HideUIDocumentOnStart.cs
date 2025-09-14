using UnityEngine;

public class HideUIDocumentOnStart : MonoBehaviour
{
    public GameObject uiViewer;

    public void StartGame()
    {
        Time.timeScale = 1f;
        if (uiViewer) uiViewer.SetActive(false);
    }
}
