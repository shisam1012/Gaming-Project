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