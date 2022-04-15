using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bewildered
{
    /// <summary>
    /// Represents a serializable collection of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the <see cref="UDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The type of the values in the <see cref="UDictionary{TKey, TValue}"/>.</typeparam>
    [Serializable]
    public class UDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct SerializableKeyValuePair
        {
            public TKey key;
            public TValue value;
            public bool isDuplicateKey;
            public int index;

            public SerializableKeyValuePair(KeyValuePair<TKey, TValue> pair)
            {
                key = pair.Key;
                value = pair.Value;
                isDuplicateKey = false;
                index = -1;
            }
        }

#if UNITY_EDITOR
        // Set to `true` exclusivally in the property drawer. Resets to false when the domain reloads.
        [NonSerialized] private bool _saveDuplicates = false;
#endif
        [SerializeField] private List<SerializableKeyValuePair> _serializedPairs = new List<SerializableKeyValuePair>();


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
                for (int i = _serializedPairs.Count - 1; i >= 0; i--)
                {
                    if (!_serializedPairs[i].isDuplicateKey)
                        _serializedPairs.RemoveAt(i);
                }
            }
            else
            {
                _serializedPairs.Clear();
            }

            // When serializing the values of the dictionary to the list, we need to add the items to the dictionary
            // in positions reletive to the duplicate key pairs. Otherwise the items will move around in the inspector
            // while you are changing their keys (for example if you change it to/from being a duplicate key.
            int index = 0;
            bool hasDuplicateKeys = _serializedPairs.Count > 0;
            int nextDuplicateKeyIndex = hasDuplicateKeys ? _serializedPairs[0].index : -1;

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                if (hasDuplicateKeys)
                {
                    if (index < nextDuplicateKeyIndex)
                    {
                        _serializedPairs.Insert(index, new SerializableKeyValuePair(pair) { index = index });
                    }
                    else if (index > nextDuplicateKeyIndex)
                    {
                        // This will happen when there are more pairs after the last duplicate key pair.
                        _serializedPairs.Add(new SerializableKeyValuePair(pair) { index = index });
                    }
                    else if (index == nextDuplicateKeyIndex)
                    {
                        // We increment the index until the index is different than that of a duplicate key pair's index.
                        while (index == nextDuplicateKeyIndex && index < _serializedPairs.Count)
                        {
                            index++;
                            if (index < _serializedPairs.Count)
                                nextDuplicateKeyIndex = _serializedPairs[index].index;
                        }
                        _serializedPairs.Insert(index, new SerializableKeyValuePair(pair) { index = index });
                    }
                }
                else
                {
                    _serializedPairs.Add(new SerializableKeyValuePair(pair) { index = index });
                }

                index++;
            }
#else
            _serializedPairs.Clear();
            foreach (var pair in this)
            {
                _serializedPairs.Add(new SerializableKeyValuePair(pair));
            }
#endif
        }

        // Add the serialized data from the list to the Dictionary.
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
#if UNITY_EDITOR
            for (int i = 0; i < _serializedPairs.Count; i++)
            {
                // We make sure that each serialized pair has it's proper index value.
                // For example, when reordering with a ReorderableList in the inspector, they will become out of sync.
                var pair = _serializedPairs[i];
                pair.index = i;
                _serializedPairs[i] = pair;

                if (!ContainsKey(_serializedPairs[i].key))
                {
                    Add(_serializedPairs[i].key, _serializedPairs[i].value);
                    SetPairDuplicatedState(i, false);
                }
                else
                {
                    SetPairDuplicatedState(i, true);
                }
            }
#else
            foreach (var pair in _serializedPairs)
            {
                this[pair.key] = pair.value;
            }
#endif


        }

#if UNITY_EDITOR
        private void SetPairDuplicatedState(int index, bool isDuplicateKey)
        {
            var pair = _serializedPairs[index];
            pair.isDuplicateKey = isDuplicateKey;
            _serializedPairs[index] = pair;
        }
#endif
    } 
}
