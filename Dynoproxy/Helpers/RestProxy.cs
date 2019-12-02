using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynoproxy.Helpers
{
    static class RestProxy
    {
        public static T Create<T>(Uri apiUrl) where T : class, IDisposable
        {
            var client = new HttpClient() { BaseAddress = apiUrl };
            return Proxy.Create<T>(ExecuteAsync);
            object ExecuteAsync(ProxyCall call)
            {
                switch(call.Name)
                {
                    case nameof(IDisposable.Dispose):
                        client.Dispose();
                        return 0;

                    default:
                        return call.Result.Void
                            ? SendAsync()
                            : SendAndReceiveAsync();
                }                

                async Task SendAsync()
                {
                    var response = await client.SendAsync(Request(call));
                    response.EnsureSuccessStatusCode();
                }

                Task SendAndReceiveAsync() =>
                    (Task)typeof(RestProxy)
                        .GetMethod(nameof(SendAndReceiveAsync), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(call.Result.Type)
                        .Invoke(null, new object[] { client, call });
            }
        }        
        
        static async Task<T> SendAndReceiveAsync<T>(HttpClient client, ProxyCall call)
        {
            var response = await client.SendAsync(Request(call));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        static HttpRequestMessage Request(ProxyCall call)
        {
            var description = call.Method.Description
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return new HttpRequestMessage(
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
        }
    }
}
