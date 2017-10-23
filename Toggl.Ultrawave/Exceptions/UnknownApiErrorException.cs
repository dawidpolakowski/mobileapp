using System.Net;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class UnknownApiErrorException : ApiException
    {
        public HttpStatusCode HttpCode { get; }

        private const string defaultMessage = "The server responded with an unexpected HTTP status code.";

        internal UnknownApiErrorException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal UnknownApiErrorException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
            this.HttpCode = response.StatusCode;
        }
    }
}
