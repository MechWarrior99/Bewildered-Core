using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bewildered
{
    // Implementation Note:
    // We do not inheirt from HashShet<T> because in the ISerializationCallbackReceiver.OnBeforeSerialize() method
    // we need to iterate over the items in the HashSet but for unknown
    // reasons the enumerator MoveNext() will result in a null reference exception.

    // Currently my suspicion is that Unity may actually be serializing some of the fields
    // in the HashSet, and that is causing the exception, however I'm not sure.

    /// <summary>
    /// Represents a serializable set of values.
    /// </summary>
    /// <typeparam name="T">The type of item in the <see cref="UHashSet{T}"/>.</typeparam>
    [Serializable]
    public class UHashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IReadOnlyCollection<T>, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct SerializableValue
        {
            public T value;
            public bool isDuplicate;
            public int index;

            public SerializableValue(T value)
            {
                this.value = value;
                isDuplicate = false;
                index = -1;
            }
        }

#if UNITY_EDITOR
        // Set to `true` exclusivally in the property drawer. Resets to false when the domain reloads.
        [NonSerialized] private bool _saveDuplicates = false;
#endif
        [NonSerialized] private readonly HashSet<T> _hashSet = new HashSet<T>();
        [SerializeField] private List<SerializableValue> _serializedValues = new List<SerializableValue>();

        /// <summary>
        /// Gets the number of items contained in the <see cref="UHashSet{T}"/>.
        /// </summary>
        public int Count
        {
            get { return _hashSet.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return ((ICollection<T>)_hashSet).IsReadOnly; }
        }

        public UHashSet()
        {
            _hashSet = new HashSet<T>();
        }

        public UHashSet(IEnumerable<T> collection)
        {
            _hashSet = new HashSet<T>(collection);
        }

        public UHashSet(IEqualityComparer<T> comparer)
        {
            _hashSet = new HashSet<T>(comparer);
        }

        public UHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _hashSet = new HashSet<T>(collection, comparer);
        }

        /// <summary>
        /// Adds the specified item to the <see cref="UHashSet{T}"/>.
        /// </summary>
        /// <param name="item">The item to add to the <see cref="UHashSet{T}"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> is added to the <see cref="UHashSet{T}"/>; 
        /// otherwise <c>false</c> if <paramref name="item"/> is already present.
        /// </returns>
        public bool Add(T item)
        {
            return _hashSet.Add(item);
        }

        void ICollection<T>.Add(T item)
        {
            _hashSet.Add(item);
        }

        /// <summary>
        /// Removes the specified item from the <see cref="UHashSet{T}"/>.
        /// </summary>
        /// <param name="item">The <see cref="T"/> to remove.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> is successfully found and removed; otherwise, <c>false</c>.
        /// This method returns <c>false</c> if <paramref name="item"/> is not found in the <see cref="UHashSet{T}"/>
        /// </returns>
        public bool Remove(T item)
        {
            return _hashSet.Remove(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="UHashSet{T}"/>.
        /// </summary>
        public void Clear()
        {
            _hashSet.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="UHashSet{T}"/> contains the specified item.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="UHashSet{T}"/>.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <see cref="UHashSet{T}"/>; otherwise <c>false</c>.</returns>
        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }

        /// <summary>
        /// Removes all items in the specified collection from the <see cref="UHashSet{T}"/>.
        /// </summary>
        /// <param name="other">The collection of items to remove from the <see cref="UHashSet{T}"/>.</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            _hashSet.ExceptWith(other);
        }

        /// <summary>
        /// Modifies the <see cref="UHashSet{T}"/> to contain only itemss that are present in both the <see cref="UHashSet{T}"/> and the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            _hashSet.IntersectWith(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashSet{T}"/> is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashSet{T}"/> is a proper subset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSubsetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashSet{T}"/> is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashSet{T}"/> is a proper superset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSupersetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashSet{T}"/> is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashSet{T}"/> is a subset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSubsetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashSet{T}"/> is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashSet{T}"/> is a superset of <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSupersetOf(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashSet{T}"/> and the specified collection share common items.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="UHashSet{T}"/> and <paramref name="other"/> share at least one common item; otherwise, <c>false</c>.
        /// </returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            return _hashSet.Overlaps(other);
        }

        /// <summary>
        /// Determines whether the <see cref="UHashSet{T}"/> and the specified collection contain the same items.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        /// <returns><c>true</c> if the <see cref="UHashSet{T}"/> and <paramref name="other"/> contain the same items; other <c>falase</c>.</returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            return _hashSet.SetEquals(other);
        }

        /// <summary>
        /// Modifies the <see cref="UHashSet{T}"/> to contain only items that are present either in the <see cref="UHashSet{T}"/> or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _hashSet.SymmetricExceptWith(other);
        }

        /// <summary>
        /// Modifies the <see cref="UHashSet{T}"/> to contain all items that are present in itself, the specified collection, or both.
        /// </summary>
        /// <param name="other">The collection to compare to the <see cref="UHashSet{T}"/>.</param>
        public void UnionWith(IEnumerable<T> other)
        {
            _hashSet.UnionWith(other);
        }

        /// <summary>
        /// Copies the items of the <see cref="UHashSet{T}"/> to an array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the items copied from the <see cref="UHashSet{T}"/>.</param>
        public void CopyTo(T[] array)
        {
            _hashSet.CopyTo(array);
        }

        /// <summary>
        /// Copies the items of the <see cref="UHashSet{T}"/> to an array, starting at a specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the items copied from the <see cref="UHashSet{T}"/>.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _hashSet.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="UHashSet{T}"/>.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _hashSet.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="UHashSet{T}"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_hashSet).GetEnumerator();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // _saveDuplicates is set to false when the domain reloads. This will cause all of the serialized pairs with duplicate keys
            // to be removed, since we just call Clear() on the serialized pairs.
            // It would be nice if there was a way to check when the property drawer was no longer being used, and clear the duplicates then,
            // but that does not seem possible at this time.
            if (_saveDuplicates)
            {
                // Remove all serialized pairs that do *not* collide since they will already be in the dictionary
                // unless they were removed from it intentionally.
                for (int i = _serializedValues.Count - 1; i >= 0; i--)
                {
                    if (!_serializedValues[i].isDuplicate)
                        _serializedValues.RemoveAt(i);
                }
            }
            else
            {
                _serializedValues.Clear();
            }

            // When serializing the values of the dictionary to the list, we need to add the items to the dictionary
            // in positions reletive to the duplicate key pairs. Otherwise the items will move around in the inspector
            // while you are changing their keys (for example if you change it to/from being a duplicate key.
            int index = 0;
            bool hasDuplicateValues = _serializedValues.Count > 0;
            int nextDuplicateValueIndex = hasDuplicateValues ? _serializedValues[0].index : -1;

            foreach (T value in this)
            {
                if (hasDuplicateValues)
                {
                    if (index < nextDuplicateValueIndex)
                    {
                        _serializedValues.Insert(index, new SerializableValue(value) { index = index });
                    }
                    else if (index > nextDuplicateValueIndex)
                    {
                        // This will happen when there are more values after the last duplicate value.
                        _serializedValues.Add(new SerializableValue(value) { index = index });
                    }
                    else if (index == nextDuplicateValueIndex)
                    {
                        // We increment the index until the index is different than that of a duplicate key pair's index.
                        while (index == nextDuplicateValueIndex && index < _serializedValues.Count)
                        {
                            index++;
                            if (index < _serializedValues.Count)
                                nextDuplicateValueIndex = _serializedValues[index].index;
                        }
                        _serializedValues.Insert(index, new SerializableValue(value) { index = index });
                    }
                }
                else
                {
                    _serializedValues.Add(new SerializableValue(value) { index = index });
                }

                index++;
            }
#else
            _serializedValues.Clear();

            foreach (var value in this)
            {
                _serializedValues.Add(new SerializableValue(value));
            }
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
#if UNITY_EDITOR
            for (int i = 0; i < _serializedValues.Count; i++)
            {
                // We make sure that each serialized item has it's proper index value.
                // For example, when reordering with a ReorderableList in the inspector, they will become out of sync.
                var pair = _serializedValues[i];
                pair.index = i;
                _serializedValues[i] = pair;

                if (!Contains(_serializedValues[i].value))
                {
                    Add(_serializedValues[i].value);
                    SetValueDuplicatedState(i, false);
                }
                else
                {
                    SetValueDuplicatedState(i, true);
                }
            }
#else
            foreach (var serializedValue in _serializedValues)
            {
                Add(serializedValue.value);
            }
#endif
        }

#if UNITY_EDITOR
        private void SetValueDuplicatedState(int index, bool isDuplicate)
        {
            var pair = _serializedValues[index];
            pair.isDuplicate = isDuplicate;
            _serializedValues[index] = pair;
        }
#endif
    }
}
