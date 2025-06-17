using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Configuration;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.API.Clients.FileTransfer;

public class RequestFactory : IRequestFactory
{
    private readonly string _accessKey;

    public RequestFactory(IOptions<FileTransferApiOptions> options)
    {
        _accessKey = options.Value.AccessKey ?? throw new ArgumentNullException(
            nameof(options.Value.AccessKey),
            "FileTransferApiOptions:AccessKey is missing from configuration."
        );
        if (string.IsNullOrWhiteSpace(_accessKey))
        {
            throw new ArgumentException("FileTransferApiOptions:AccessKey cannot be null or whitespace.", nameof(options.Value.AccessKey));
        }
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