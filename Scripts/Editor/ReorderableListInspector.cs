using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    [CustomEditor(typeof(Component), true)]
    [CanEditMultipleObjects]
    internal class ReorderableListInspector : UnityEditor.Editor
    {
        private Dictionary<string, ReorderablePropertyList> _reorderableLists = new Dictionary<string, ReorderablePropertyList>(5);

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var property = serializedObject.GetIterator();
            bool next = property.NextVisible(true);
            if (next)
            {
                do
                {
                    HandleProperty(property);
                }
                while (property.NextVisible(false));
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void HandleProperty(SerializedProperty property)
        {
            bool isdefaultScriptProperty = property.name.Equals("m_Script") && property.type.Equals("PPtr<MonoScript>") && property.propertyType == SerializedPropertyType.ObjectReference && property.propertyPath.Equals("m_Script");
            bool cachedGUIEnabled = GUI.enabled;

            if (isdefaultScriptProperty)
                GUI.enabled = false;

            //if (property.isArray && property.propertyType != SerializedPropertyType.String)
            //    GetReorderableList(property).DrawLayout();
            //else
            //    EditorGUILayout.PropertyField(property, property.isExpanded);
            ExtraEditorGUI.PropertyField(property);

            if (isdefaultScriptProperty)
                GUI.enabled = cachedGUIEnabled;
        }

        private ReorderablePropertyList GetReorderableList(SerializedProperty property)
        {
            ReorderablePropertyList reorderable;
            if (_reorderableLists.TryGetValue(property.name, out reorderable))
            {
                reorderable.Property = property;
                return reorderable;
            }
            else
            {
                reorderable = new ReorderablePropertyList(property);
                _reorderableLists.Add(property.name, reorderable);
                return reorderable;
            }
        }
    } 
}
