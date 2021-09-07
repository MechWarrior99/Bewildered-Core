using System;
using System.Reflection;

namespace Bewildered
{
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
