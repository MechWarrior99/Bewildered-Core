using System;
using System.Collections;
using System.Collections.Generic;

namespace Bewildered.Editor
{
    /// <summary>
    /// Represents a collection of items that can be filtered by a string value.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the <see cref="FilteredList{T}"/>.</typeparam>
    public class FilteredList<T> : IEnumerable<FilteredList<T>.Entry>
    {
        public struct Entry
        {
            /// <summary>
            /// Index of the item in the unfiltered collection.
            /// </summary>
            public int Index;

            /// <summary>
            /// The entry item.
            /// </summary>
            public T Item;
        }

        /// <summary>
        /// The unfiltered items.
        /// </summary>
        public T[] AllItems { get; }

        /// <summary>
        /// The string used to filter items by.
        /// </summary>
        public string Filter { get; private set; }

        /// <summary>
        /// The list of items filtered by <see cref="Filter"/> with their index.
        /// </summary>
        public List<Entry> Entries { get; private set; } = new List<Entry>();

        /// <summary>
        /// The list of items filtered by <see cref="Filter"/>.
        /// </summary>
        public List<T> FilteredItems { get; private set; } = new List<T>();

        /// <summary>
        /// Function to get the string to filter by from an item.
        /// </summary>
        public Func<T, string> GetItemString { get; }

        /// <summary>
        /// Retrive an <see cref="Entry"/> in the filtered list by index.
        /// </summary>
        /// <param name="index">Index in the filtered list.</param>
        public Entry this[int index]
        {
            get { return Entries[index]; }
        }

        /// <param name="items">The items to filter.</param>
        /// <param name="getItemString"> Function to get the string to filter by from an item.</param>
        public FilteredList(T[] items, Func<T, string> getItemString)
        {
            AllItems = items;
            GetItemString = getItemString;
            UpdateFilter("");
        }

        /// <summary>
        /// Updates the <see cref="Filter"/>, and refilters <see cref="Entries"/> if there is achange in the filter value.
        /// </summary>
        /// <param name="filter">The new filter value.</param>
        /// <returns><c>true</c> if <paramref name="filter"/> is different than the current <see cref="Filter"/>; otherwise, <c>false</c>.</returns>
        public bool UpdateFilter(string filter)
        {
            if (filter == Filter)
                return false;

            Filter = filter;
            Entries.Clear();
            FilteredItems.Clear();

            for (int i = 0; i < AllItems.Length; i++)
            {
                string itemString = GetItemString(AllItems[i]);
                if (string.IsNullOrEmpty(Filter) || itemString.ToLower().Contains(Filter.ToLower()))
                {
                    Entry entry = new Entry()
                    {
                        Index = i,
                        Item = AllItems[i]
                    };

                    if (string.Equals(itemString, filter, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Entries.Insert(0, entry);
                        FilteredItems.Insert(0, AllItems[i]);
                    }
                    else
                    {
                        Entries.Add(entry);
                        FilteredItems.Add(AllItems[i]);
                    }
                }
            }

            return true;
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Entries.GetEnumerator();
        }
    } 
}
