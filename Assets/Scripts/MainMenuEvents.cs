using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    [SerializeField] private UIDocument _document;
    private VisualElement _rootElem;
    //private VisualElement _mainPanel;
    private Button _btnStart, _btnSettings, _btnExit;
    //private AudioSource _audioSource; // uncomment when there's audio
    private List<Button> _menuButtons;
    private void Awake()
    {
        //_audioSource = GetComponent<AudioSource>();
        _rootElem = _document.rootVisualElement;
    }
    private void OnEnable()
    {
        //_btnStart = _rootElem.Q<Button>("btnStart");
        //_btnSettings = _rootElem.Q<Button>("btnSettings");
        //_btnExit = _rootElem.Q<Button>("btnExit");
        
        _menuButtons = _rootElem.Query<Button>().ToList();
        _btnStart = _menuButtons[0];
        _btnSettings = _menuButtons[1];
        _btnExit = _menuButtons[2];
        AddBtnFunctionality();
        //_mainPanel = _rootElem.Q<VisualElement>("panel");
        //_mainPanel.RemoveFromClassList("show");

        //// Automatically show the panel after a delay (for demonstration)
        //Invoke(nameof(ShowPanel), 1f); // Change the delay as needed
    }

    private void OnDisable()
    {
        _btnStart.clicked -= OnStartGameClick;
        _btnSettings.clicked -= OnSettingsClick;
        _btnExit.clicked -= OnExitClick;

        foreach (Button btn in _menuButtons)
        {
            btn.clicked -= OnAllButtonsClick;
        }
    }
    private void AddBtnFunctionality()
    {
        _btnStart.clicked += OnStartGameClick;
        _btnSettings.clicked += OnSettingsClick;
        _btnExit.clicked += OnExitClick;
        foreach (Button btn in _menuButtons)
        {
            btn.clicked += OnAllButtonsClick;
        }
    }
    
    //private void ShowPanel()
    //{
    //    _mainPanel.AddToClassList("show");
    //}
    private void OnExitClick()
    {
        //throw new NotImplementedException();
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    private void OnSettingsClick()
    {
        //throw new NotImplementedException();
        Debug.Log("Settings yo");

    }

    private void OnAllButtonsClick()
    {
        Debug.Log("meow");
        //_audioSource.Play();
        //throw new NotImplementedException();
    }
    private void OnStartGameClick()
    {
        //Debug.Log("Play game lol");
        //gameObject.SetActive(false); // straight to the game
        SceneManager.LoadScene(sceneBuildIndex:1);
    }
}
