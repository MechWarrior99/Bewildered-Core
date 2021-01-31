using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bewildered.Editor
{
    internal class DictionaryList
    {
        private ReorderableList _list;
        private SerializedProperty _dictionaryProperty;
        private float _warningWidth = 15;

        public SerializedProperty DictionaryProperty
        {
            get { return _dictionaryProperty; }
            set
            {
                _dictionaryProperty = value;
                KeysProperty = _dictionaryProperty.FindPropertyRelative("_keysList");
                ValuesProperty = _dictionaryProperty.FindPropertyRelative("_valuesList");
            }
        }

        public SerializedProperty KeysProperty
        {
            get;
            private set;
        }

        public SerializedProperty ValuesProperty
        {
            get;
            private set;
        }

        public DictionaryList(SerializedProperty dictionaryProperty)
        {
            DictionaryProperty = dictionaryProperty;
            _list = new ReorderableList(dictionaryProperty.serializedObject, KeysProperty);
            _list.drawElementCallback = OnDrawElement;
            _list.elementHeightCallback = ElementHeight;
            _list.onAddCallback = OnAdd;
            _list.onRemoveCallback = OnRemove;
        }

        public void Draw(Rect rect)
        {
            _list.DoList(rect);
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocus)
        {
            SerializedProperty keyProperty = KeysProperty.GetArrayElementAtIndex(index);
            SerializedProperty valueProperty = ValuesProperty.GetArrayElementAtIndex(index);

            Rect warningRect = new Rect(rect);
            warningRect.width = _warningWidth;

            Rect keyRect = new Rect(rect);
            keyRect.x += _warningWidth;
            keyRect.width = rect.width / 3;
            keyRect.height = EditorGUI.GetPropertyHeight(keyProperty);

            Rect valueRect = new Rect(rect);
            valueRect.x += keyRect.width + 5 + _warningWidth;
            valueRect.width = (keyRect.width * 2) - 5 - _warningWidth;
            valueRect.height = EditorGUI.GetPropertyHeight(valueProperty);

            float previusLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 30;
            EditorGUI.PropertyField(keyRect, keyProperty, new GUIContent("Key"), true);
            EditorGUIUtility.labelWidth = 40;
            EditorGUI.PropertyField(valueRect, valueProperty, new GUIContent("Value"), true);
            EditorGUIUtility.labelWidth = previusLabelWidth;

            if (DoesCollide(keyProperty))
            {
                GUI.Label(warningRect, "W");
            }
        }

        public float GetHeight()
        {
            return _list.GetHeight();
        }

        private float ElementHeight(int index)
        {
            float keyElementHeight = EditorGUI.GetPropertyHeight(KeysProperty.GetArrayElementAtIndex(index), GUIContent.none, true);
            float valueElementHeight = EditorGUI.GetPropertyHeight(ValuesProperty.GetArrayElementAtIndex(index), GUIContent.none, true);
            return Mathf.Max(EditorGUIUtility.singleLineHeight, keyElementHeight, valueElementHeight) + EditorGUIUtility.standardVerticalSpacing;
        }

        private void OnAdd(ReorderableList list)
        {
            ValuesProperty.arraySize++;
            KeysProperty.arraySize++;
        }

        private void OnRemove(ReorderableList list)
        {
            ValuesProperty.ClearAndDeleteArrayElementAtIndex(list.index);
            KeysProperty.ClearAndDeleteArrayElementAtIndex(list.index);
        }

        private bool DoesCollide(SerializedProperty keyProperty)
        {
            int matchCount = 0;
            for (int i = 0; i < KeysProperty.arraySize; i++)
            {
                if (SerializedProperty.DataEquals(KeysProperty.GetArrayElementAtIndex(i), keyProperty))
                {
                    matchCount++;
                    if (matchCount > 1)
                        return true;
                }
            }

            return false;
        }
    } 
}
