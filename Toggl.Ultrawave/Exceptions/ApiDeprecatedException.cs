using System.Net;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class ApiDeprecatedException : ClientErrorException
    {
        public const HttpStatusCode CorrespondingHttpCode = HttpStatusCode.Gone;

        private const string defaultMessage = "This version of API is deprecated and the client must be updated to an up-to-date version.";

        internal ApiDeprecatedException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal ApiDeprecatedException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
