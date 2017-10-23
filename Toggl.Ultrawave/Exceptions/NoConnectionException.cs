using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class NoConnectionException : EndpointUnreachableException
    {
        private const string defaultErrorMessage = "No connection found.";

        internal NoConnectionException(IRequest request, IResponse response)
            : this(request, response, defaultErrorMessage)
        {
        }

        internal NoConnectionException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
