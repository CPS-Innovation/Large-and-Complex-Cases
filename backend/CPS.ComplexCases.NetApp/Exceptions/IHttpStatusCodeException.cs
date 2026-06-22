using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public interface IHttpStatusCodeException
{
    HttpStatusCode StatusCode { get; }
}