using System.Net;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class BadGatewayException : ServerErrorException
    {
        public const HttpStatusCode CorrespondingHttpCode = HttpStatusCode.BadGateway;

        private const string defaultMessage = "Bad gateway.";

        internal BadGatewayException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal BadGatewayException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
