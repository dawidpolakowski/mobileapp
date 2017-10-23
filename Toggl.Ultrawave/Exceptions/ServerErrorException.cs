using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public abstract class ServerErrorException : ApiException
    {
        internal ServerErrorException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
