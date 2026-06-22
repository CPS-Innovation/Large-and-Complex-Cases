using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppConflictException : Exception, IHttpStatusCodeException
{
    public NetAppConflictException()
        : base("Conflict occurred while accessing NetApp API.")
    {
    }

    public NetAppConflictException(string message)
        : base(message)
    {
    }

    public HttpStatusCode StatusCode { get; } = HttpStatusCode.Conflict;
}