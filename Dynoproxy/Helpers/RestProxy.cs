using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Authentication;

namespace Dynoproxy.Helpers
{
    static class RestProxy
    {
        public static T Create<T>(Uri apiUrl, Func<HttpClient, Task> authenticate = null) 
            where T : class, IDisposable
        {
            var client = new HttpClient() { BaseAddress = apiUrl };
            return Proxy.Create<T>(Execute);            
            object Execute(ProxyCall call)
            {
                if(call.IsDispose)
                {
                    client.Dispose();
                    return null;
                }
                                
                return call.Result.Void ? Send() : SendAndReceive();
                object Send() => client.SendAsync(call, authenticate);
                object SendAndReceive() =>
                    typeof(RestProxy)
                        .GetMethod(nameof(SendAndReceiveAsync), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(call.Result.Type)
                        .Invoke(null, new object[] { client, call, authenticate });
            }
        }        
        
        static async Task<T> SendAndReceiveAsync<T>(
            HttpClient client, ProxyCall call, Func<HttpClient, Task> authenticate)
        {
            var response = await client.SendAsync(call, authenticate);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        static async Task<HttpResponseMessage> SendAsync(
            this HttpClient client, ProxyCall call, Func<HttpClient, Task> authenticate = null)
        {
            var description = call.Method.Description
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var request = new HttpRequestMessage(
                new HttpMethod(description[0]),
                new Uri(
                    string.Format(description[1], call.Args.ToArray()),
                    UriKind.Relative))
                {
                    Content = description.Length < 3 ||
                        !int.TryParse(description[2].Trim("{}".ToCharArray()), out var index)
                        ? null
                        : new StringContent(
                            JsonConvert.SerializeObject(call.Args[index]),
                            Encoding.UTF8,
                            "application/json")
                };

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                if(authenticate == null)
                    throw new AuthenticationException();
                else
                {
                    await authenticate(client);
                    return await SendAsync(client, call);
                }

            response.EnsureSuccessStatusCode();
            return response;            
        }
    }
}
