using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public CallResult Result { get; private set; } = CallResult.None;
        public ProxyCall Returns<T>() => Returns(typeof(T));
        public ProxyCall ReturnsAsync<T>() => Returns(typeof(Task<T>));
        public ProxyCall Returns(Type type) => With(result: new CallResult(type));

        public CallMethod Method { get; private set; } = CallMethod.Undefined;
        public ProxyCall Define(string description) =>
            Define(new DescriptionAttribute(description));
        public ProxyCall Define<TAttribute>() where TAttribute : Attribute, new() =>
            Define(new TAttribute());
        public ProxyCall Define(IEnumerable<Attribute> attributes) =>
            Define(attributes.ToArray());
        public ProxyCall Define(params Attribute[] attributes) => 
            With(method: new CallMethod(attributes.Concat(Method)));
        
        ProxyCall With(CallResult result = null, CallMethod method = null) => 
            new ProxyCall(Name, Args) 
            { 
                Result = result ?? Result,
                Method = method ?? Method
            };
    }
}
