using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Bewildered.Editor
{
    internal class AdvancedPopupWindowContainer : EditorWindow
    {

    }

    /// <summary>
    /// Class used to implement a popup window using <see cref="VisualElement"/>s.
    /// </summary>
    public abstract class AdvancedPopupWindow
    {
        public EditorWindow EditorWindow
        {
            get;
            internal set;
        }

        /// <summary>
        /// Color used for the border of the window.
        /// </summary>
        protected static Color BorderColor
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f, 1.333f) : new Color(0.6f, 0.6f, 0.6f, 1.333f); }
        }

        public abstract VisualElement CreateContentElement();

        public virtual void Show(Rect activeRect)
        {
            EditorWindow = ScriptableObject.CreateInstance<AdvancedPopupWindowContainer>();
            EditorWindow.rootVisualElement.Add(CreateContentElement());
            EditorWindow.rootVisualElement.style.borderTopColor = BorderColor;
            EditorWindow.rootVisualElement.style.borderRightColor = BorderColor;
            EditorWindow.rootVisualElement.style.borderBottomColor = BorderColor;
            EditorWindow.rootVisualElement.style.borderLeftColor = BorderColor;
            EditorWindow.rootVisualElement.style.borderTopWidth = 1;
            EditorWindow.rootVisualElement.style.borderRightWidth = 1;
            EditorWindow.rootVisualElement.style.borderBottomWidth = 1;
            EditorWindow.rootVisualElement.style.borderLeftWidth = 1;
            EditorWindow.ShowAsDropDown(GUIUtility.GUIToScreenRect(activeRect), GetWindowSize());
        }

        public void ShowDebugable(Rect activeRect)
        {
            EditorWindow = ScriptableObject.CreateInstance<AdvancedPopupWindowContainer>();
            EditorWindow.rootVisualElement.Add(CreateContentElement());
            EditorWindow.Show();
        }

        public virtual Vector2 GetWindowSize()
        {
            return new Vector2(200.0f, 200.0f);
        }
    }
}