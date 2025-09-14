using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace game.ui
{
    public class InGamePresenter
    {

        private Button _btnPause;
        public Action PauseAction { set => _btnPause.clicked += value; }

        public InGamePresenter(VisualElement root)
        {
            _btnPause = root.Q<Button>("btnPause");
            AddLogstoButtons();
        }

        private void AddLogstoButtons()
        {
            _btnPause.clicked += () => Debug.Log("Pressed Pause");
        }

    }
}
