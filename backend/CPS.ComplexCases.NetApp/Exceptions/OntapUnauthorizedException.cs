using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class OntapUnauthorizedException : Exception
{
    public OntapUnauthorizedException()
        : base("Unauthorized access to ONTAP resource.")
    {
    }

    public OntapUnauthorizedException(string message)
        : base(message)
    {
    }

    public static HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
}
