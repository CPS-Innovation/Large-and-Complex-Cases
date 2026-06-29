using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class OntapUnauthorizedException : Exception, IHttpStatusCodeException
{
    public OntapUnauthorizedException()
        : base("Unauthorized access to ONTAP resource.")
    {
    }

    public OntapUnauthorizedException(string message)
        : base(message)
    {
    }

    public HttpStatusCode StatusCode { get; } = HttpStatusCode.Unauthorized;
}
