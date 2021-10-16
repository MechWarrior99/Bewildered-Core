using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    [CustomPropertyDrawer(typeof(TimeValue))]
    internal class TimeValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty timeProperty = property.FindPropertyRelative("time");

            // We create the rect for the main label to be inline with the field input areas.
            Rect prefixLabelRect = new Rect()
            {
                x = position.x,
                y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                width = position.width,
                height = EditorGUIUtility.singleLineHeight
            };

            EditorGUI.BeginProperty(position, label, property);

            // The label on the left of the control. Uses a ControlID so the label can be select like the Vector3 field.
            prefixLabelRect = EditorGUI.PrefixLabel(prefixLabelRect, GUIUtility.GetControlID(-1, FocusType.Keyboard, position), label);

            float fieldWidth = (prefixLabelRect.width - 4 - 8) / 3;

            position.width = fieldWidth;
            position.x = prefixLabelRect.x;

            EditorGUI.BeginChangeCheck();
            int previousMinutes = (int)(timeProperty.floatValue / 60);
            int newMinutes = EditorGUITool.IntFieldLabelAbove(position, new GUIContent("Minutes"), previousMinutes);
            if (EditorGUI.EndChangeCheck())
                timeProperty.floatValue += (newMinutes - previousMinutes) * 60.0f;

            position.x += fieldWidth + 4;

            EditorGUI.BeginChangeCheck();
            int previousSeconds = (int)(timeProperty.floatValue % 60);
            float newSeconds = EditorGUITool.IntFieldLabelAbove(position, new GUIContent("Seconds"), previousSeconds);
            if (EditorGUI.EndChangeCheck())
                timeProperty.floatValue += newSeconds - previousSeconds;

            // Move the position over and give it extra space for better readability in the inspector.
            position.x += fieldWidth + 8;

            EditorGUI.BeginChangeCheck();
            float timeResult = EditorGUITool.FloatFieldLabelAbove(position, new GUIContent("Total"), timeProperty.floatValue);
            if (EditorGUI.EndChangeCheck())
                timeProperty.floatValue = timeResult;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }
    }
}
