using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class OntapNotFoundException : Exception
{
    public OntapNotFoundException(string message)
        : base(message)
    {
    }

    public OntapNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public static HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}
