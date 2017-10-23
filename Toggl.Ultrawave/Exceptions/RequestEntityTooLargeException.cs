using System.Net;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class RequestEntityTooLargeException : ClientErrorException
    {
        public const HttpStatusCode CorrespondingHttpCode = HttpStatusCode.RequestEntityTooLarge;

        private const string defaultMessage = "The payload is too large, split it into batches.";

        internal RequestEntityTooLargeException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal RequestEntityTooLargeException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
