using System;
using UnityEngine;

namespace Bewildered
{
    /// <summary>
    /// Base class for <see cref="MonoBehaviour"/> based singletons.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly Lazy<T> LazyInstance = new Lazy<T>(CreateSingleton);

        public static T Instance
        {
            get { return LazyInstance.Value; }
        }

        private static T CreateSingleton()
        {
            GameObject ownerObject = new GameObject($"{typeof(T).Name} (singleton)");
            T instance = ownerObject.AddComponent<T>();
            DontDestroyOnLoad(instance);
            return instance;
        }
    } 
}
