using System;
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
            => $"ApiException for request {request.HttpMethod} {request.Endpoint}:"
                + $"Response:"
                + $"(Status code [{response.StatusCode}])"
                + $"(Headers: {response.StatusCode})"
                + $"(Body: {response.RawData})"
                + $"(Message: {message})";

        public override string Message => ToString();
    }
}
