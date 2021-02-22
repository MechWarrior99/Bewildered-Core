using System;

namespace Bewildered
{
    public class Accessor
    {
        public Func<object, object> Get { get; protected set; }
        public Action<object, object> Set { get; protected set; }

        public string Name { get; protected set; }

        public object this[object instance]
        {
            get
            {
                if (instance == null)
                    throw new ArgumentNullException();

                return Get(instance);
            }
            set
            {
                if (instance == null)
                    throw new ArgumentNullException();

                Set(instance, value);
            }
        }

        protected Accessor()
        {

        }

        public Accessor(string name, Func<object, object> get, Action<object, object> set)
        {
            Name = name;
            Get = get;
            Set = set;
        }
    }

    public class Accessor<TInstance, TValue> : Accessor
    {
        public new Func<TInstance, TValue> Get { get; protected set; }

        public new Action<TInstance, TValue> Set { get; protected set; }

        public TValue this[TInstance instance]
        {
            get
            {
                if (instance == null)
                    throw new ArgumentNullException();

                return Get(instance);
            }
            set
            {
                if (instance == null)
                    throw new ArgumentNullException();

                Set(instance, value);
            }
        }


        protected Accessor()
        {

        }

        public Accessor(string name, Func<TInstance, TValue> get, Action<TInstance, TValue> set)
        {
            Name = name;
            Get = get;
            Set = set;
        }
    }
}
