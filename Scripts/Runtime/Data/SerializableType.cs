using System;
using UnityEngine;

namespace Bewildered
{
    [Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        private Type _type;

        [SerializeField] private string _typeName;

        public Type Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public SerializableType()
        {

        }

        public SerializableType(Type type)
        {
            _type = type;
        }

        public SerializableType(string typeName)
        {
            _typeName = typeName;
            _type = Type.GetType(_typeName);
        }

        public static implicit operator Type(SerializableType serializableType)
        {
            return serializableType.Type;
        }

        public static implicit operator SerializableType(Type type)
        {
            return new SerializableType(type);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _typeName = _type.AssemblyQualifiedName;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _type = Type.GetType(_typeName);
        }
    } 
}
