using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Bewildered
{
    // https://stackoverflow.com/questions/38528620/c-sharp-fieldinfo-reflection-alternatives
    /// <summary>
    /// Used to access a field without reflection beyond initial construction. Use <c>nameof()</c> to get the name of a field.
    /// </summary>
    public class FieldAccessor : Accessor
    {
        private static readonly ParameterExpression _fieldParameter = Expression.Parameter(typeof(object));
        private static readonly ParameterExpression _ownerParameter = Expression.Parameter(typeof(object));

        public FieldAccessor(Type type, string fieldName)
        {
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
                throw new ArgumentException();

            Name = fieldInfo.Name;

            var fieldExpression = Expression.Field(
                Expression.Convert(_ownerParameter, type),
                fieldInfo);

            Get = Expression.Lambda<Func<object, object>>(
                Expression.Convert(fieldExpression, typeof(object)),
                _ownerParameter).Compile();

            Set = Expression.Lambda<Action<object, object>>(
                Expression.Assign(fieldExpression,
                    Expression.Convert(_fieldParameter, fieldInfo.FieldType)),
                _ownerParameter, _fieldParameter).Compile();
        }

        public FieldAccessor(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentException();

            Name = fieldInfo.Name;

            var fieldExpression = Expression.Field(
                Expression.Convert(_ownerParameter, fieldInfo.DeclaringType),
                fieldInfo);

            Get = Expression.Lambda<Func<object, object>>(
                Expression.Convert(fieldExpression, typeof(object)),
                _ownerParameter).Compile();

            Set = Expression.Lambda<Action<object, object>>(
                Expression.Assign(fieldExpression,
                    Expression.Convert(_fieldParameter, fieldInfo.FieldType)),
                _ownerParameter, _fieldParameter).Compile();
        }
    }

    /// <inheritdoc/>
    /// <typeparam name="T">The type of the field to access.</typeparam>
    public class FieldAccessor<T> : FieldAccessor
    {
        public FieldAccessor(string fieldName) : base(typeof(T), fieldName)
        {

        }
    }
}
