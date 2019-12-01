using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Dynoproxy
{
    public class ProxyCall
    {
        public ProxyCall(string name, IEnumerable<object> args)
            : this(name, args.ToArray())
        {
        }

        public ProxyCall(string name, params object[] args)
        {
            Name = name;
            Args = args;
        }

        public string Name { get; }
        public IReadOnlyList<object> Args { get; }

        public Type Result { get; private set; } = typeof(void);
        public ProxyCall Returns<T>() => Returns(typeof(T));
        public ProxyCall Returns(Type type) => With(result: type);

        IReadOnlyList<Attribute> Attributes { get; set; } = new Attribute[0];
        public bool Contains<TAttribute>() where TAttribute : Attribute =>
            Select<TAttribute>().Any();
        public T Peek<TAttribute, T>(Func<TAttribute, T> selector) where TAttribute : Attribute =>
            Select(selector).FirstOrDefault();
        public IEnumerable<Attribute> Select<TAttribute>() where TAttribute : Attribute =>
            Select((TAttribute a) => a);
        public IEnumerable<T> Select<TAttribute, T>(Func<TAttribute, T> selector) where TAttribute : Attribute =>
            Attributes.OfType<TAttribute>().Select(selector);

        public string Description => Peek((DescriptionAttribute a) => a.Description);

        public ProxyCall Define(string description) =>
            Define(new DescriptionAttribute(description));
        public ProxyCall Define<TAttribute>() where TAttribute : Attribute, new() =>
            Define(new TAttribute());
        public ProxyCall Define(IEnumerable<Attribute> attributes) =>
            Define(attributes.ToArray());
        public ProxyCall Define(params Attribute[] attributes) => 
            With(attributes: attributes.Concat(Attributes).ToArray());
        
        ProxyCall With(Type result = null, IReadOnlyList<Attribute> attributes = null) => 
            new ProxyCall(Name, Args) 
            { 
                Result = result ?? Result,
                Attributes = attributes ?? Attributes
            };
    }
}
