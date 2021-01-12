using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Bewildered.Core.Editor
{
    /// <summary>
    /// A reorderable list for a collection <see cref="SerializedProperty"/>. Acts as a wrapper around <see cref="ReorderableList"/>.
    /// </summary>
    public class ReorderablePropertyList
    {
        private const float headerHeight = 18.0f;
        private const float headerPadding = 3.0f;
        private const float arraySizeFieldWidth = 48.0f;
        private const float minHeaderHeight = 2.0f;

        public ReorderableList _reorderableList;
        private GUIContent _headerContent;
        private SerializedProperty _arraySizeProperty;
        private bool _displayElementsName = true;
        private bool _isCollapsible = true;
        private bool _displayHeader = true;
        private Action<ReorderablePropertyList> _onAddItem;
        private Action<Rect, ReorderablePropertyList> _onAddDropdownItem;
        private Action<Rect, SerializedProperty> _drawElementGUI;

        public SerializedProperty Property
        {
            get { return _reorderableList.serializedProperty; }
            set 
            {
                _reorderableList.serializedProperty = value;
                _arraySizeProperty = value.FindPropertyRelative("Array.size");
            }
        }

        /// <summary>
        /// The zero-based index of the element that is selected.
        /// </summary>
        public int SelectedIndex
        {
            get { return _reorderableList.index; }
            set { _reorderableList.index = value; }
        }

        /// <summary>
        /// The number of elements in the <see cref="ReorderablePropertyList"/>.
        /// </summary>
        public int Count
        {
            get { return _reorderableList.count; }
        }

        /// <summary>
        /// <c>true</c> if the <see cref="ReorderablePropertyList"/> can be collapsed; otherwise <c>false</c>.
        /// </summary>
        public bool IsCollapsible
        {
            get { return _isCollapsible; }
            set { _isCollapsible = value; }
        }

        public Action<Rect, ReorderablePropertyList> OnAddDropdown
        {
            get { return _onAddDropdownItem; }
            set 
            {
                _onAddDropdownItem = value;
                if (_onAddItem.GetInvocationList().Length > 0)
                {
                    _reorderableList.onAddDropdownCallback -= HandleOnAddDropdownItem;
                    _reorderableList.onAddDropdownCallback += HandleOnAddDropdownItem;
                }
                else
                {
                    _reorderableList.onAddCallback -= HandleOnAddItem;
                }
            }
        }

        public Action<ReorderablePropertyList> OnAdd
        {
            get { return _onAddItem; }
            set 
            {
                _onAddItem = value;
                if (_onAddItem.GetInvocationList().Length > 0)
                {
                    _reorderableList.onAddCallback -= HandleOnAddItem;
                    _reorderableList.onAddCallback += HandleOnAddItem;
                }
                else
                {
                    _reorderableList.onAddDropdownCallback -= HandleOnAddDropdownItem;
                }
            }
        }

        public Action<Rect, SerializedProperty> DrawElementGUI
        {
            get { return _drawElementGUI; }
            set
            {
                _drawElementGUI = value;
                _reorderableList.drawElementCallback = HandleDrawElementGUI;
            }
        }

        public ReorderablePropertyList(SerializedProperty property) : this(property, true, true, true, true)
        {
            
        }

        public ReorderablePropertyList(SerializedProperty property, bool reorderable, bool collapsible, bool displayHeader, bool displayElementsName) : this(property, property.displayName, reorderable, collapsible, displayHeader, displayElementsName)
        {

        }

        public ReorderablePropertyList(SerializedProperty property, string headerDisplayName, bool reorderable, bool collapsible, bool displayHeader, bool displayElementsName)
        {
            _isCollapsible = collapsible;
            _displayHeader = displayHeader;
            _displayElementsName = displayElementsName;
            _headerContent = new GUIContent(headerDisplayName);

            _arraySizeProperty = property.FindPropertyRelative("Array.size");
            _reorderableList = new ReorderableList(property.serializedObject, property, reorderable, !collapsible && displayHeader, true, true);

            if (_isCollapsible || !displayHeader)
                _reorderableList.headerHeight = minHeaderHeight;
            
            _reorderableList.drawElementCallback += DrawElement;
            _reorderableList.elementHeightCallback += ElementHeight;
            _reorderableList.onRemoveCallback += OnRemoveHandler;

            if (!_isCollapsible && _displayHeader)
                _reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, _headerContent);

            if (!_isCollapsible || !displayHeader)
                property.isExpanded = true;
        }

        private void OnRemoveHandler(ReorderableList list)
        {
            // If it is an ObjectReference field, set it to null so that if it has a reference the element is still actually removed and not just set to null.
            if (list.serializedProperty.GetArrayElementAtIndex(list.index).propertyType == SerializedPropertyType.ObjectReference)
                list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue = null;

            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }


        private float ElementHeight(int index)
        {
            float propertyHeight = EditorGUI.GetPropertyHeight(Property.GetArrayElementAtIndex(index), GUIContent.none, true);
            return Mathf.Max(EditorGUIUtility.singleLineHeight, propertyHeight) + EditorGUIUtility.standardVerticalSpacing;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty elementProperty = Property.GetArrayElementAtIndex(index);
            // If it is a custom type then it needs to be indented so that the foldout arrow does not overlap the drag element icon.
            if (elementProperty.propertyType == SerializedPropertyType.Generic)
            {
                rect.x += 11;
                rect.width -= 11;
            }

            GUIContent propertyContent = _displayElementsName ? new GUIContent(elementProperty.displayName) : GUIContent.none;

            rect.height = EditorGUI.GetPropertyHeight(elementProperty, GUIContent.none, true);
            rect.y += 1;

            EditorGUI.BeginProperty(rect, propertyContent, elementProperty);
            EditorGUI.PropertyField(rect, elementProperty, propertyContent, true);
            EditorGUI.EndProperty();
        }

        public float GetHeight()
        {
            if (_isCollapsible)
                return headerHeight + (Property.isExpanded && _reorderableList != null ? _reorderableList.GetHeight() + headerPadding : 0.0f);
            else
                return _reorderableList.GetHeight();
        }

        public void DrawLayout()
        {
            Draw(GUILayoutUtility.GetRect(Screen.width, GetHeight()));
        }

        public void Draw(Rect r)
        {
            Rect headerRect = new Rect(r.x, r.y, r.width, headerHeight);
            Rect arraySizeRect = new Rect(headerRect.xMax - arraySizeFieldWidth, headerRect.y, arraySizeFieldWidth, headerHeight);

            if (_isCollapsible)
            {
                Event evt = Event.current;

                EventType prevEventType = Event.current.type;
                if (evt.type == EventType.MouseUp && arraySizeRect.Contains(evt.mousePosition))
                    Event.current.type = EventType.Used;

                EditorGUI.BeginChangeCheck();
                Property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, Property.isExpanded, _headerContent);
                EditorGUI.EndFoldoutHeaderGroup();
                if (EditorGUI.EndChangeCheck())
                    Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                if (evt.type == EventType.Used && arraySizeRect.Contains(evt.mousePosition))
                    Event.current.type = prevEventType;

                EditorGUI.PropertyField(arraySizeRect, _arraySizeProperty, GUIContent.none);
                // Used to give the field a tooltip as they only show when hovering over labels and not fields.
                EditorGUI.LabelField(arraySizeRect, new GUIContent("", "Array size.")); 
            }

            if (Property.isExpanded)
            {
                if (_isCollapsible)
                {
                    r.y += headerHeight + headerPadding;
                    r.height -= headerHeight + headerPadding; 
                }

                _reorderableList.DoList(r);
            }
        }

        private void HandleOnAddItem(ReorderableList list)
        {
            _onAddItem?.Invoke(this);
        }

        private void HandleOnAddDropdownItem(Rect rect, ReorderableList list)
        {
            _onAddDropdownItem?.Invoke(rect, this);
        }

        private void HandleDrawElementGUI(Rect rect, int index, bool isActive, bool isFocused)
        {
            _drawElementGUI?.Invoke(rect, Property.GetArrayElementAtIndex(index));
        }
    }
}
