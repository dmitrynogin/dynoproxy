using Castle.DynamicProxy;
using Dynamitey;
using System;

namespace Dynoproxy
{
    public static class Proxy
    {
        public static T Create<T>(this object source) where T : class
        {
            var proxyGenerator = new ProxyGenerator();
            return proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(
                ProxyGenerationOptions.Default,
                new Interceptor(source));
        }

        class Interceptor : IInterceptor
        {
            public Interceptor(object target) => Target = target;
            object Target { get; }
            public void Intercept(IInvocation invocation) =>
                invocation.ReturnValue = Dynamic.InvokeMember(Target, invocation.Method.Name, invocation.Arguments);
        }
    }
}
