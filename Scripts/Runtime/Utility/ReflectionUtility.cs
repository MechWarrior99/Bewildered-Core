using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bewildered
{
    public static class ReflectionUtility
    {
        public const BindingFlags CommonFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static T CreateDelegate<T>(this MethodInfo method) where T : Delegate
        {
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }
    } 
}
