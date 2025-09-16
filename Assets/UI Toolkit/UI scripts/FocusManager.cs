using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.UI_Toolkit.UI_scripts
{
    public class FocusManager : MonoBehaviour
    {
        public void SetGameFocus()
        {
            // Clear focus from UI
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void SetUIFocus(GameObject uiElement)
        {
            // Set focus to the specified UI element
            EventSystem.current.SetSelectedGameObject(uiElement);
        }
    }
}

