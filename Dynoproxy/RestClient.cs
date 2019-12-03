using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Dynoproxy
{
    class RestClient : IDisposable
    {
        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public static T Create<T>(Uri apiUrl, Type error, Func<HttpClient, Task> authenticate = null)
            where T : class, IDisposable
        {
            var client = new RestClient(apiUrl, error, authenticate);
            return Proxy.Create<T>(Execute);
            object Execute(ProxyCall call)
            {
                if (call.IsDispose) 
                { 
                    client.Dispose(); 
                    return null; 
                }

                return client.SendAsync(call);
            }
        }

        public RestClient(Uri apiUrl, Type error, Func<HttpClient, Task> authenticate = null)
        {
            Client = new HttpClient() { BaseAddress = apiUrl };
            Client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            Authenticate = () => authenticate?.Invoke(Client) ?? Task.CompletedTask;
            Init = Authenticate();
            Error = error;
        }

        HttpClient Client { get; }
        Task Init { get; }        
        Type Error { get; }
        Func<Task> Authenticate { get; }

        public void Dispose() => Client.Dispose();

        public Task SendAsync(ProxyCall call)
        {
            return call.Result.Sync 
                ? throw new NotSupportedException("Synchrounous API is not supported.")
                : call.Result.Void
                    ? sendAsync()
                    : sendAndReceiveAsync();

            async Task sendAsync() => await ReadAsync(call, Authenticate);
            Task sendAndReceiveAsync() =>
                (Task)typeof(RestClient)
                    .GetMethod(nameof(SendAndReceiveAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(call.Result.Type)
                    .Invoke(this, new object[] { call });
        }

        async Task<T> SendAndReceiveAsync<T>(ProxyCall call) =>
            JsonConvert.DeserializeObject<T>(await ReadAsync(call, Authenticate), Settings);

        async Task<string> ReadAsync(ProxyCall call, Func<Task> authenticate = null)
        {
            await Init;

            var response = await Client.SendAsync(Request(call));
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                if (authenticate == null)
                    throw new AuthenticationException();
                else
                {
                    await authenticate();
                    return await ReadAsync(call);
                }

            var payload = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw RestException.Create(
                    Authenticate, 
                    response.StatusCode, 
                    payload, 
                    Error);
                
            return payload;
        }

        HttpRequestMessage Request(ProxyCall call)
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
                            JsonConvert.SerializeObject(call.Args[index], Settings),
                            Encoding.UTF8,
                            "application/json")
            };
        }
    }
}
