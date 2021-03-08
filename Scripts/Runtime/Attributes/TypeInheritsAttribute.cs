using System;

namespace Bewildered
{
    /// <summary>
    /// Limits the <see cref="System.Type"/>'s that can be selected and assigned to the field in the editor, to only those that inherit from the specified <see cref="System.Type"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TypeInheritsAttribute : Attribute
    {
        public Type Type { get; }

        public TypeInheritsAttribute(Type type)
        {
            Type = type;
        }
    }

}