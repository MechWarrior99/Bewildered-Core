using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bewildered
{
    // Much is copied from the AccessTools class from the Harmony library: https://github.com/pardeike/Harmony/blob/master/Harmony/Tools/AccessTools.cs
    /// <summary>
    /// A helper class for reflection related functions.
    /// </summary>
    public static class AccessUtility
    {
        /// <summary>
        /// Shortcut for <see cref="BindingFlags"/> to simplify the use of reflections and make it work for any access level.
        /// </summary>
        public const BindingFlags All = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

        /// <summary>
        /// Enumerates all assemblies in the current app domain.
        /// </summary>
        /// <returns>An enumeration of <see cref="Assembly"/>.</returns>
        public static IEnumerable<Assembly> AllAssemblies()
        {
            // We want to ignore the visual studio assemblies.
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Microsoft.VisualStudio") is false);
        }

        /// <summary>
        /// Enumerates all successfully loaded types in the current app domain.
        /// </summary>
        /// <returns>An enumeration of all <see cref="Type"/> in all assemblies.</returns>
        public static IEnumerable<Type> AllTypes()
        {
            return AllAssemblies().SelectMany(a => a.GetTypes());
        }

        /// <summary>
        /// Returns a <see cref="Type"/> whose name matches the specified name. 
        /// Prefers if the specified name includes the namespace, if not it match the first type with the same name. 
        /// </summary>
        /// <param name="typeName">The name of the <see cref="Type"/> to get.</param>
        /// <returns>A <see cref="Type"/> with a matching name to <paramref name="typeName"/>; If one is not found, <c>null</c>.</returns>
        public static Type TypeByName(string typeName)
        {
            Type type = Type.GetType(typeName);

            if (type == null && typeName.Contains('.'))
                type = AllTypes().FirstOrDefault(t => t.FullName == typeName);
            
            if (type == null)
                type = AllTypes().FirstOrDefault(t => t.Name == typeName);

            return type;
        }

        /// <summary>
        /// Gets the reflection information for a field.
        /// </summary>
        /// <param name="type">The type where the field is defined.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The <see cref="FieldInfo"/> for the field on <paramref name="type"/> with <paramref name="name"/>.</returns>
        public static FieldInfo Field(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException("Type is null.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Name is null.");

            return FindMemberIncludingBaseTypes(type, t => t.GetField(name, All));
        }

        /// <summary>
        /// Gets the reflection information for a property.
        /// </summary>
        /// <param name="type">The type where the property is defined.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The <see cref="PropertyInfo"/> for the property on <paramref name="type"/> with <paramref name="name"/>.</returns>
        public static PropertyInfo Property(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException("Type is null.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Name is null.");

            return FindMemberIncludingBaseTypes(type, t => t.GetProperty(name, All));
        }

        /// <summary>
        /// Gets the reflection information for a method.
        /// </summary>
        /// <param name="type">The type where the method is defined.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">Optional parameters to target a specific overload of the method</param>
        /// <returns>The <see cref="MethodInfo"/> for a method on <paramref name="type"/>.</returns>
        public static MethodInfo Method(Type type, string name, params Type[] parameters)
        {
            if (type == null)
                throw new ArgumentNullException("Type is null.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Name is null.");

            return FindMemberIncludingBaseTypes(type, t => t.GetMethod(name, All, null, parameters, new ParameterModifier[0]));
        }

        private static T FindMemberIncludingBaseTypes<T>(Type type, Func<Type, T> func) where T : class
        {
            for (Type currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                T member = func(currentType);
                if (member != null)
                    return member;
            }

            return null;
        }
    }
}
