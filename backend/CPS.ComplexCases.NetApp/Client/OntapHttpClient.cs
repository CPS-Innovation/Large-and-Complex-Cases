using System.Net;
using System.Text.Json;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Ontap;

namespace CPS.ComplexCases.NetApp.Client;

public class OntapHttpClient(
    HttpClient httpClient,
    IOntapRequestFactory ontapRequestFactory,
    IHttpResponseHandler httpResponseHandler) : IOntapHttpClient
{
    private static readonly HttpResponseHandlerConfig HttpResponseHandlerConfig = new()
    {
        StatusCodeExceptionFactories = new Dictionary<HttpStatusCode, Func<HttpResponseMessage, Exception>>
        {
            [HttpStatusCode.Unauthorized] = response => new OntapUnauthorizedException(response.ReasonPhrase ?? "Unauthorized access to ONTAP.")
        },
        DefaultExceptionFactory = (statusCode, httpRequestException) => new OntapClientException(statusCode, httpRequestException)
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly IOntapRequestFactory _ontapRequestFactory = ontapRequestFactory;
    private readonly IHttpResponseHandler _httpResponseHandler = httpResponseHandler;

    public async Task<MaterialRenameResult> RenameMaterialAsync(MaterialRenameArg arg)
    {
        var request = _ontapRequestFactory.CreateRenameMaterialRequest(arg);

        var response = await CallOntap(request, HttpStatusCode.NotFound, HttpStatusCode.Conflict);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => new MaterialRenameResult(true, true, 1, null, null),
            HttpStatusCode.NotFound => new MaterialRenameResult(false, false, 0, $"Material not found at path: {arg.CurrentFilePath}", (int)HttpStatusCode.NotFound),
            HttpStatusCode.Conflict => new MaterialRenameResult(false, true, 0, $"Conflict occurred while renaming material from {arg.CurrentFilePath} to {arg.NewFilePath}", (int)HttpStatusCode.Conflict),
            _ => throw new OntapClientException(response.StatusCode, new HttpRequestException($"Unexpected status code: {response.StatusCode}")),
        };
    }

    public async Task<GetFileLockProtocolResult> GetFileLockAsync(GetFileLockArg arg)
    {
        var request = _ontapRequestFactory.CreateGetFileLockRequest(arg);
        var response = await CallOntap(request, HttpStatusCode.NotFound);

        var result = await DeserializeResponse<GetFileLockProtocolResult>(response);
        result.StatusCode = response.StatusCode;

        return result.StatusCode switch
        {
            HttpStatusCode.OK => result,
            HttpStatusCode.NotFound => new GetFileLockProtocolResult { StatusCode = HttpStatusCode.NotFound, Records = null },
            _ => throw new OntapClientException(result.StatusCode, new HttpRequestException($"Unexpected status code: {result.StatusCode}")),
        };
    }

    public async Task<GetCifsSessionUserResult> GetCifsSessionUserAsync(GetCifsSessionUserArg arg)
    {
        var request = _ontapRequestFactory.CreateGetCifsSessionUserRequest(arg);
        var response = await CallOntap(request, HttpStatusCode.NotFound);

        var result = await DeserializeResponse<GetCifsSessionUserResult>(response);
        result.StatusCode = response.StatusCode;

        return result.StatusCode switch
        {
            HttpStatusCode.OK => result,
            HttpStatusCode.NotFound => new GetCifsSessionUserResult { StatusCode = HttpStatusCode.NotFound, Records = null },
            _ => throw new OntapClientException(result.StatusCode, new HttpRequestException($"Unexpected status code: {result.StatusCode}")),
        };
    }

    private async Task<T> DeserializeResponse<T>(HttpResponseMessage response) where T : class
    {
        var content = await response.Content.ReadAsStringAsync();

        T? result;
        try
        {
            result = JsonSerializer.Deserialize<T>(content);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}", ex);
        }

        if (result == null)
        {
            throw new InvalidOperationException($"Deserialization to {typeof(T).Name} returned null.");
        }

        return result;
    }

    private Task<HttpResponseMessage> CallOntap(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
        => _httpResponseHandler.SendAsync(_httpClient, request, HttpResponseHandlerConfig, expectedUnhappyStatusCodes);
}