using System.Text;
using System.Text.Json;
using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.API.Clients.FileTransfer;

public class FileTransferClient(IRequestFactory requestFactory, HttpClient httpClient) : IFileTransferClient
{
    private readonly IRequestFactory _requestFactory = requestFactory;
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HttpResponseMessage> InitiateFileTransferAsync(TransferRequest transferRequest, Guid correlationId)
    {
        return await SendRequestAsync(
            HttpMethod.Post,
            "transfer",
            correlationId,
            new StringContent(JsonSerializer.Serialize(transferRequest), Encoding.UTF8, "application/json"));
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, HttpContent? content = null)
    {
        var request = _requestFactory.Create(httpMethod, requestUri, correlationId, content);

        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

}