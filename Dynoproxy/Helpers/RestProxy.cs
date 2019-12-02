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
        public static T Create<T>(Uri apiUrl, Action<HttpClient> authenticate = null) where T : class, IDisposable
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

                try { return Try(); }
                catch (AuthenticationException) when (authenticate != null)
                {
                    authenticate(client);
                    return Try();
                }

                object Try() => call.Result.Void ? Send() : SendAndReceive();
                object Send() => client.SendAsync(call);
                object SendAndReceive() =>
                    typeof(RestProxy)
                        .GetMethod(nameof(SendAndReceiveAsync), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(call.Result.Type)
                        .Invoke(null, new object[] { client, call });
            }
        }        
        
        static async Task<T> SendAndReceiveAsync<T>(HttpClient client, ProxyCall call)
        {
            var response = await client.SendAsync(call);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        static async Task<HttpResponseMessage> SendAsync(this HttpClient client, ProxyCall call)
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
                throw new AuthenticationException();

            response.EnsureSuccessStatusCode();
            return response;            
        }
    }
}
