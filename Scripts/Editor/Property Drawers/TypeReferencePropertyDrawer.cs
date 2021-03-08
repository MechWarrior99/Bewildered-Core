using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    [CustomPropertyDrawer(typeof(TypeReference))]
    internal class TypeReferencePropertyDrawer : PropertyDrawer
    {
        // Cache TypeInherits attributes by property path so they don't need to be gotten each redraw.
        private Dictionary<string, TypeInheritsAttribute> _cachedInheritsAttributes = new Dictionary<string, TypeInheritsAttribute>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var inheritsAttribute = GetInheritsAttribute(property);
            Type type = Type.GetType(property.FindPropertyRelative("_typeName").stringValue);
            string currentTypeName = "(None)";
            if (type != null)
                currentTypeName = type.GetFriendlyName() + " (" + type.Namespace + ")";
            
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            
            if (EditorGUI.DropdownButton(position, new GUIContent(currentTypeName, currentTypeName), FocusType.Passive))
            {
                // Show a dropdown to select a type from.
                TypeDropdown typeDropdown = new TypeDropdown(inheritsAttribute?.Type, TypeUtility.GetAssembliesTypeHasAccessTo(fieldInfo.DeclaringType), TypeDisplayGroup.ByNamespace, selectedType =>
                {
                    property.FindPropertyRelative("_typeName").stringValue = selectedType?.AssemblyQualifiedName;
                    property.serializedObject.ApplyModifiedProperties();
                });
                typeDropdown.Show(GUIUtility.GUIToScreenRect(position));
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private TypeInheritsAttribute GetInheritsAttribute(SerializedProperty property)
        {
            if (!_cachedInheritsAttributes.TryGetValue(property.propertyPath, out TypeInheritsAttribute inheritsAttribute))
            {
                // Try to get a TypeInherits attribute from the field.
                inheritsAttribute = fieldInfo.GetCustomAttributes(typeof(TypeInheritsAttribute), true).FirstOrDefault() as TypeInheritsAttribute;
                // Cache the TypeInherits result, even if it does not have one and thus is null.
                _cachedInheritsAttributes.Add(property.propertyPath, inheritsAttribute);
            }

            return inheritsAttribute;
        }
    } 
}
