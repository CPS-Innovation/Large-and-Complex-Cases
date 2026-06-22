using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppNotFoundException : Exception, IHttpStatusCodeException
{
    public NetAppNotFoundException(string message)
        : base(message)
    {
    }

    public NetAppNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public HttpStatusCode StatusCode { get; } = HttpStatusCode.NotFound;
}
