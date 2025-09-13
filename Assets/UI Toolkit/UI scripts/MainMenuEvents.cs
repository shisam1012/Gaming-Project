using NUnit.Framework.Internal.Filters;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace game.ui
{
    public class MainMenuEvents : MonoBehaviour
    {
        [SerializeField] private UIDocument _mainMenu;
        private VisualElement _rootElem;
        private VisualElement _mainPanel;
        private Button _btnStart, _btnSettings;
        //[SerializeField] private AudioSource _audioSource; // uncomment when there's audio
        private List<Button> _menuButtons;
        private string activeClass = "menu-active";
        private void Awake()
        {
            //_audioSource = GetComponent<AudioSource>();
            _rootElem = _mainMenu.rootVisualElement;

        }
        private void OnEnable()
        {
            _mainPanel = _rootElem.Q(className: "menu");
            _mainPanel.AddToClassList(activeClass);

            _menuButtons = _rootElem.Query<Button>().ToList();
            AddBtnFunctionality();

        }

        private void OnDisable()
        {
            _btnStart.clicked -= OnStartGameClick;
            _btnSettings.clicked -= OnSettingsClick;

            foreach (Button btn in _menuButtons)
            {
                btn.clicked -= OnAllButtonsClick;
            }
        }
        private void AddBtnFunctionality()
        {
            _btnStart.clicked += OnStartGameClick;
            _btnSettings.clicked += OnSettingsClick;
            foreach (Button btn in _menuButtons)
            {
                btn.clicked += OnAllButtonsClick;
            }
        }


        private void OnSettingsClick()
        {

            Debug.Log("Settings yo");

        }

        private void OnAllButtonsClick()
        {
            Debug.Log("meow");
            //_audioSource.Play();
        }
        private void OnStartGameClick()
        {
            SceneManager.LoadScene(sceneBuildIndex: 1);
        }
    }
}