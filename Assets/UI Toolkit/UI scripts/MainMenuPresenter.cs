using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.UI_scripts
{
    public class MainMenuPresenter
    {
        private Button _btnStart, _btnSettings;
        public Action OpenSettings { set => _btnSettings.clicked += value; }
        public Action StartGame { set => _btnStart.clicked += value; }
        public MainMenuPresenter(VisualElement root) {
            Debug.Log("[MainMenuPresenter] Creating MainMenuPresenter");
            _btnStart = root.Q<Button>("btnStart");
            _btnSettings = root.Q<Button>("btnSettings");
            
            if (_btnStart == null) {
                Debug.LogError("[MainMenuPresenter] btnStart not found!");
            } else {
                Debug.Log("[MainMenuPresenter] btnStart found successfully");
            }
            
            if (_btnSettings == null) {
                Debug.LogError("[MainMenuPresenter] btnSettings not found!");
            } else {
                Debug.Log("[MainMenuPresenter] btnSettings found successfully");
            }
            
            AddLogstoButtons();
        }

        private void AddLogstoButtons() { 
            _btnStart.clicked += () => Debug.Log("Pressed Start");
            _btnSettings.clicked += () => Debug.Log("Pressed Settings");
        }

    }
}