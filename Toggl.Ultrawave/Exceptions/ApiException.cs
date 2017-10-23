using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public class ApiException : Exception
    {
        private readonly IRequest request;

        private readonly IResponse response;

        private string message;

        internal ApiException(IRequest request, IResponse response, string message)
        {
            this.request = request;
            this.response = response;
            this.message = message;
        }

        public override string ToString()
            => $"ApiException for request {request.HttpMethod} {request.Endpoint}: "
                + $"Response: "
                + $"(Status code [{response.StatusCode}]) "
                + $"(Headers: [{SerializeHeaders(response.Headers)}]) "
                + $"(Body: {response.RawData}) "
                + $"(Message: {message})";

        public static string SerializeHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
            => String.Join(", ", headers.Select(pair => $"{pair.Key}: [{String.Join(", ", pair.Value.ToArray())}]").ToArray());

        public override string Message => ToString();
    }
}
