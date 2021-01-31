using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    [CustomPropertyDrawer(typeof(UHashset<>), true)]
    internal class UHashsetPropertyDrawer : PropertyDrawer
    {
        private class DrawerData
        {
            public ReorderablePropertyList propertyList;
            public object targetInstanceValue;
            public bool isSerializingToHashset = true;
        }

        private FieldInfo _doSerializeToHashsetField;
        // Only a single PropertyDrawer instance is created for elements in an array and is then given the data for each element. 
        // So if there was a list of UHashsets, they would all share the same ReorderableList if they were not put in a collection like this.
        private Dictionary<string, DrawerData> _propertyPathsDrawerData = new Dictionary<string, DrawerData>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_propertyPathsDrawerData.TryGetValue(property.propertyPath, out DrawerData drawerData))
            {
                drawerData = InitializeDrawerData(property, label);
            }
            
            drawerData.propertyList.HeaderContent = label;

            EditorGUI.BeginChangeCheck();
            drawerData.propertyList.Draw(position);
            if (EditorGUI.EndChangeCheck() && drawerData.isSerializingToHashset)
            {
                drawerData.isSerializingToHashset = false;
                _doSerializeToHashsetField.SetValue(drawerData.targetInstanceValue, false);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_doSerializeToHashsetField == null)
            {
                System.Type hashsetBaseType = property.GetPropertyType();
                if (hashsetBaseType == fieldInfo.FieldType)
                    hashsetBaseType = hashsetBaseType.BaseType;
                else if (hashsetBaseType == fieldInfo.DeclaringType)
                    hashsetBaseType = fieldInfo.FieldType.BaseType;
                else
                    hashsetBaseType = hashsetBaseType.BaseType;
                
                _doSerializeToHashsetField = hashsetBaseType.GetField("_doSerializeToHashset", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            
            if (!_propertyPathsDrawerData.TryGetValue(property.propertyPath, out DrawerData drawerData))
            {
                drawerData = InitializeDrawerData(property, label);
            }

            return drawerData.propertyList.GetHeight();
        }

        private DrawerData InitializeDrawerData(SerializedProperty property, GUIContent label)
        {
            ReorderablePropertyList propertyList = new ReorderablePropertyList(property.FindPropertyRelative("_hashsetList"), label.text, true, true, true, true);
            DrawerData drawerData = new DrawerData()
            {
                propertyList = propertyList,
                targetInstanceValue = property.GetValue()
            };
            _propertyPathsDrawerData[property.propertyPath] = drawerData;
            return drawerData;
        }
    } 
}
