using Assets.UI_Toolkit.UI_scripts;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace game.ui
{
    public class UIViewPresenter : MonoBehaviour
    {
        [SerializeField] private UIDocument _baseView;
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
            SetInGamePresenter();
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
            _inGameView = _rootElem.Q("InGameView");
            Debug.Log("setVisualElements");
        }
        private void SetInGamePresenter()
        {
            InGamePresenter inGamePresenter = new(_inGameView)
            {
                PauseAction = () => ToggleView(ViewState.Pause)
            };
        }

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
            MainMenuPresenter mainMenuPresenter = new(_mainMenuView)
            {
                StartGame = () => ToggleView(ViewState.InGame),
                OpenSettings = () => ToggleView(ViewState.Settings)
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

        private void ToggleView(ViewState newState) {
            Debug.Log($"Toggle from {_currentViewState} to {newState}");
            _previousViewState = _currentViewState;
            _currentViewState = newState;
            UpdateViews();
        }

        private void UpdateViews()
        {
            _mainMenuView.Display(_currentViewState == ViewState.MainMenu);
            _settingsView.Display(_currentViewState == ViewState.Settings);
            _pauseView.Display(_currentViewState == ViewState.Pause);
            _inGameView.Display(_currentViewState == ViewState.InGame);
            Debug.Log($"update to view {_currentViewState}");
        }

    }
}