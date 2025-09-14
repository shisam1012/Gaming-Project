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
        private VisualElement _mainMenuView;
        private VisualElement _settingsView;
        private VisualElement _pauseView, _inGameView;

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

        private void SetSettingsPresenter()
        {
            SettingsPresenter menuPresenter = new(_settingsView)
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
            MainMenuPresenter menuPresenter = new(_mainMenuView)
            {
                OpenSettings = () => ToggleSettings(enable: true, isPaused: false)
            };
        }

        private void SetPausePresenter()
        {
            PausePresenter pausePresenter = new(_pauseView)
            {
                OpenSettings = () => ToggleSettings(enable: true, isPaused: true)
            };
        }

        private void ToggleSettings(bool enable, bool isPaused) {
            _previousViewState = _currentViewState;
            if (enable)
            {
                _currentViewState = ViewState.Settings;
            }
            else if (isPaused)
            {
                _currentViewState = ViewState.Pause;
            }
            else
            {
                _currentViewState = ViewState.MainMenu;
            }
            UpdateViews();
        }

        private void UpdateViews()
        {
            _mainMenuView.Display(_currentViewState == ViewState.MainMenu);
            _settingsView.Display(_currentViewState == ViewState.Settings);
            _pauseView.Display(_currentViewState == ViewState.Pause);
            Debug.Log($"update to view {_currentViewState}");
        }

    }
}