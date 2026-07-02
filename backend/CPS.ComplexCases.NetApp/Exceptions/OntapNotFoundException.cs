using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class OntapNotFoundException : Exception, IHttpStatusCodeException
{
    public OntapNotFoundException(string message)
        : base(message)
    {
    }

    public OntapNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public HttpStatusCode StatusCode { get; } = HttpStatusCode.NotFound;
}
