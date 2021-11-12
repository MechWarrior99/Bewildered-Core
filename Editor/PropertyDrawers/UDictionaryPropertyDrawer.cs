using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bewildered.Editor
{
    [CustomPropertyDrawer(typeof(UDictionary<,>))]
    internal class UDictionaryPropertyDrawer : PropertyDrawer
    {
        private static readonly float _defaultSplitPercent = 0.3f;
        private const float _elementRightPadding = 5.0f;

        private SerializedProperty _pairsProperty;
        private SerializedProperty _property;
        private ReorderableList _reorderableList;

        private FieldInfo _saveDuplicatesInfo;
        private bool _isSavingDuplicates = false;

        private bool _isKeySerializable = false;        // Unity can serialize this type
        private bool _isKeyEquatable = false;           // The type implements Equals and GetHashCode
        private bool _isKeyUnityObjectDerived = false;  // The type is derived from UnityEngine.Object
        private bool _isValueSerializable = false;      // Unity can serialize this type


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.IndentedRect(position);

            position = EditorGUITool.FoldoutListHeader(position, _reorderableList, label.text);

            if (!_reorderableList.serializedProperty.isExpanded)
                return;

            // Draws the key/value columns header.
            Rect typesContainerRect = position;
            typesContainerRect.height = EditorGUIUtility.singleLineHeight;

            Rect backgroundRect = typesContainerRect;
            if (Event.current.type == EventType.Repaint)
            {
                // Draws the top border.
                Styles.HeaderStyle.Draw(backgroundRect, GUIContent.none, 0);
                backgroundRect.y += 2;
                // Draws the main background area.
                Styles.BackgroundStyle.Draw(backgroundRect, GUIContent.none, 0);
            }

            Rect keyRect = SplitRect(typesContainerRect, true);
            Rect valueRect = SplitRect(typesContainerRect, false);

            keyRect.x += 30;
            GUI.Label(keyRect, "Keys");
            valueRect.x += 20;
            GUI.Label(valueRect, "Values");

            position.y += EditorGUIUtility.singleLineHeight;

            // Draws the list.
            _reorderableList.DoList(position);

            // Draws the vertical split line.
            backgroundRect = SplitRect(position, false);
            backgroundRect.x += 7;
            backgroundRect.y -= EditorGUIUtility.singleLineHeight;
            backgroundRect.height -= EditorGUIUtility.singleLineHeight;
            backgroundRect.width = 1;
            EditorGUI.DrawRect(backgroundRect, new Color(0.2f, 0.2f, 0.2f));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_property == null)
                Init(property);

            float height = EditorGUIUtility.singleLineHeight;
            if (_reorderableList.serializedProperty.isExpanded)
                height += _reorderableList.GetHeight()  + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        private void Init(SerializedProperty property)
        {
            EvaluateKeyAndValueTypes(property);

            _property = property;
            
            _pairsProperty = _property.FindPropertyRelative("_serializedPairs");
            bool isReorderable = fieldInfo.GetCustomAttribute<NonReorderableAttribute>() == null;
            _reorderableList = new ReorderableList(_pairsProperty.serializedObject, _pairsProperty, true, false, true, true);
            _reorderableList.drawElementCallback += DrawElement;
            _reorderableList.elementHeightCallback += GetElementHeight;
            _reorderableList.onAddCallback += rl =>
            {
                if (CheckKeyAndValueTypesAreValid())
                {
                    _pairsProperty.arraySize++;
                    _pairsProperty.GetLastArrayElement().FindPropertyRelative("index").intValue = _pairsProperty.arraySize - 1;
                    EnssureSaveDuplicates();
                }
            };
            _reorderableList.drawNoneElementCallback += rect => 
            {
                if(_isKeyUnityObjectDerived)
                {
                    GUI.Label(rect, new GUIContent(
                        "The Key type cannot be a child of UnityEngine.Object!",
                        "The Key cannot be serialized as a reference."));
                }
                else if (!_isKeySerializable)
                {
                    GUI.Label(rect, new GUIContent(
                        "The Key type is not Serializable!",
                        "Add the [Serializable] attribute to the type definition."));
                }
                else if (!_isValueSerializable)
                {
                    GUI.Label(rect, new GUIContent(
                        "The Value type is not Serializable!",
                        "Add the [Serializable] attribute to the type definition."));
                }
                else if(!_isKeyEquatable)
                {
                    GUI.Label(rect, new GUIContent(
                        "The Key type is not Equatable!", 
                        "Make the Key a struct or override the functions Equals and GetHashCode"));
                }
                else
                {
                    GUI.Label(rect, new GUIContent(
                        "Dictionary is Empty.",
                        "Use the + button to add values."));
                }
            };

            _saveDuplicatesInfo = AccessUtility.Field(fieldInfo.FieldType, "_saveDuplicates");
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }

        private bool CheckKeyAndValueTypesAreValid()
        {
            return _isKeySerializable && _isKeyEquatable && !_isKeyUnityObjectDerived && _isValueSerializable;
        }

        private void EvaluateKeyAndValueTypes(SerializedProperty property)
        {
            _isKeySerializable = false;
            _isKeyEquatable = false;
            _isKeyUnityObjectDerived = false;
            _isValueSerializable = false;

            // TODO: Is there a better way to check if key and value are serializable without instantianting an object?
            _property = property;
            _pairsProperty = _property.FindPropertyRelative("_serializedPairs");

            _pairsProperty.arraySize++;
            var temporaryObj = _pairsProperty.GetLastArrayElement();
            SerializedProperty keyProperty = temporaryObj.FindPropertyRelative("key");
            SerializedProperty valueProperty = temporaryObj.FindPropertyRelative("value");

            // If the properties are not null, it means Unity can serialize its' types
            _isKeySerializable = keyProperty != null;
            _isValueSerializable = valueProperty != null;

            if (!_isKeySerializable)
            {
                _pairsProperty.arraySize--;
                return;
            }

            Type keyType = keyProperty.GetPropertyFieldType();
            _pairsProperty.arraySize--;

            // If it's a primitce type, an enum or a struct type, it is Equatable
            if (keyType.IsValueType)
            {
                _isKeyEquatable = true;
                return;
            }

            // We don't accept class types that Unity serializes as reference and not as value (i.e. the key can be null)
            if (keyType.IsSubclassOf(typeof(UnityEngine.Object)) || keyType == typeof(UnityEngine.Object))
            {
                _isKeyUnityObjectDerived = true;
                return;
            }

            // We don't accept types that are not defined as Equatable (i.e. overriding Equals and GetHashCode)
            MethodInfo infoEquals = keyType.GetMethod("Equals",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.Any,
                new Type[] { typeof(object) },
                null);

            MethodInfo infoGetHashCode = keyType.GetMethod("GetHashCode",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.Any,
                Type.EmptyTypes,
                null);

            bool hasEquals = infoEquals.DeclaringType == keyType;
            bool hasGetHashCode = infoGetHashCode.DeclaringType == keyType;

            _isKeyEquatable = hasEquals && hasGetHashCode;
        }


        private float GetElementHeight(int index)
        {
            SerializedProperty pairProperty = _pairsProperty.GetArrayElementAtIndex(index);

            SerializedProperty keyProperty = pairProperty.FindPropertyRelative("key");
            SerializedProperty valueProperty = pairProperty.FindPropertyRelative("value");

            float keyHeight = EditorGUI.GetPropertyHeight(keyProperty, keyProperty.isExpanded);
            float valueHeight = EditorGUI.GetPropertyHeight(valueProperty, valueProperty.isExpanded);

            return Mathf.Max(keyHeight, valueHeight);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // For some reason the width will be negative sometimes.
            if (rect.width < 0)
                return;

            rect.y++;
            rect.width -= _elementRightPadding;
            SerializedProperty pairProperty = _pairsProperty.GetArrayElementAtIndex(index);

            SerializedProperty keyProperty = pairProperty.FindPropertyRelative("key");
            SerializedProperty valueProperty = pairProperty.FindPropertyRelative("value");

            // Draws the key field.
            Color previousColor = GUI.color;
            if (pairProperty.FindPropertyRelative("isDuplicateKey").boolValue)
                GUI.color = new Color(0.9f, 0.4f, 0.4f);

            Rect keyRect = SplitRect(rect, true);
            keyRect.height = EditorGUI.GetPropertyHeight(keyProperty, keyProperty.isExpanded);
            EditorGUI.BeginProperty(keyRect, GUIContent.none, keyProperty);
            keyRect.xMin += 12;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none, keyProperty.isExpanded);
            if (EditorGUI.EndChangeCheck())
                EnssureSaveDuplicates();
            EditorGUI.EndProperty();

            GUI.color = previousColor;

            // Draws the value field.
            Rect valueRect = SplitRect(rect, false);
            valueRect.xMin += 12;
            valueRect.height = EditorGUI.GetPropertyHeight(valueProperty, valueProperty.isExpanded);
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, valueProperty.isExpanded);
        }

        private Rect SplitRect(Rect rect, bool isKey)
        {
            if (isKey)
                return new Rect(rect)
                {
                    width = (_defaultSplitPercent * rect.width) - _elementRightPadding
                };
            else
                return new Rect(rect)
                {
                    width = (1 - _defaultSplitPercent) * rect.width,
                    x = rect.x + (_defaultSplitPercent * rect.width) + _elementRightPadding
                };
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

        private static class Styles
        {
            public static readonly GUIStyle BackgroundStyle;
            public static readonly GUIStyle HeaderStyle;

            static Styles()
            {
                BackgroundStyle = new GUIStyle("RL Background");
                HeaderStyle = new GUIStyle("RL Empty Header");
            }
        }
    }
}
