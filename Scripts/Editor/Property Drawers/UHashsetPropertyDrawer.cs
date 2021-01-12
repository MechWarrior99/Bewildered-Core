using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Core.Editor
{
    [CustomPropertyDrawer(typeof(UHashset<>), true)]
    internal class UHashsetPropertyDrawer : PropertyDrawer
    {
        private ReorderablePropertyList _list;
        private FieldInfo _doSerializeToHashsetField;
        private object _targetInstanceValue;
        private bool _isSerializingToHashsete = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            _list.Draw(position);
            if (EditorGUI.EndChangeCheck() && _isSerializingToHashsete)
            {
                _doSerializeToHashsetField.SetValue(_targetInstanceValue, false);
                _isSerializingToHashsete = false;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_list == null)
            {
                Init(property, label);
            }
            
            return _list.GetHeight();
        }

        private void Init(SerializedProperty property, GUIContent label)
        {
            _targetInstanceValue = property.GetValue();
            _doSerializeToHashsetField = fieldInfo.FieldType.BaseType.GetField("_doSerializeToHashset", BindingFlags.NonPublic | BindingFlags.Instance);
            
            _list = new ReorderablePropertyList(property.FindPropertyRelative("_hashsetList"), label.text, true, true, true, true);
            _list.OnAdd += (list) => {
                list.Property.AddAndGetArrayElement().ResetValueToDefault();
                list.Property.serializedObject.ApplyModifiedProperties();
            };
            _list.DrawElementGUI = (rect, elementProperty) =>
            {
                rect.height = EditorGUI.GetPropertyHeight(elementProperty, GUIContent.none, true);
                rect.y += 1;
                EditorGUI.BeginProperty(rect, GUIContent.none, elementProperty);
                EditorGUI.PropertyField(rect, elementProperty);
                EditorGUI.EndProperty();
            };
        }
    } 
}
