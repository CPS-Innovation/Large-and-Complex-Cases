using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppUnauthorizedException : Exception
{
    public NetAppUnauthorizedException()
        : base("Unauthorized access to NetApp resource.")
    {
    }

    public NetAppUnauthorizedException(string message)
        : base(message)
    {
    }

    public static HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
}
