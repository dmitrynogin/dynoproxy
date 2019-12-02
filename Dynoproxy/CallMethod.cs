using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Dynoproxy
{
    public class CallMethod : ReadOnlyCollection<Attribute>
    {
        public static readonly CallMethod Undefined = new CallMethod();

        internal CallMethod(IEnumerable<Attribute> attributes) 
            : this(attributes.ToArray())
        {
        }

        internal CallMethod(params Attribute[] attributes)
            : base(attributes)
        {
        }

        public bool Contains<TAttribute>() where TAttribute : Attribute =>
            Select<TAttribute>().Any();
        public T Peek<TAttribute, T>(Func<TAttribute, T> selector) where TAttribute : Attribute =>
            Select(selector).FirstOrDefault();
        public IEnumerable<Attribute> Select<TAttribute>() where TAttribute : Attribute =>
            Select((TAttribute a) => a);
        public IEnumerable<T> Select<TAttribute, T>(Func<TAttribute, T> selector) where TAttribute : Attribute =>
            Items.OfType<TAttribute>().Select(selector);

        public string Description => Peek((DescriptionAttribute a) => a.Description);
    }
}
