using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppUnauthorizedException : NetAppClientException
{
    public NetAppUnauthorizedException()
        : base(HttpStatusCode.Unauthorized, "Unauthorized access to NetApp resource.")
    {
    }

    public NetAppUnauthorizedException(string message)
        : base(HttpStatusCode.Unauthorized, message)
    {
    }
}
