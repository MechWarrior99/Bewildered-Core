using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Bewildered
{
    // https://stackoverflow.com/questions/38528620/c-sharp-fieldinfo-reflection-alternatives
    /// <summary>
    /// Used to access a field or property without reflection beyond initial construction. Use <c>nameof()</c> to get the name of a field.
    /// </summary>
    public class VariableAccessor : Accessor
    {
        private static readonly ParameterExpression _variableParameter = Expression.Parameter(typeof(object));
        private static readonly ParameterExpression _ownerParameter = Expression.Parameter(typeof(object));

        public VariableAccessor(Type type, string variableName)
        {
            Type variableType;
            FieldInfo fieldInfo = type.GetField(variableName, ReflectionUtility.CommonFlags);
            if (fieldInfo == null)
            {
                PropertyInfo propertyInfo = type.GetProperty(variableName, ReflectionUtility.CommonFlags);
                if (propertyInfo == null)
                    throw new ArgumentException();

                variableType = propertyInfo.PropertyType;
            }
            else
            {
                variableType = fieldInfo.FieldType;
            }

            Name = variableName;

            MemberExpression fieldExpression = Expression.PropertyOrField(
                Expression.Convert(_ownerParameter, type),
                variableName);

            Get = Expression.Lambda<Func<object, object>>(
                Expression.Convert(fieldExpression, typeof(object)),
                _ownerParameter).Compile();

            Set = Expression.Lambda<Action<object, object>>(
                Expression.Assign(fieldExpression,
                    Expression.Convert(_variableParameter, variableType)),
                _ownerParameter, _variableParameter).Compile();
        }
    }

    public class VariableAccessor<S> : VariableAccessor
    {
        public VariableAccessor(string variableName) : base(typeof(S), variableName) { }
    }
}
