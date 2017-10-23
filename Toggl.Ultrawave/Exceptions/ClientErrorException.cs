using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public abstract class ClientErrorException : ApiException
    {
        internal ClientErrorException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
