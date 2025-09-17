using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Assets.UI_Toolkit.UI_scripts
{
    public class UIViewPresenter : MonoBehaviour
    {
        [SerializeField] private UIDocument _baseView;
        [SerializeField] private GameObject _view;
        private VisualElement _rootElem;
        private VisualElement _mainMenuView, _settingsView, _pauseView, _inGameView;

        private enum ViewState
        {
            MainMenu,
            Settings,
            Pause,
            InGame
        }
        private ViewState _currentViewState;
        private ViewState _previousViewState;

        private void OnEnable()
        {
            _rootElem = _baseView.rootVisualElement;
            SetVisualElements();
            SetMainMenuPresenter();
            //SetInGamePresenter();
            SetPausePresenter();
            SetSettingsPresenter();
            UpdateViews();
        }

        private void SetVisualElements()
        {
            _currentViewState = ViewState.MainMenu;
            _previousViewState = ViewState.MainMenu;
            _mainMenuView = _rootElem.Q("MainMenuView");
            _settingsView = _rootElem.Q("SettingsView");
            _pauseView = _rootElem.Q("PauseView");
            //_inGameView = _rootElem.Q("InGameView");
            Debug.Log("setVisualElements");
        }
        //private void SetInGamePresenter()
        //{
        //    InGamePresenter inGamePresenter = new(_view);
        //}

        private void SetSettingsPresenter()
        {
            SettingsPresenter settingsPresenter = new(_settingsView)
            {
                BackAction = () =>
                {
                    _currentViewState = _previousViewState;
                    UpdateViews();
                }
            };
        }

        private void SetMainMenuPresenter()
        {
            Debug.Log("[UIViewPresenter] Setting up MainMenuPresenter");
            MainMenuPresenter mainMenuPresenter = new(_mainMenuView)
            {
                StartGame = () => {
                    Debug.Log("[UIViewPresenter] StartGame action triggered!");
                    Debug.Log("[UIViewPresenter] Hiding main menu and starting game...");
                    
                    // Hide the UI like the old version did
                    ToggleView(ViewState.InGame);
                    
                    // Start the game through GameManager like before
                    if (GamingProject.GameManager.Instance != null) 
                    {
                        GamingProject.GameManager.Instance.StartGame();
                        Debug.Log("[UIViewPresenter] Game started through GameManager");
                    }
                    else
                    {
                        Debug.LogError("[UIViewPresenter] GameManager instance not found!");
                    }
                },
                OpenSettings = () => {
                    Debug.Log("[UIViewPresenter] Settings button clicked");
                    ToggleView(ViewState.Settings);
                }
            };
        }

        private void SetPausePresenter()
        {
            PausePresenter pausePresenter = new(_pauseView)
            {
                ResumeGame = () => ToggleView(ViewState.InGame),
                OpenSettings = () => ToggleView(ViewState.Settings)
            };
        }

        private void ToggleView(ViewState newState)
        {
            Debug.Log($"Toggle from {_currentViewState} to {newState}");
            _previousViewState = _currentViewState;
            _currentViewState = newState;
            UpdateViews();
        }

        private void UpdateViews()
        {
            if (_currentViewState == ViewState.InGame)
            {
                if (_view != null)
                    _view.SetActive(false);
            }
            _mainMenuView.Display(_currentViewState == ViewState.MainMenu);
            _settingsView.Display(_currentViewState == ViewState.Settings);
            _pauseView.Display(_currentViewState == ViewState.Pause);
            //_inGameView.Display(_currentViewState == ViewState.InGame);
            //Debug.Log($"update to view {_currentViewState}");
        }

    }
}
