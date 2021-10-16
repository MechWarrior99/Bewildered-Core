using System;
using System.Reflection;
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

        private static readonly int floatFieldHash = "EditorTextField".GetHashCode();
        private static readonly string _floatFieldFormatString = "g7";
        private static readonly string _intFieldFormatString = "#######0";
        private static FieldInfo _dragStartValueInfo;
        private static MethodInfo _doNumberFieldInfo;
        private static object _recycledTextEditor;

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

        /// <summary>
        /// Draws a float field with the label above it.
        /// </summary>
        public static float FloatFieldLabelAbove(Rect position, GUIContent label, float value)
        {
            int id = GUIUtility.GetControlID(floatFieldHash, FocusType.Keyboard, position);

            Rect labelRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight
            };
            position.y += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
            position.height -= labelRect.height + EditorGUIUtility.standardVerticalSpacing;

            // We use the HandlePrefixLabel because it is not affected by indentation where the normal PrefixLabel is.
            EditorGUI.HandlePrefixLabel(position, labelRect, label, id);
            return DoFloatField(position, labelRect, id, value, EditorStyles.numberField);
        }

        /// <summary>
        /// Draws a float field with the label above it.
        /// </summary>
        public static int IntFieldLabelAbove(Rect position, GUIContent label, int value)
        {
            int id = GUIUtility.GetControlID(floatFieldHash, FocusType.Keyboard, position);

            Rect labelRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight
            };
            position.y += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
            position.height -= labelRect.height + EditorGUIUtility.standardVerticalSpacing;

            // We use the HandlePrefixLabel because it is not affected by indentation where the normal PrefixLabel is.
            EditorGUI.HandlePrefixLabel(position, labelRect, label, id);
            return DoIntField(position, labelRect, id, value, EditorStyles.numberField);
        }

        private static float DoFloatField(Rect position, Rect dragHotZone, int id, float value, GUIStyle style)
        {
            if (_dragStartValueInfo == null)
                _dragStartValueInfo = AccessUtility.Field(typeof(EditorGUI), "s_DragStartValue");

            long l = 0L;
            double doubleValue = value;
            double startDragValue = (double)_dragStartValueInfo.GetValue(null);
            float dragValue = Event.current.GetTypeForControl(id) == EventType.MouseDown ? (float)CalculateFloatDragSensitivity(startDragValue) : 0;

            DoNumberField(position, dragHotZone, id, true, ref doubleValue, ref l, _floatFieldFormatString, style, true, dragValue);
            return (float)doubleValue;
        }

        private static int DoIntField(Rect position, Rect dragHotZone, int id, int value, GUIStyle style)
        {
            double d = 0;
            long longValue = value;

            DoNumberField(position, dragHotZone, id, false, ref d, ref longValue, _intFieldFormatString, style, true, CalculateIntDragSensitivity(value));
            return (int)longValue;
        }

        private static void DoNumberField(Rect position, Rect dragHotZone, int id, bool isDouble, ref double doubleVal, ref long longVal, string formatString, GUIStyle style, bool draggable, float dragSensitivity)
        {
            if (_doNumberFieldInfo == null)
            {
                _doNumberFieldInfo = AccessUtility.Method(typeof(EditorGUI), "DoNumberField");
                _recycledTextEditor = AccessUtility.Field(typeof(EditorGUI), "s_RecycledEditor").GetValue(null);
            }

            object[] args = new object[] { _recycledTextEditor, position, dragHotZone, id, isDouble, doubleVal, longVal, formatString, style, draggable, dragSensitivity };
            _doNumberFieldInfo.Invoke(null, args);
            doubleVal = (double)args[5];
            longVal = (long)args[6];
        }

        private static double CalculateFloatDragSensitivity(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                 return 0.0;
            }
            else
            {
                return Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5)) * 0.029999999329447746;
            }
        }

        private static long CalculateIntDragSensitivity(long value)
        {
            return (long)Math.Max(1.0, Math.Pow(Math.Abs((double)value), 0.5) * 0.029999999329447746);
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
