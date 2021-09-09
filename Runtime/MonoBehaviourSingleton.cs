using System;
using UnityEngine;

namespace Bewildered
{
    /// <summary>
    /// Generic base class for <see cref="MonoBehaviour"/> based Singletons.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="MonoBehaviour"/> that inherits from the <see cref="MonoBehaviourSingleton{T}"/>.</typeparam>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly Lazy<T> _lazyInstance = new Lazy<T>(CreateSingleton);

        /// <summary>
        /// Returns the instance of the Singleton. Creates the Singleton instance if one does not already exist in the scene.
        /// </summary>
        public static T Instance
        {
            get { return _lazyInstance.Value; }
        }

        private static T CreateSingleton()
        {
            var instance = GameObject.FindObjectOfType<T>();
            if (instance)
                return instance;

            var ownerObject = new GameObject($"{typeof(T).Name} Singleton");
            DontDestroyOnLoad(ownerObject);
            instance = ownerObject.AddComponent<T>();
            return instance;
        }
    }
}
