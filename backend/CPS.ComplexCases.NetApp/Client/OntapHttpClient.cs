using System.Net;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;

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

    private Task<HttpResponseMessage> CallOntap(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
        => _httpResponseHandler.SendAsync(_httpClient, request, HttpResponseHandlerConfig, expectedUnhappyStatusCodes);
}