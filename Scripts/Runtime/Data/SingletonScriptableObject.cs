using System.Linq;
using UnityEngine;

namespace Bewildered
{
    /// <summary>
    /// A class you can derive from if you want to create singleton objects that don't need to be attached to game objects. Singleton asset must be in Resources folder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<T>(typeof(T).Name);

                return _instance;
            }
        }

        public static bool HasInstance()
        {
            return Instance != null;
        }
    } 
}
