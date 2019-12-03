using Castle.DynamicProxy;
using Dynamitey;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Dynoproxy
{
    public static class Proxy
    {
        public static T Create<T>(object target) where T : class =>
            Create<T>(call => Dynamic.InvokeMember(
                target, call.Name, call.Args.ToArray()));

        public static T Create<T>(string apiUrl, Func<HttpClient, Task> authenticate = null) where T : class, IDisposable =>
            Create<T>(new Uri(apiUrl, UriKind.Absolute), authenticate);
        public static T Create<T>(Uri apiUrl, Func<HttpClient, Task> authenticate = null) where T : class, IDisposable =>
            RestClient.Create<T>(apiUrl, typeof(void), authenticate);

        public static T Create<T, TError>(string apiUrl, Func<HttpClient, Task> authenticate = null) where T : class, IDisposable =>
            Create<T, TError>(new Uri(apiUrl, UriKind.Absolute), authenticate);
        public static T Create<T, TError>(Uri apiUrl, Func<HttpClient, Task> authenticate = null) where T : class, IDisposable =>
            RestClient.Create<T>(apiUrl, typeof(TError), authenticate);

        public static T Create<T>(Func<ProxyCall, object> target) where T : class
        {
            var proxyGenerator = new ProxyGenerator();
            return proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(
                ProxyGenerationOptions.Default,
                new Interceptor(target));
        }

        class Interceptor : IInterceptor
        {
            public Interceptor(Func<ProxyCall, object> target) => Target = target;
            Func<ProxyCall, object> Target { get; }
            public void Intercept(IInvocation invocation) =>
                invocation.ReturnValue = Target(
                    new ProxyCall(invocation.Method.Name, invocation.Arguments)
                        .Returns(invocation.Method.ReturnType)
                        .Define(invocation.Method.GetCustomAttributes()));
        }
    }
}
