using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppUnauthorizedException : Exception, IHttpStatusCodeException
{
    public NetAppUnauthorizedException()
        : base("Unauthorized access to NetApp resource.")
    {
    }

    public NetAppUnauthorizedException(string message)
        : base(message)
    {
    }

    public HttpStatusCode StatusCode { get; } = HttpStatusCode.Unauthorized;
}
