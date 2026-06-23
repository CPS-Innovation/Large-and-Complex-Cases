using System.Net;

namespace CPS.ComplexCases.Common.Handlers;

public class HttpResponseHandler : IHttpResponseHandler
{
    public async Task<HttpResponseMessage> SendAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        HttpResponseHandlerConfig config,
        params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
        {
            return response;
        }

        if (config.StatusCodeExceptionFactories.TryGetValue(response.StatusCode, out var exceptionFactory))
        {
            throw exceptionFactory(response);
        }

        var content = await response.Content.ReadAsStringAsync();
        var httpRequestException = new HttpRequestException(content);
        throw config.DefaultExceptionFactory(response.StatusCode, httpRequestException);
    }
}
