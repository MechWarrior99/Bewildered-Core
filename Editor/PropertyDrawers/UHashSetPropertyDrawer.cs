using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bewildered.Editor
{
    [CustomPropertyDrawer(typeof(UHashSet<>))]
    internal class UHashSetPropertyDrawer : PropertyDrawer
    {
        private const float _elementRightPadding = 5.0f;

        private SerializedProperty _valuesProperty;
        private SerializedProperty _property;
        private ReorderableList _reorderableList;

        private FieldInfo _saveDuplicatesInfo;
        private bool _isSavingDuplicates = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.IndentedRect(position);

            position = EditorGUITool.FoldoutListHeader(position, _reorderableList, label.text);

            if (!_reorderableList.serializedProperty.isExpanded)
                return;

            // Draws the list.
            _reorderableList.DoList(position);
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_property == null)
                Init(property);

            float height = EditorGUIUtility.singleLineHeight;
            if (_reorderableList.serializedProperty.isExpanded)
                height += _reorderableList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        private void Init(SerializedProperty property)
        {
            _property = property;

            _valuesProperty = _property.FindPropertyRelative("_serializedValues");
            bool isReorderable = fieldInfo.GetCustomAttribute<NonReorderableAttribute>() == null;
            _reorderableList = new ReorderableList(_valuesProperty.serializedObject, _valuesProperty, true, false, true, true);
            _reorderableList.drawElementCallback += DrawElement;
            _reorderableList.elementHeightCallback += GetElementHeight;
            _reorderableList.onAddCallback += rl =>
            {
                _valuesProperty.arraySize++;
                _valuesProperty.GetLastArrayElement().FindPropertyRelative("index").intValue = _valuesProperty.arraySize - 1;
                EnssureSaveDuplicates();
            };
            _reorderableList.drawNoneElementCallback += rect => GUI.Label(rect, "HashSet is Empty");

            _saveDuplicatesInfo = AccessUtility.Field(fieldInfo.FieldType, "_saveDuplicates");
        }

        private float GetElementHeight(int index)
        {
            SerializedProperty valueProperty = _valuesProperty.GetArrayElementAtIndex(index).FindPropertyRelative("value");
            return EditorGUI.GetPropertyHeight(valueProperty, valueProperty.isExpanded);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // For some reason the width will be negative sometimes.
            if (rect.width < 0)
                return;

            rect.y++;
            rect.width -= _elementRightPadding;
            var serializedValueProperty = _valuesProperty.GetArrayElementAtIndex(index);
            var valueProperty = serializedValueProperty.FindPropertyRelative("value");

            // Draws the value field.
            Color previousColor = GUI.color;
            if (serializedValueProperty.FindPropertyRelative("isDuplicate").boolValue)
                GUI.color = new Color(0.9f, 0.4f, 0.4f);

            rect.height = EditorGUI.GetPropertyHeight(valueProperty, valueProperty.isExpanded);
            EditorGUI.BeginProperty(rect, GUIContent.none, valueProperty);
            rect.xMin += 12;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, valueProperty, GUIContent.none, valueProperty.isExpanded);
            if (EditorGUI.EndChangeCheck())
                EnssureSaveDuplicates();
            EditorGUI.EndProperty();

            GUI.color = previousColor;
        }

        /// <summary>
        /// Sets duplicates to be saved if they are not already being saved.
        /// </summary>
        private void EnssureSaveDuplicates()
        {
            if (!_isSavingDuplicates)
            {
                _isSavingDuplicates = true;
                _saveDuplicatesInfo.SetValue(_property.GetValue(), true);
            }
        }
    }
}
