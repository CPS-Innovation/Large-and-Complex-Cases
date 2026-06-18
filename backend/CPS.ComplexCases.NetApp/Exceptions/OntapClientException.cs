using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class OntapClientException(HttpStatusCode statusCode, HttpRequestException httpRequestException) : Exception($"The HTTP request failed with status code {statusCode}", httpRequestException)
{
    public HttpStatusCode StatusCode { get; private set; } = statusCode;
}