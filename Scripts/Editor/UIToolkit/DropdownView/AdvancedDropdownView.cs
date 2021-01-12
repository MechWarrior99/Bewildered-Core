using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Bewildered.Core.Editor
{
    /// <summary>
    /// Inherit from this class to implement your own drop-down control.
    /// </summary>
    /// <remarks>A dropdown menu like the <see cref="UnityEditor.IMGUI.Controls.AdvancedDropdown"/>, but using <see cref="VisualElement"/>s and more customization options.</remarks>
    public abstract class AdvancedDropdownView
    {
        static class Styles
        {
            public static GUIContent rightArrowContent;
            public static GUIContent leftArrowContent;
            public static GUIStyle rightArrow = new GUIStyle("ArrowNavigationRight");
            public static GUIStyle leftArrow = new GUIStyle("ArrowNavigationLeft");

            static Styles()
            {
                rightArrowContent = EditorGUIUtility.IconContent("ArrowNavigationRight");
                leftArrowContent = EditorGUIUtility.IconContent("ArrowNavigationLeft");
            }
        }

        protected VisualElement _rootVisualElement;
        protected EnhancedListView _list;
        private Button _headerButton;

        /// <summary>
        /// The first <see cref="DropdownItem"/> for the <see cref="AdvancedDropdownView"/>.
        /// </summary>
        public DropdownItem RootItem { get; private set; }

        /// <summary>
        /// The <see cref="DropdownItem"/> who's <see cref="DropdownItem.Children"/> is being shown.
        /// </summary>
        public DropdownItem ParentItem { get; private set; }

        /// <summary>
        /// The search field <see cref="VisualElement"/>.
        /// </summary>
        public ToolbarSearchField SearchField { get; }

        /// <summary>
        /// Event for when a <see cref="DropdownItem"/> with no children is selected.
        /// </summary>
        public event Action<DropdownItem> OnItemSelected;

        public static string dropdownItemUssClassName = "bewildered-dropdown-item";

        public AdvancedDropdownView()
        {
            _rootVisualElement = new VisualElement();
            _rootVisualElement.name = "base";
            _rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("Bewildered/AdvancedDropdown"));

            Color borderColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            _rootVisualElement.style.borderTopColor = borderColor;
            _rootVisualElement.style.borderRightColor = borderColor;
            _rootVisualElement.style.borderBottomColor = borderColor;
            _rootVisualElement.style.borderLeftColor = borderColor;
            
            RootItem = BuildRoot();
            ParentItem = RootItem;

            // Search field.
            SearchField = new ToolbarSearchField();
            SearchField.RegisterValueChangedCallback(OnSearchValueChanged);
            _rootVisualElement.Add(SearchField);

            // Button for going back to the previus parent and for displaying the name of the current parent.
            _headerButton = new Button(OnHeaderSelected);
            _headerButton.name = "header";
            _headerButton.text = RootItem.Name;

            Image arrowImage = VisualElementUtility.CreateIconImage(Styles.leftArrowContent);
            arrowImage.name = "arrow";
            arrowImage.style.display = DisplayStyle.None;
            _headerButton.Add(arrowImage);

            _rootVisualElement.Add(_headerButton);

            _list = new EnhancedListView();
            _list.makeItem = MakeCompleteItem;
            _list.bindItem = BindCompleteItem;
            _list.itemsSource = RootItem.Children;
            _list.itemHeight = 21;
            _list.onSelectionChanged += selectedItems => 
            {
                if (selectedItems.Count > 0)
                    SelectMenuItem((DropdownItem)selectedItems[0]);
            };
            _list.style.flexGrow = 1.0f;
            _list.Refresh();
            _rootVisualElement.Add(_list);
        }

        private void OnHeaderSelected()
        {
            // Selecting the button taks focus away from the SearchField, so it needs to be returned.
            SearchField.Q(TextField.textInputUssName).Focus();

            SelectMenuItem(ParentItem.Parent);
        }

        public void Show(Rect rect)
        {
            DropdownWindow window = ScriptableObject.CreateInstance<DropdownWindow>();
            window.rootVisualElement.Add(_rootVisualElement);
            OnItemSelected += item => window.Close();
            window.ShowAsDropDown(rect, new Vector2(250, 300));

            // The element won't be focused until the window is opened, 
            // so need to set the focus here, after the window has already opened.
            window.rootVisualElement.schedule.Execute(() => {
                SearchField.Q(TextField.textInputUssName).Focus();
            });
        }

        public void Reload()
        {
            RootItem = BuildRoot();
            ParentItem = RootItem;

            _headerButton.text = RootItem.Name;

            _list.itemsSource = RootItem.Children;
            _list.Refresh();
        }

        protected abstract DropdownItem BuildRoot();

        private VisualElement MakeCompleteItem()
        {
            VisualElement item = MakeItem();
            item.AddToClassList(dropdownItemUssClassName);

            return item;
        }

        /// <summary>
        /// Constructs the <see cref="VisualElement"/> that serve as the template for each item in the <see cref="AdvancedDropdownView"/>.
        /// </summary>
        /// <returns>The <see cref="VisualElement"/> for an item in the <see cref="AdvancedDropdownView"/>.</returns>
        protected virtual VisualElement MakeItem()
        {
            VisualElement item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;

            Image icon = new Image();
            icon.name = "icon";
            icon.style.width = 16.0f;
            icon.style.height = 16.0f;
            item.Add(icon);

            Label label = new Label("Item");
            item.Add(label);
            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1.0f;
            item.Add(spacer);
            item.Add(CreateItemGroupArrow());
            return item;
        }

        protected Image CreateItemGroupArrow()
        {
            Image arrowImage = VisualElementUtility.CreateIconImage(Styles.rightArrowContent);
            arrowImage.name = "arrow";
            return arrowImage;
        }

        private void BindCompleteItem(VisualElement element, int index)
        {
            Image arrowImage = element.Q<Image>("arrow");
            if (arrowImage != null)
                arrowImage.style.display = ParentItem.Children[index].HasChildren ? DisplayStyle.Flex : DisplayStyle.None;

            BindItem(element, index);
        }

        protected virtual void BindItem(VisualElement element, int index)
        {
            Label label = element.Q<Label>();
            label.text = ParentItem.Children[index].Name;
        }

        protected virtual void ItemSelected(DropdownItem item)
        {
            
        }

        private void SelectMenuItem(DropdownItem item)
        {
            if (item == null)
                return;

            if (item.HasChildren)
            {
                SetCurrentParentItem(item);
            }
            else
            {
                ItemSelected(item);
                OnItemSelected?.Invoke(item);
            }
        }

        private void OnSearchValueChanged(ChangeEvent<string> evt)
        {
            if (!string.IsNullOrEmpty(evt.newValue))
                SetCurrentParentItem(OnSearch(evt.newValue));

            // When the search is cleared, set the parent back to the root.
            if (string.IsNullOrEmpty(evt.newValue) && !string.IsNullOrEmpty(evt.previousValue))
                SetCurrentParentItem(RootItem);
        }

        protected virtual DropdownItem OnSearch(string searchFilter)
        {
            List<DropdownItem> items = RootItem.GetAllChildren();
            DropdownItem searchParent = new DropdownItem("Search");

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (!Regex.IsMatch(items[i].Name, searchFilter, RegexOptions.IgnoreCase))
                {
                    items.RemoveAt(i);
                }
            }
            searchParent.DirectAddChildRange(items);

            return searchParent;
        }

        public void SetCurrentParentItem(DropdownItem newParent)
        {
            if (newParent == null)
                return;

            ParentItem = newParent;
            _headerButton.text = ParentItem.Name;
            _headerButton.Q<Image>("arrow").style.display = ParentItem.HasParent ? DisplayStyle.Flex : DisplayStyle.None;

            _list.itemsSource = ParentItem.Children;
            _list.Refresh();
            _list.ClearSelection();
        }
    }
}