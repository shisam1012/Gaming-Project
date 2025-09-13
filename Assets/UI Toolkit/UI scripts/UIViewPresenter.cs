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
        private VisualElement _pauseView;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rootElem = _baseView.rootVisualElement;
            _mainMenuView = _rootElem.Q("MainMenuView");
            _settingsView = _rootElem.Q("SettingsView");
            _pauseView = _rootElem.Q("PauseView");

            MainMenuPresenter MMenuPresenter = new MainMenuPresenter();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}