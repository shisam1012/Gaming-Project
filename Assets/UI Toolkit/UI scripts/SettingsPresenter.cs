using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.UI_scripts
{
    public class SettingsPresenter
    {
        private Button _btnBack;
        public Action BackAction { set => _btnBack.clicked += value; }

        public SettingsPresenter(VisualElement root)
        {
            _btnBack = root.Q<Button>("btnBack");
            AddLogstoButtons();
        }

        private void AddLogstoButtons()
        {
            _btnBack.clicked += () => Debug.Log("Pressed Resume");
        }

    }
}