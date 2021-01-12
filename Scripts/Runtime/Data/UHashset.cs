using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bewildered.Core
{
    /// <summary>
    /// Represents a set of values.
    /// </summary>
    /// <remarks>A serializable <see cref="HashSet{T}"/> implementation.</remarks>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class UHashset<T> : ISet<T>, ICollection<T>, IEnumerable<T>, ISerializationCallbackReceiver
    {
        private readonly HashSet<T> _hashset = new HashSet<T>();
        [SerializeField] private List<T> _hashsetList = new List<T>();
        [NonSerialized] private bool _doSerializeToHashset = true;

        /// <summary>
        /// Gets the number of <see cref="T"/> contained in the <see cref="UHashset{T}"/>.
        /// </summary>
        public int Count
        {
            get { return _hashset.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return ((ICollection<T>)_hashset).IsReadOnly; }
        }

        public UHashset()
        {
            _hashset = new HashSet<T>();
        }

        public UHashset(IEnumerable<T> collection)
        {
            _hashset = new HashSet<T>(collection);
        }

        public UHashset(IEqualityComparer<T> comparer)
        {
            _hashset = new HashSet<T>(comparer);
        }

        public UHashset(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _hashset = new HashSet<T>(collection, comparer);
        }

        /// <summary>
        /// Adds the specified item to the <see cref="UHashset{T}"/>.
        /// </summary>
        /// <param name="item">The item to add to the <see cref="UHashset{T}"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> is added to the <see cref="UHashset{T}"/>; 
        /// otherwise <c>false</c> if <paramref name="item"/> is already present.
        /// </returns>
        public bool Add(T item)
        {
            return _hashset.Add(item);
        }

        void ICollection<T>.Add(T item)
        {
            _hashset.Add(item);
        }

        /// <summary>
        /// Removes the specified item from the <see cref="UHashset{T}"/>.
        /// </summary>
        /// <param name="item">The <see cref="T"/> to remove.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is successfully found and removed; otherwise, <c>false</c>. This method returns <c>false</c> if <paramref name="item"/> is not found in the <see cref="UHashset{T}"/></returns>
        public bool Remove(T item)
        {
            return _hashset.Remove(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="UHashset{T}"/>.
        /// </summary>
        public void Clear()
        {
            _hashset.Clear();
        }

        /// <summary>
        /// Determins whether the <see cref="UHashset{T}"/> contains the specified item.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="UHashset{T}"/>.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <see cref="UHashset{T}"/>; otherwise <c>false</c>.</returns>
        public bool Contains(T item)
        {
            return _hashset.Contains(item);
        }

        /// <summary>
        /// Removes all items in the specified collection from the <see cref="UHashset{T}"/>.
        /// </summary>
        /// <param name="other">The collection of items to remove from the <see cref="UHashset{T}"/>.</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            _hashset.ExceptWith(other);
        }

        /// <summary>
        /// Modifies the <see cref="UHashset{T}"/> to contain only itemss that are present in both the <see cref="UHashset{T}"/> and the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            _hashset.IntersectWith(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashset{T}"/> is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashset{T}"/> is a proper subset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _hashset.IsProperSubsetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashset{T}"/> is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashset{T}"/> is a proper superset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _hashset.IsProperSupersetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashset{T}"/> is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashset{T}"/> is a subset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _hashset.IsSubsetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashset{T}"/> is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashset{T}"/> is a superset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _hashset.IsSupersetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashset{T}"/> and the specified collection share common items.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashset{T}"/> and <paramref name="other"/> share at least one common item; otherwise, <c>false</c>.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            return _hashset.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _hashset.SetEquals(other);
        }

        /// <summary>
        /// Modifies the <see cref="UHashset{T}"/> to contain only items that are present either in the <see cref="UHashset{T}"/> or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _hashset.SymmetricExceptWith(other);
        }

        /// <summary>
        /// Modifies the <see cref="UHashset{T}"/> to contain all items that are present in itself, the specified collection, or both.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashset{T}"/>.</param>
        public void UnionWith(IEnumerable<T> other)
        {
            _hashset.UnionWith(other);
        }

        /// <summary>
        /// Copies the items of the <see cref="UHashset{T}"/> to an array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the items copied from the <see cref="UHashset{T}"/>.</param>
        public void CopyTo(T[] array)
        {
            _hashset.CopyTo(array);
        }

        /// <summary>
        /// Copies the items of the <see cref="UHashset{T}"/> to an array, starting at a specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the items copied from the <see cref="UHashset{T}"/>.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _hashset.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="UHashset{T}"/>.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _hashset.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="UHashset{T}"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_hashset).GetEnumerator();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_doSerializeToHashset)
            {
                _hashsetList.Clear();
                _hashsetList.AddRange(_hashset); 
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _hashset.Clear();
            foreach (var hashtag in _hashsetList)
            {
                _hashset.Add(hashtag);
            }
        }
    } 
}
