using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    private UIDocument _document;
    private Button _button;
    //private AudioSource _audioSource; // uncomment when there's audio
    private List<Button> _menuButtons;
    private void Awake()
    {
        //_audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();
        _button = _document.rootVisualElement.Q<Button>("Start");
        _button.RegisterCallback<ClickEvent>(OnPlayGameClick);
        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();

        for (int i = 0;  i < _menuButtons.Count; i++)
        {
            _menuButtons[i].RegisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }

    private void OnAllButtonsClick(ClickEvent evt)
    {
        Debug.Log("meow");
        //_audioSource.Play();
        //throw new NotImplementedException();
    }

    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(OnPlayGameClick);
        for( int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].UnregisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }
    private void OnPlayGameClick(ClickEvent evt)
    {
        Debug.Log("Play game lol");
        //throw new NotImplementedException();
    }
}
