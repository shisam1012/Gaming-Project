using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    private UIDocument _document;
    private Button _btnStart, _btnSettings, _btnExit;
    //private AudioSource _audioSource; // uncomment when there's audio
    private List<Button> _menuButtons;
    private void Awake()
    {
        //_audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();
    }
    private void OnEnable()
    {
        _btnStart = _document.rootVisualElement.Q<Button>("btnStart");
        _btnStart.clicked += OnStartGameClick;
        _btnSettings = _document.rootVisualElement.Q<Button>("btnSettings");
        _btnSettings.clicked += OnSettingsClick;
        _btnExit = _document.rootVisualElement.Q<Button>("btnExit");
        _btnExit.clicked += OnExitClick;

        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();

        for (int i = 0;  i < _menuButtons.Count; i++)
        {
            _menuButtons[i].clicked += OnAllButtonsClick;
        }
        
    }

    private void OnDisable()
    {
        _btnStart.clicked -= OnStartGameClick;
        _btnSettings.clicked -= OnSettingsClick;
        _btnExit.clicked -= OnExitClick;

        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].clicked -= OnAllButtonsClick;
        }
    }

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
        Debug.Log("Play game lol");
        //gameObject.SetActive(false); // straight to the game
    }
}
