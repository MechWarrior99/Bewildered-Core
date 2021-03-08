using System;
using System.Reflection;

namespace Bewildered
{
    public static class ReflectionUtility
    {
        public const BindingFlags CommonFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Creates a delegate of the specified type to represent the specified stat method.
        /// </summary>
        /// <typeparam name="T">The type of delegate to create.</typeparam>
        /// <returns>A delegate of the specified type that represents the specified method.</returns>
        public static T CreateDelegate<T>(this MethodInfo method) where T : Delegate
        {
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }
    } 
}
