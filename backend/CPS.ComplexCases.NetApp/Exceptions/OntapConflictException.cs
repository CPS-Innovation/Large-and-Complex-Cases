using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class OntapConflictException : Exception, IHttpStatusCodeException
{
    public OntapConflictException()
        : base("Conflict occurred while accessing ONTAP API.")
    {
    }

    public OntapConflictException(string message)
        : base(message)
    {
    }

    public HttpStatusCode StatusCode { get; } = HttpStatusCode.Conflict;
}