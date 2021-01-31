using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bewildered
{
    [Serializable]
    public class UDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keysList = new List<TKey>();
        [SerializeField] private List<TValue> _valuesList = new List<TValue>();
        [NonSerialized] private bool _doSerializeToList = true;


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_doSerializeToList)
            {
                _keysList.Clear();
                _valuesList.Clear();
                _keysList.AddRange(Keys);
                _valuesList.AddRange(Values);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < _keysList.Count; i++)
            {
                if (!ContainsKey(_keysList[i]))
                    Add(_keysList[i], _valuesList[i]);
            }
        }
    } 
}
