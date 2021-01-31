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
                DrawManagedReference(property, options);
            else
                EditorGUILayout.PropertyField(property, property.isExpanded, options);
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

        public static void DrawManagedReference(SerializedProperty property, params GUILayoutOption[] options)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(property, property.isExpanded, options);
                if (GUILayout.Button("T", GUILayout.Width(25)))
                {
                    var cachedTypes = TypeCache.GetTypesDerivedFrom(property.GetPropertyType());
                    List<Type> types = new List<Type>(cachedTypes);
                    types.Insert(0, property.GetPropertyType());
                    types.Insert(0, null);
                    FilteredList<Type> filteredTypes = new FilteredList<Type>(types.ToArray(), type => type != null ? type.Name : "Null");

                    Action<int> onSelect = i =>
                    {
                        _selectedIndex = i;
                    };

                    SearchablePopup<Type> popup = new SearchablePopup<Type>(filteredTypes, 0, onSelect);
                    popup.Show(GUILayoutUtility.GetLastRect());
                }
            }

            if (_selectedIndex >= 0)
            {
                var cachedTypes = TypeCache.GetTypesDerivedFrom(property.GetPropertyType());
                List<Type> types = new List<Type>(cachedTypes);
                types.Insert(0, property.GetPropertyType());
                types.Insert(0, null);

                if (types[_selectedIndex] == null)
                    property.managedReferenceValue = null;
                else
                    property.managedReferenceValue = Activator.CreateInstance(types[_selectedIndex]);

                _selectedIndex = -1;
            }
        }
    }
}
