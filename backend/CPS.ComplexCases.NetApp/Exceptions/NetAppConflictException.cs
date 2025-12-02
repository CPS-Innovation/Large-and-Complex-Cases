using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppConflictException : Exception
{
    public NetAppConflictException()
        : base("Conflict occurred while accessing NetApp API.")
    {
    }

    public NetAppConflictException(string message)
        : base(message)
    {
    }

    public static HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}