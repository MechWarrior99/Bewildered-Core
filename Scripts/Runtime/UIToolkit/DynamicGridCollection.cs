using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bewildered.UIToolkit
{
    internal struct GridRow<T>
    {
        public List<T> items;
        public int sourceIndex;

        public GridRow(int sourceIndex, List<T> items)
        {
            this.sourceIndex = sourceIndex;
            this.items = items;
        }
    }

    internal class DynamicGridCollection<T> : IList, IList<GridRow<T>>
    {
        private List<T> _itemsSource = new List<T>();
        
        public int CountPerRow
        {
            get; 
            set;
        }

        bool IList.IsFixedSize => throw new NotImplementedException();

        bool ICollection<GridRow<T>>.IsReadOnly
        {
            get { return false; }
        }

        bool IList.IsReadOnly 
        {
            get { return false; } 
        }

        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        public int Count
        {
            get { return Mathf.CeilToInt((float)_itemsSource.Count / (float)CountPerRow); }
        }

        int ICollection.Count
        {
            get { return Count; }
        }

        public List<T> ItemsSource
        {
            get { return _itemsSource; }
            set { _itemsSource = value; }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (GridRow<T>)value; }
        }

        public GridRow<T> this[int index]
        {
            get
            {
                int startIndex = index * CountPerRow;
                return new GridRow<T>(startIndex, _itemsSource.GetRange(startIndex, GetCountOnRow(index)));
            }
            set
            {
                for (int i = 0; i < value.items.Count; i++)
                {
                    _itemsSource[value.sourceIndex + i] = value.items[i];
                }
            }
        }

        public DynamicGridCollection(IList list)
        {
            ItemsSource = list.OfType<T>().ToList();
        }

        public void Add(GridRow<T> row)
        {
            _itemsSource.AddRange(row.items);
        }

        int IList.Add(object value)
        {
            Add((GridRow<T>)value);
            return _itemsSource.Count - 1;
        }

        public bool Remove(GridRow<T> row)
        {
            for (int i = 0; i < row.items.Count; i++)
            {
                int sourceItemIndex = row.sourceIndex + i;
                if (sourceItemIndex >= _itemsSource.Count)
                    return false;

                if (_itemsSource[sourceItemIndex].Equals(row.items[i]))
                    _itemsSource.RemoveAt(sourceItemIndex);
            }

            return true;
        }

        void IList.Remove(object value)
        {
            Remove((GridRow<T>)value);
        }

        public void RemoveAt(int rowIndex)
        {
            int startSourceIndex = rowIndex * CountPerRow;
            for (int i = GetCountOnRow(rowIndex) - 1; i >= 0; i--)
            {
                _itemsSource.RemoveAt(startSourceIndex + i);
            }
        }

        void IList.RemoveAt(int rowIndex)
        {
            RemoveAt(rowIndex);
        }

        public void Clear()
        {
            _itemsSource.Clear();
        }

        void IList.Clear()
        {
            _itemsSource.Clear();
        }

        public bool Contains(GridRow<T> row)
        {
            int startSourceIndex = row.sourceIndex;

            if (_itemsSource.Count <= startSourceIndex)
                return false;

            for (int i = 0; i < row.items.Count; i++)
            {
                if (startSourceIndex + i >= _itemsSource.Count)
                    return false;

                if (!_itemsSource[startSourceIndex + i].Equals(row.items[i]))
                    return false;
            }

            return true;
        }

        bool IList.Contains(object value)
        {
            return Contains((GridRow<T>)value);
        }

        public int IndexOf(GridRow<T> row)
        {
            return row.sourceIndex / CountPerRow;
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((GridRow<T>)value);
        }

        public void Insert(int index, GridRow<T> row)
        {
            for (int i = 0; i < row.items.Count; i++)
            {
                _itemsSource.Insert(row.sourceIndex + i, row.items[i]);
            }
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (GridRow<T>)value);
        }

        public void CopyTo(GridRow<T>[] array, int index)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<GridRow<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int GetRowIndex(int sourceIndex)
        {
            return Mathf.FloorToInt((float)sourceIndex / (float)CountPerRow);
        }

        /// <summary>
        /// Gets the number of items that are on a specified row. Used to account for the end where there may not be a full row of items.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private int GetCountOnRow(int rowIndex)
        {
            int startSourceIndex = rowIndex * CountPerRow;
            return Mathf.Min(CountPerRow, _itemsSource.Count - startSourceIndex);
        }
    } 
}
