using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace game.ui
{
    public class PausePresenter
    {
        private Button _btnResume, _btnSettings;
        public Action OpenSettings { set => _btnSettings.clicked += value; }

        public PausePresenter(VisualElement root)
        {
            _btnResume = root.Q<Button>("btnResume");
            _btnSettings = root.Q<Button>("btnSettings");
            AddLogstoButtons();
        }

        private void AddLogstoButtons()
        {
            _btnResume.clicked += () => Debug.Log("Pressed Resume");
            _btnSettings.clicked += () => Debug.Log("Pressed Settings");
        }

    }
}