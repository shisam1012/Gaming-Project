using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.UI_scripts
{
    public static class UIExtensions
    {

        // Use this for initialization
        public static void Display(this VisualElement element, bool enabled)
        {
            if (element == null) return;
            element.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}