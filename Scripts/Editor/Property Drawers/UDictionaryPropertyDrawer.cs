using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    [CustomPropertyDrawer(typeof(UDictionary<,>), true)]
    public class UDictionaryPropertyDrawer : PropertyDrawer
    {
        private class DrawerData
        {
            public DictionaryList list;
            public object targetInstanceValue;
            public bool isSerializingToDictionary = true;
        }

        private FieldInfo _doSerializeToListField;

        // Only a single PropertyDrawer instance is created for elements in an array and is then given the data for each element. 
        // So if there was a list of UHashsets, they would all share the same ReorderableList if they were not put in a collection like this.
        private Dictionary<string, DrawerData> _propertyPathsDrawerData = new Dictionary<string, DrawerData>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            DrawerData data = GetDrawerData(property);

            EditorGUI.BeginChangeCheck();
            data.list.Draw(position);
            if (EditorGUI.EndChangeCheck() && data.isSerializingToDictionary)
            {
                data.isSerializingToDictionary = false;
                _doSerializeToListField.SetValue(data.targetInstanceValue, false);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            DrawerData data = GetDrawerData(property);
            
            return data.list.GetHeight();
        }

        private DrawerData GetDrawerData(SerializedProperty property)
        {
            if (!_propertyPathsDrawerData.TryGetValue(property.propertyPath, out DrawerData data))
            {
                data = new DrawerData()
                {
                    list = new DictionaryList(property),
                    targetInstanceValue = property.GetValue()
                };
                _propertyPathsDrawerData.Add(property.propertyPath, data);
            }

            return data;
        }

        private void Initialize(SerializedProperty property)
        {
            if (_doSerializeToListField == null)
            {
                System.Type dictionaryBaseType = property.GetPropertyType();
                if (dictionaryBaseType == fieldInfo.FieldType)
                    dictionaryBaseType = dictionaryBaseType.BaseType;
                else if (dictionaryBaseType == fieldInfo.DeclaringType)
                    dictionaryBaseType = fieldInfo.FieldType.BaseType;
                else
                    dictionaryBaseType = dictionaryBaseType.BaseType;

                _doSerializeToListField = dictionaryBaseType.GetField("_doSerializeToList", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
    } 
}
