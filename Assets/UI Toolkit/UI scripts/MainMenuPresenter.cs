using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace game.ui
{
    public class MainMenuPresenter
    {
        public Action OpenSettings { set => _btnSettings.clicked += value}
        private Button _btnStart, _btnSettings;

        public MainMenuPresenter(VisualElement root) {
            _btnStart = root.Q<Button>("btnStart");
            _btnSettings = root.Q<Button>("btnSettings");
            AddLogstoButtons();
        }

        private void AddLogstoButtons() { 
            _btnStart.clicked += () => Debug.Log("Pressed Start");
            _btnSettings.clicked += () => Debug.Log("Pressed Settings");
        }
    }
}