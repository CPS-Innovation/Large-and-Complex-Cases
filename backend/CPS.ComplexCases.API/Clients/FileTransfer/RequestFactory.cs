using CPS.ComplexCases.API.Constants;
using Microsoft.Extensions.Configuration;

namespace CPS.ComplexCases.API.Clients.FileTransfer;

public class RequestFactory : IRequestFactory
{
    private readonly string _accessKey;

    public RequestFactory(IConfiguration configuration)
    {
        _accessKey = configuration["FileTransferApiOptions:AccessKey"] ?? throw new ArgumentNullException("AccessKey not found in configuration");
    }

    public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(httpMethod, requestUri)
        {
            Content = content
        };

        request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
        request.Headers.Add(HttpHeaderKeys.FunctionAccessKey, _accessKey);

        return request;
    }
}