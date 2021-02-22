using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bewildered.UIToolkit
{
    public class GridView : BindableElement
    {
        public static readonly string UssClassName = "bewildered-grid-view";
        public static readonly string RowUssClassName = UssClassName + "__row";
        public static readonly string ItemUssClassName = UssClassName + "__item";
        public static readonly string ItemSelectedUssClassName = ItemUssClassName + "--selected";

        private ListView _listView;
        private int _itemWidth = 64;
        private int _itemHeight = 30;
        private DynamicGridCollection<object> _gridItems;
        private Func<VisualElement> _makeItem;
        private Action<VisualElement, int> _bindItem;
        private Action<VisualElement, int> _unbindItem;
        private SelectionType _selectionType = SelectionType.Multiple;
        [SerializeField]
        private List<int> _selectedIndices = new List<int>();
        private List<object> _selectedItems = new List<object>();
        private int _rangeSelectionOriginIndex = -1;

        public int ItemHeight
        {
            get { return _itemHeight; }
            set 
            {
                if (value != _itemHeight)
                {
                    _itemHeight = value;
                    _listView.itemHeight = ItemHeight;
                }
            }
        }

        public int ItemWidth
        {
            get { return _itemWidth; }
            set
            {
                if (value != _itemWidth)
                {
                    _itemWidth = value;
                    Refresh();
                }
            }
        }

        public int ItemsPerRow
        {
            get { return _gridItems.CountPerRow; }
        }

        /// <summary>
        /// Controls the selection state. You can set the state to disable selections, have one selectable item, or have multiple selectable items.
        /// </summary>
        public SelectionType SelectionType
        {
            get { return _selectionType; }
            set
            {
                _selectionType = value;
                if (_selectionType == SelectionType.None)
                    ClearSelection();
            }
        }

        /// <summary>
        /// The items data source. This property must be set for the <see cref="GridView"/> to function.
        /// </summary>
        public IList ItemsSource
        {
            get { return _gridItems.ItemsSource; }
            set
            {
                if (_gridItems == null)
                {
                    _gridItems = new DynamicGridCollection<object>(value);
                    _listView.itemsSource = _gridItems;
                }
                else
                {
                    _gridItems.ItemsSource = value.OfType<object>().ToList();
                }
                Refresh();
            }
        }

        /// <summary>
        /// Callback for constructing the <see cref="VisualElement"/> that will serve as the template for each recycled and re-bound element in the grid. This property must be set for the <see cref="GridView"/> to function.
        /// </summary>
        public Func<VisualElement> MakeItem
        {
            get { return _makeItem; }
            set
            {
                if (value == _makeItem)
                    return; 

                _makeItem = value;
                Refresh();
            }
        }

        /// <summary>
        /// Callback for binding a data item to a <see cref="VisualElement"/>.
        /// </summary>
        public Action<VisualElement, int> BindItem
        {
            get { return _bindItem; }
            set 
            {
                if (value == _bindItem)
                    return;

                _bindItem = value;
                Refresh();
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndices.Count == 0 ? -1 : _selectedIndices.First(); }
            set { SetSelection(value); }
        }

        public override VisualElement contentContainer
        {
            get { return null; }
        }

        /// <summary>
        /// Callback triggered when the user 'acts on' a selection of one or more items, for example by double-clicking or pressing Enter.
        /// </summary>
        public event Action<IEnumerable<object>> OnItemsChosen;

        /// <summary>
        /// Calback triggered when the selection changes.
        /// </summary>
        public event Action<IEnumerable<object>> OnSelectionChange;

        public GridView()
        {
            _listView = new ListView();
            _listView.makeItem = MakeRowItem;
            _listView.bindItem = BindRowItem;
            hierarchy.Add(_listView);
            _listView.style.flexGrow = 1;
            style.flexGrow = 1;

            _listView.Q<ScrollView>().contentContainer.UnregisterCallback<KeyDownEvent>(_listView.OnKeyDown);
            //_listView.Q<ScrollView>().contentContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            styleSheets.Add(Resources.Load<StyleSheet>("Bewildered Core/UI/GridView"));

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<GeometryChangedEvent>(OnSizeChange);
            
            focusable = true;

        }

        public GridView(IList itemsSource, int itemWidth, int itemHeight, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem) : this()
        {
            _gridItems = new DynamicGridCollection<object>(itemsSource);
            _makeItem = makeItem;
            _bindItem = bindItem;
            _itemWidth = itemWidth;
            _listView.itemHeight = itemHeight;

            _listView.itemsSource = _gridItems;
            
            Refresh();
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            Debug.Log("On Attach To Panel");
            _listView.Q<ScrollView>().contentContainer.UnregisterCallback<KeyDownEvent>(_listView.OnKeyDown);
        }

        private VisualElement MakeRowItem()
        {
            VisualElement rowElement = new VisualElement();
            rowElement.style.flexDirection = FlexDirection.Row;
            rowElement.AddToClassList(RowUssClassName);
            for (int i = 0; i < _gridItems.CountPerRow; i++)
            {
                if (_makeItem != null)
                    rowElement.Add(_makeItem());
                else
                    rowElement.Add(new Label("Grid Item"));

                rowElement[i].style.width = _itemWidth;
                rowElement[i].AddToClassList(ItemUssClassName);
            }

            return rowElement;
        }

        private void BindRowItem(VisualElement rowElement, int rowIndex)
        {
            rowElement.userData = rowIndex;
            rowElement.name = "row-index: " + rowIndex.ToString();
            GridRow<object> row = _gridItems[rowIndex];

            for (int i = 0; i < row.items.Count; i++)
            {
                int sourceItemIndex = row.sourceIndex + i;
                _bindItem?.Invoke(rowElement.ElementAt(i), sourceItemIndex);

                if (_selectedIndices.Contains(sourceItemIndex))
                    rowElement[i].AddToClassList(ItemSelectedUssClassName);
                else
                    rowElement[i].RemoveFromClassList(ItemSelectedUssClassName);

                rowElement[i].userData = sourceItemIndex;
            }
        }

        public void ScrollToItem(int itemIndex)
        {
            _listView.ScrollToItem(_gridItems.GetRowIndex(itemIndex));
        }

        /// <summary>
        /// Scroll to a specified <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">Element to scroll to.</param>
        public void ScrollTo(VisualElement element)
        {
            _listView.ScrollTo(element);
        }

        /// <summary>
        /// Clear, recreate all visible visual elements, and rebind all items. This should be called whenever the items source changes.
        /// </summary>
        public void Refresh()
        {
            _selectedItems.Clear();
            _selectedIndices.Clear();
            _listView.Refresh();
        }

        // ---------------------------
        // Selection methods.
        // ---------------------------

        /// <summary>
        /// Sets the currently selected item.
        /// </summary>
        /// <param name="index">Item index.</param>
        public void SetSelection(int index)
        {
            SetSelectionInternal(new int[] { index }, true);
        }

        /// <summary>
        /// Sets a collection of selected items.
        /// </summary>
        /// <param name="indices">Collection of items to be selected.</param>
        public void SetSelection(IEnumerable<int> indices)
        {
            SetSelectionInternal(indices, true);
        }

        /// <summary>
        /// Sets a collection of selected items without triggering a selection change callback.
        /// </summary>
        /// <param name="indices">Collection of items to be selected.</param>
        public void SetSelectionWithoutNotify(IEnumerable<int> indices)
        {
            SetSelectionInternal(indices, false);
        }

        private void SetSelectionInternal(IEnumerable<int> indices, bool sendNotification)
        {
            if (!Validate() || indices == null)
                return;

            ClearSelectionWithoutNotify();
            foreach (var index in indices)
                AddToSelectionDirect(index);

            if (sendNotification)
                NotifyOfSelectionChange();
        }

        public void AddToSelection(int index)
        {
            if (!Validate())
                return;

            AddToSelectionDirect(index);
            NotifyOfSelectionChange();
        }

        private void AddToSelectionDirect(int index)
        {
            if (_selectedIndices.Contains(index))
                return;

            _selectedIndices.Add(index);
            _selectedItems.Add(_gridItems.ItemsSource[index]);

            SetElementSelection(index, true);
        }

        public void RemoveFromSelection(int index)
        {
            if (!Validate() || !_selectedIndices.Contains(index))
                return;

            _selectedIndices.Remove(index);
            _selectedItems.Remove(_gridItems.ItemsSource[index]);
            SetElementSelection(index, false);
            NotifyOfSelectionChange();
        }

        /// <summary>
        /// Unselects any selected items.
        /// </summary>
        public void ClearSelection()
        {
            ClearSelectionWithoutNotify();
            NotifyOfSelectionChange();
        }

        public void ClearSelectionWithoutNotify()
        {
            if (!Validate() || _selectedIndices.Count == 0)
                return;

            _selectedIndices.Clear();
            _selectedItems.Clear();
            SetAllElementSelection(false);
        }

        private void NotifyOfSelectionChange()
        {
            OnSelectionChange?.Invoke(_selectedItems);
        }

        // ---------------------------
        // Event handling methods.
        // ---------------------------

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!Validate())
                return;

            int clickedRowIndex = (int)(evt.localMousePosition.y / ItemHeight);
            if (clickedRowIndex > _gridItems.Count - 1)
                return;

            int clickedColumnIndex = (int)evt.localMousePosition.x / ItemWidth;

            VisualElement rowElement = GetRowElementFromPosition(evt.localMousePosition);
            int clickedSourceIndex = (int)rowElement[clickedColumnIndex].userData;
            

            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (evt.clickCount == 1)
            {
                if (SelectionType == SelectionType.None)
                    return;

                if (SelectionType == SelectionType.Multiple && evt.actionKey) // Ctrl-click item.
                {
                    _rangeSelectionOriginIndex = clickedSourceIndex;

                    if (_selectedIndices.Contains(clickedSourceIndex))
                        RemoveFromSelection(clickedSourceIndex);
                    else
                        AddToSelection(clickedSourceIndex);
                }
                else if (SelectionType == SelectionType.Multiple && evt.shiftKey) // Shift-click item.
                {
                    if (_rangeSelectionOriginIndex == -1)
                    {
                        _rangeSelectionOriginIndex = clickedSourceIndex;
                        SetSelection(clickedSourceIndex);
                    }
                    else
                    {
                        ClearSelectionWithoutNotify();

                        int fromIndex = Mathf.Min(clickedSourceIndex, _rangeSelectionOriginIndex);
                        int toIndex = Mathf.Max(clickedSourceIndex, _rangeSelectionOriginIndex);

                        for (int i = fromIndex; i <= toIndex; i++)
                        {
                            AddToSelection(i);
                        }
                    }
                }
                else if (SelectionType == SelectionType.Multiple && _selectedIndices.Contains(clickedSourceIndex))
                {

                }
                else // Single click
                {
                    Debug.Log("Clicked Pos: " + evt.localMousePosition);
                    Debug.Log("Selected: " + clickedSourceIndex);
                    _rangeSelectionOriginIndex = clickedSourceIndex;
                    SetSelection(clickedSourceIndex);
                }
            }
            else if (evt.clickCount == 2)
            {
                _rangeSelectionOriginIndex = clickedSourceIndex;
                SetSelection(clickedSourceIndex);
                OnItemsChosen?.Invoke(_selectedItems);
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            bool doScroll = true;

            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:
                    if (SelectedIndex >= ItemsPerRow)
                        SelectedIndex -= ItemsPerRow;
                    break;
                case KeyCode.DownArrow:
                    if (SelectedIndex < _gridItems.ItemsSource.Count)
                        SelectedIndex = Mathf.Min(SelectedIndex + ItemsPerRow, _gridItems.ItemsSource.Count-1);
                    break;
                case KeyCode.LeftArrow:
                    if (SelectedIndex > 0)
                        SelectedIndex--;
                    break;
                case KeyCode.RightArrow:
                    if (SelectedIndex + 1 < _gridItems.ItemsSource.Count)
                        SelectedIndex++;
                    break;
                case KeyCode.Home:
                    SelectedIndex = 0;
                    break;
                case KeyCode.End:
                    SelectedIndex = ItemsSource.Count - 1;
                    break;
                case KeyCode.Return:
                    OnItemsChosen?.Invoke(_selectedItems);
                    break;
                default:
                    evt.StopPropagation();
                    break;
            }

            if (doScroll)
                ScrollToItem(SelectedIndex);
        }

        private void OnSizeChange(GeometryChangedEvent evt)
        {
            if (Mathf.Approximately(evt.newRect.width, evt.oldRect.width))
                return;

            int countPerRow = Mathf.FloorToInt(evt.newRect.width / ItemWidth);
            if (countPerRow != _gridItems.CountPerRow)
            {
                _gridItems.CountPerRow = countPerRow;
                Refresh();
            }
        }

        // ---------------------------
        // Utility methods.
        // ---------------------------

        private bool Validate()
        {
            return _gridItems != null && _makeItem != null && _bindItem != null;
        }


        // Set the selection state for an item at a specified index.
        private void SetElementSelection(int index, bool selected)
        {
            VisualElement itemElement = GetItemElement(index);
            
            if (itemElement != null)
                itemElement.EnableInClassList(ItemSelectedUssClassName, selected);
        }

        // Set the selection state for all items.
        private void SetAllElementSelection(bool selected)
        {
            _listView.Q(null, ScrollView.contentUssClassName).Query(null, ItemUssClassName).ForEach(e =>
            {
                if (selected)
                    e.AddToClassList(ItemSelectedUssClassName);
                else
                    e.RemoveFromClassList(ItemSelectedUssClassName);
            });
        }

        public int GetItemIndexFromPosition(Vector2 localPosition)
        {
            return (int)GetItemElementFromPosition(localPosition).userData;
        }

        public VisualElement GetItemElementFromPosition(Vector2 localPosition)
        {
            int columnIndex = (int)localPosition.x / ItemWidth;
            VisualElement rowElement = GetRowElementFromPosition(localPosition);

            if (rowElement == null)
                return null;

            if (columnIndex >= rowElement.childCount)
                return null;

            return rowElement[columnIndex];
        }

        private VisualElement GetRowElementFromPosition(Vector2 localPosition)
        {
            VisualElement rowsContainer = _listView.Q(null, ScrollView.contentUssClassName);
            float offset = _listView.Q<ScrollView>().scrollOffset.y % ItemHeight;
            int visibleRowIndex = (int)((localPosition.y + offset) / ItemHeight);

            if (visibleRowIndex >= rowsContainer.childCount)
                return null;

            return rowsContainer[visibleRowIndex];
        }

        // Returns the VisualElement for a specified data index if it is visible, otherwise returns null;
        private VisualElement GetItemElement(int itemSourceIndex)
        {
            VisualElement rowsContainer = _listView.Q(null, ScrollView.contentUssClassName);

            if (rowsContainer.childCount == 0)
                return null;

            int firstVisibleRowSourceIndex = (int)rowsContainer[0].userData;
            int lastVisibleRowSourceIndex = (int)rowsContainer[rowsContainer.childCount - 1].userData;

            if (_gridItems[firstVisibleRowSourceIndex].sourceIndex > itemSourceIndex || _gridItems[lastVisibleRowSourceIndex].sourceIndex < itemSourceIndex)
                return null;

            int rowSourceIndex = _gridItems.GetRowIndex(itemSourceIndex);
            int visibleRowIndex = rowSourceIndex - firstVisibleRowSourceIndex;

            int indexInRow = itemSourceIndex - _gridItems[rowSourceIndex].sourceIndex;

            return rowsContainer[visibleRowIndex][indexInRow];
        }
    }
}