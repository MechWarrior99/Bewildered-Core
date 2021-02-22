using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    public static class ExtraEditorGUI
    {
        private static Dictionary<string, ReorderablePropertyList> _reorderableLists = new Dictionary<string, ReorderablePropertyList>(5);

        public static void PropertyField(SerializedProperty property, params GUILayoutOption[] options)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
                DrawListProperty(property);
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
                ManagedReferenceField(GUILayoutUtility.GetRect(100, EditorGUI.GetPropertyHeight(property)), property);
            else
                EditorGUILayout.PropertyField(property, property.isExpanded, options);
        }

        public static void PropertyField(Rect position, SerializedProperty property)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
                DrawListProperty(property);
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
                ManagedReferenceField(position, property);
            else
                EditorGUI.PropertyField(position, property, property.isExpanded);
        }

        public static void DrawListProperty(SerializedProperty property)
        {
            ReorderablePropertyList reorderable;
            if (_reorderableLists.TryGetValue(property.name, out reorderable))
            {
                reorderable.Property = property;
            }
            else
            {
                reorderable = new ReorderablePropertyList(property);
                _reorderableLists.Add(property.name, reorderable);
            }
            reorderable.DrawLayout();
        }

        private static int _selectedIndex = -1;
        private static List<Type> _lastTypeList = new List<Type>();

        public static void ManagedReferenceField(Rect position, SerializedProperty property)
        {
            float typeWidth = 50;
            Rect typeFieldRect = new Rect()
            {
                x = position.x + position.width - typeWidth,
                y = position.y,
                width = typeWidth,
                height = EditorGUIUtility.singleLineHeight
            };

            Event evt = Event.current;
            EventType cachedEventType = evt.type;

            if (typeFieldRect.Contains(evt.mousePosition) && evt.type == EventType.MouseDown)
                evt.Use();

            EditorGUI.PropertyField(position, property, property.isExpanded);

            if (typeFieldRect.Contains(evt.mousePosition) && evt.type == EventType.Used)
                evt.type = cachedEventType;

            if (EditorGUI.DropdownButton(typeFieldRect, new GUIContent("Type"), FocusType.Passive))
            {
                Type propertyType = property.GetPropertyType();
                _lastTypeList = new List<Type>(TypeCache.GetTypesDerivedFrom(propertyType));
                _lastTypeList.Insert(0, property.GetPropertyType());
                _lastTypeList.Insert(0, null);
                FilteredList<Type> filteredTypes = new FilteredList<Type>(_lastTypeList.ToArray(), type => type != null ? type.Name : "Null");

                Action<int> onSelect = i =>
                {
                    _selectedIndex = i;
                };

                SearchablePopup<Type> popup = new SearchablePopup<Type>(filteredTypes, 0, index => _selectedIndex = index);
                popup.Show(typeFieldRect);
            }

            if (_selectedIndex >= 0)
            {
                if (_lastTypeList[_selectedIndex] == null)
                    property.managedReferenceValue = null;
                else
                    property.managedReferenceValue = Activator.CreateInstance(_lastTypeList[_selectedIndex]);
                _selectedIndex = -1;
            }

            //position.height = EditorGUIUtility.singleLineHeight;
            //property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName);
            //position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            //if (property.isExpanded)
            //{
            //    SerializedProperty iterator = property.Copy();
            //    if (iterator.Next(true))
            //    {
            //        SerializedProperty end = iterator.GetEndProperty();
            //        do
            //        {
            //            position.height = EditorGUI.GetPropertyHeight(iterator);
            //            EditorGUI.PropertyField(position, iterator);

            //            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            //        } while (iterator.Next(false) && !SerializedProperty.EqualContents(iterator, end));
            //    }
            //}


            //using (new GUILayout.HorizontalScope())
            //{

            //    if (GUILayout.Button("T", GUILayout.Width(25)))
            //    {
            //        var cachedTypes = TypeCache.GetTypesDerivedFrom(property.GetPropertyType());
            //        List<Type> types = new List<Type>(cachedTypes);
            //        types.Insert(0, property.GetPropertyType());
            //        types.Insert(0, null);
            //        FilteredList<Type> filteredTypes = new FilteredList<Type>(types.ToArray(), type => type != null ? type.Name : "Null");

            //        Action<int> onSelect = i =>
            //        {
            //            _selectedIndex = i;
            //        };

            //        SearchablePopup<Type> popup = new SearchablePopup<Type>(filteredTypes, 0, onSelect);
            //        popup.Show(GUILayoutUtility.GetLastRect());
            //    }
            //}

            //if (_selectedIndex >= 0)
            //{
            //    var cachedTypes = TypeCache.GetTypesDerivedFrom(property.GetPropertyType());
            //    List<Type> types = new List<Type>(cachedTypes);
            //    types.Insert(0, property.GetPropertyType());
            //    types.Insert(0, null);

            //    if (types[_selectedIndex] == null)
            //        property.managedReferenceValue = null;
            //    else
            //        property.managedReferenceValue = Activator.CreateInstance(types[_selectedIndex]);

            //    _selectedIndex = -1;
            //}
        }
    }
}
