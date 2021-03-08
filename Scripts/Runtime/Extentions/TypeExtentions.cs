using System;
using System.Collections.Generic;
using System.Linq;

namespace Bewildered
{
    public static class TypeExtentions
    {
        private static readonly Dictionary<Type, string> _typeToFriendlyName = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(sbyte), "sbyte" },
            { typeof(float), "float" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };

        
        /// <summary>
        /// Get the <see cref="Type"/>'s name that looks nice if it is generic.
        /// </summary>
        public static string GetFriendlyName(this Type type)
        {
            return GetFriendlyNameInternal(type, type.Name);
        }

        /// <summary>
        /// Get the <see cref="Type"/>'s full name that looks nice if it is generic.
        /// </summary>
        public static string GetFriendlyFullName(this Type type)
        {
            return GetFriendlyNameInternal(type, type.FullName);
        }

        // https://stackoverflow.com/a/33529925
        private static string GetFriendlyNameInternal(Type type, string name)
        {
            string friendlyName;
            if (_typeToFriendlyName.TryGetValue(type, out friendlyName))
                return friendlyName;

            friendlyName = name;

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return type.GetGenericArguments().First().GetFriendlyName() + "?";

                int backtickIndex = friendlyName.IndexOf('`');
                if (backtickIndex > 0)
                    friendlyName = friendlyName.Remove(backtickIndex);

                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; i++)
                {
                    string typeParamName = typeParameters[i].GetFriendlyName();
                    friendlyName += (i == 0 ? typeParamName : ", " + typeParamName);
                }
                friendlyName += ">";
            }

            if (type.IsArray)
            {
                return type.GetElementType().GetFriendlyName() + "[]";
            }

            return friendlyName;
        }
    } 
}
