using System;
using UnityEngine;

namespace Bewildered
{
    /// <summary>
    /// A stable, Globally Unique Identifier. Instances of prefabs will have the same Id as the source prefab, thus making it not unique for them.
    /// </summary>
    [Serializable]
    public struct UniqueId : ISerializationCallbackReceiver
    {
        private Guid _guid;

        // Use bytes because strings allocate memory and is twice as slow.
        [SerializeField] private byte[] _serializedGuid;

        public static readonly UniqueId Empty = new UniqueId() { _guid = Guid.Empty, _serializedGuid = null };

        public UniqueId(string id)
        {
            _guid = new Guid(id);
            _serializedGuid = _guid.ToByteArray();
        }

        public static UniqueId NewUniqueId()
        {
            UniqueId id = new UniqueId();
            id._guid = Guid.NewGuid();
            id._serializedGuid = id._guid.ToByteArray();
            return id;
        }

        public static bool operator ==(UniqueId lhs, UniqueId rhs)
        {
            return lhs._guid == rhs._guid;
        }

        public static bool operator !=(UniqueId lhs, UniqueId rhs)
        {
            return lhs._guid != rhs._guid;
        }

        public override bool Equals(object obj)
        {
            if (obj is UniqueId id && _guid.Equals(id._guid))
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public override string ToString()
        {
            return _guid.ToString();
        }


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_guid != Guid.Empty)
            {
                _serializedGuid = _guid.ToByteArray();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_serializedGuid != null && _serializedGuid.Length == 16)
            {
                _guid = new Guid(_serializedGuid);
            }
        }
    }

}