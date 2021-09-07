using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bewildered.Editor
{
    public static class EditorGUITool
    {
        private const float foldoutHeaderHeight = 18.0f;
        private const float headerPadding = 3.0f;
        private const float arraySizeFieldWidth = 48.0f;
        private const float minHeaderHeight = 2.0f;

        public static float FoldoutListHeaderHeight = foldoutHeaderHeight + headerPadding;

        /// <summary>
        /// Draws a header for a <see cref="ReorderableList"/> with a foldout like the default list drawer in the inspector.
        /// Sets the isExpanded property of the <see cref="ReorderableList"/>.
        /// </summary>
        /// <param name="rect">The position and width of the header to draw.</param>
        /// <param name="list">The targeted <see cref="ReorderableList"/>. Must be using a <see cref="SerializedProperty"/> for it's items.</param>
        /// <param name="content">The text to display in the header.</param>
        /// <returns>A <see cref="Rect"/> positioned under the header.</returns>
        public static Rect FoldoutListHeader(Rect rect, ReorderableList list, string content)
        {
            SerializedProperty property = list.serializedProperty;
            if (property == null)
                return rect;

            list.headerHeight = minHeaderHeight;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, foldoutHeaderHeight);
            Rect arraySizeRect = new Rect(headerRect.xMax - arraySizeFieldWidth, headerRect.y, arraySizeFieldWidth, foldoutHeaderHeight);

            Event evt = Event.current;

            EventType prevEventType = Event.current.type;
            if (evt.type == EventType.MouseUp && arraySizeRect.Contains(evt.mousePosition))
                Event.current.type = EventType.Used;

            EditorGUI.BeginProperty(headerRect, GUIContent.none, property.FindPropertyRelative("Array.size"));
            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, property.isExpanded, content);
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();

            if (evt.type == EventType.Used && arraySizeRect.Contains(evt.mousePosition))
                Event.current.type = prevEventType;

            EditorGUI.PropertyField(arraySizeRect, property.FindPropertyRelative("Array.size"), GUIContent.none);
            // Used to give the field a tooltip as they only show when hovering over labels and not fields.
            EditorGUI.LabelField(arraySizeRect, new GUIContent("", "Array size."));

            rect.y += foldoutHeaderHeight + headerPadding;
            rect.height -= foldoutHeaderHeight + headerPadding;

            return rect;
        }

        private static class Styles
        {
            public static readonly GUIStyle HeaderStyle;

            static Styles()
            {
                // The normal foldoutHeader has a background image that is the same color as the default window color.
                // However if the background behind the list is different, this other color shows up (like a nested list).
                // Wo we use the foldout style background instead for the normal state.
                HeaderStyle = new GUIStyle(EditorStyles.foldoutHeader);
                HeaderStyle.normal = new GUIStyle(EditorStyles.foldout).normal;
                HeaderStyle.fontStyle = FontStyle.Bold;
            }
        }
    }
}
