using System.Net;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class HttpVersionNotSupportedException : ServerErrorException
    {
        public const HttpStatusCode CorrespondingHttpCode = HttpStatusCode.HttpVersionNotSupported;

        private const string defaultMessage = "HTTP version is not supported.";

        internal HttpVersionNotSupportedException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal HttpVersionNotSupportedException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}