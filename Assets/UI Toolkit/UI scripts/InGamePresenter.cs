using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.UI_scripts
{
    public class InGamePresenter
    {
        //private Button _btnPause;
        //private VisualElement _uiBar, _gameArea;
        //public Action PauseAction { set => _btnPause.clicked += value; }

        //public InGamePresenter(VisualElement root)
        //{
        //    _btnPause = root.Q<Button>("btnPause");
        //    _gameArea = root.Q("gameArea");
        //    _uiBar = root.Q("uiBar");
        //    //_gameArea.pickingMode = PickingMode.Ignore;

        //    AddLogstoButtons();
        //}

        //private void AddLogstoButtons()
        //{
        //    _btnPause.clicked += () => Debug.Log("Pressed Pause");
        //}
        private GameObject _view;
        public InGamePresenter(GameObject view)
        {
            _view = view;
            _view.SetActive(false);
        }
}
}
