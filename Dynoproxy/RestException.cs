using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dynoproxy
{
    public class RestException : Exception
    {
        public static RestException Create(Func<Task> authenticate, HttpStatusCode statusCode, string text, Type error) =>
            error == typeof(void)
            ? new RestException(authenticate, statusCode, text)
            : (RestException)Activator.CreateInstance(
                typeof(RestException<>).MakeGenericType(error),
                authenticate, statusCode, text);
        
        public RestException(Func<Task> authenticate, HttpStatusCode statusCode, string text)
            : base($"REST API error ({statusCode}).")
        {
            Authenticate = authenticate;
            StatusCode = statusCode;
            Text = text;
        }

        Func<Task> Authenticate { get; }
        public async Task AuthenticateAsync() => await Authenticate();

        public HttpStatusCode StatusCode { get; }
        public string Text { get; }

    }

    public class RestException<TError> : RestException
    {
        public RestException(Func<Task> authenticate, HttpStatusCode statusCode, string text)
            : base(authenticate, statusCode, text)
        {
            Error = JsonConvert.DeserializeObject<TError>(text);
        }

        public TError Error { get; }
    }
}
