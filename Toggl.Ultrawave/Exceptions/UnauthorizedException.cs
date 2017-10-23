using System.Net;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class UnauthorizedException : ClientErrorException
    {
        public const HttpStatusCode CorrespondingHttpCode = HttpStatusCode.Unauthorized;

        private const string defaultMessage = "User is not authorized to make this request and must enter login again.";

        internal UnauthorizedException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal UnauthorizedException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
