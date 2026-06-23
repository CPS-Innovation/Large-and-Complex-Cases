using System.Net;

namespace CPS.ComplexCases.Common.Handlers;

public interface IHttpResponseHandler
{
    Task<HttpResponseMessage> SendAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        HttpResponseHandlerConfig config,
        params HttpStatusCode[] expectedUnhappyStatusCodes);
}
