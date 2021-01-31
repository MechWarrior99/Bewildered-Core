using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Bewildered.Editor
{
    /// <summary>
    /// A searchable popup menu.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchablePopup<T> : AdvancedPopupWindow
    {
        private readonly Action<int> _onSelect;
        private readonly int _currentIndex;

        public FilteredList<T> Options { get; private set; }
        public ListView OptionsListView { get; private set; }
        public ToolbarSearchField SearchField { get; private set; }

        public virtual int ItemHeight
        {
            get { return 21; }
        }

        public SearchablePopup(FilteredList<T> options, int current, Action<int> onSelect)
        {
            Options = options;

            _onSelect = onSelect;
            _currentIndex = current;
        }

        public override void Show(Rect activeRect)
        {
            base.Show(activeRect);

            // The element won't be focused until the window is opened, 
            // so need to set the focus here, after the window has already opened.
            EditorWindow.rootVisualElement.schedule.Execute(() => {
                SearchField.Q(TextField.textInputUssName).Focus();
            });
        }

        public override VisualElement CreateContentElement()
        {
            // Root.
            VisualElement root = new VisualElement();
            root.name = "rootElement";
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.bewildered.core/Scripts/Editor/UI/SearchablePopup.uss"));

            // Search field.
            SearchField = new ToolbarSearchField();
            SearchField.name = "searchField";
            SearchField.RegisterValueChangedCallback(OnSearchFieldChange);
            SearchField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            root.Add(SearchField);

            // ListView items.
            OptionsListView = new ListView(Options.Entries, ItemHeight, MakeItem, BindItem);
            OptionsListView.name = "itemsContainer";
            OptionsListView.style.borderTopColor = BorderColor;
            OptionsListView.selectedIndex = _currentIndex;
            OptionsListView.RegisterCallback<KeyDownEvent>(OnKeyDown);
            root.Add(OptionsListView);

            return root;
        }

        protected virtual VisualElement MakeItem()
        {
            return new Label();
        }

        protected virtual void BindItem(VisualElement element, int index)
        {
            Label label = (Label)element;
            label.text = Options.GetItemString(Options[index].Item);
            label.RegisterCallback<MouseDownEvent>(evt => 
            {
                SelectItem(index);
            });
        }

        protected virtual void SelectItem(int index)
        {
            _onSelect(Options[index].Index);
            EditorWindow.Close();
        }

        private void OnSearchFieldChange(ChangeEvent<string> evt)
        {
            if (Options.UpdateFilter(evt.newValue))
            {
                OptionsListView.Refresh();
                OptionsListView.selectedIndex = 0;
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                SelectItem(OptionsListView.selectedIndex);
            }
            else if (evt.keyCode == KeyCode.DownArrow)
            {
                OptionsListView.selectedIndex = (OptionsListView.selectedIndex + 1) % OptionsListView.itemsSource.Count;
                evt.PreventDefault();
            }
            else if (evt.keyCode == KeyCode.UpArrow)
            {
                OptionsListView.selectedIndex = (OptionsListView.selectedIndex - 1) % OptionsListView.itemsSource.Count;
                evt.PreventDefault();
            }
        }
    } 
}
