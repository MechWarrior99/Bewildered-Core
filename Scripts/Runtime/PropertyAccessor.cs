using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Bewildered
{
    public class PropertyAccessor<T> : Accessor
    {
        public bool IsReadable { get; private set; }
        public bool IsWritable { get; private set; }

        public PropertyAccessor(Expression<Func<T, object>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression body))
                throw new MissingMemberException("something went wrong");

            PropertyInfo prop = body.Member as PropertyInfo;
            IsReadable = prop.CanRead;
            IsWritable = prop.CanWrite;

            if (IsReadable)
                Get = prop.GetGetMethod().CreateDelegate<Func<object, object>>();

            if (IsWritable)
                Set = prop.GetSetMethod().CreateDelegate<Action<object, object>>();
        }
    } 
}
