using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dynoproxy
{
    public class CallResult 
    {
        public static readonly CallResult None = new CallResult(typeof(void));

        internal CallResult(Type raw) => Raw = raw;
        public Type Raw { get; }
        public bool Sync => !Async;
        public bool Async => typeof(Task).IsAssignableFrom(Raw);
        public bool Void => Raw == typeof(void) || Raw == typeof(Task);
        public Type Type => Async 
            ? (Void ? typeof(void) : Raw.GetGenericArguments()[0])
            : Raw;
    }
}
