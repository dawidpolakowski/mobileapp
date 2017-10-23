using System.Net;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class GatewayTimeoutException : ServerErrorException
    {
        public const HttpStatusCode CorrespondingHttpCode = HttpStatusCode.GatewayTimeout;

        private const string defaultMessage = "Bad gateway.";

        internal GatewayTimeoutException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal GatewayTimeoutException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}