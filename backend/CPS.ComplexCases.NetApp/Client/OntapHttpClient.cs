using System.Net;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.AspNetCore.Mvc;

namespace CPS.ComplexCases.NetApp.Client;

public class OntapHttpClient(
    HttpClient httpClient,
    IOntapArgFactory ontapArgFactory,
    IOntapRequestFactory ontapRequestFactory,
    IHttpResponseHandler httpResponseHandler) : IOntapHttpClient
{
    private static readonly HttpResponseHandlerConfig HttpResponseHandlerConfig = new()
    {
        StatusCodeExceptionFactories = new Dictionary<HttpStatusCode, Func<HttpResponseMessage, Exception>>
        {
            [HttpStatusCode.Unauthorized] = response => new OntapUnauthorizedException(response.ReasonPhrase ?? "Unauthorized access to ONTAP."),
            [HttpStatusCode.NotFound] = response => new OntapNotFoundException(response.ReasonPhrase ?? "User not found."),
            [HttpStatusCode.Conflict] = response => new OntapConflictException(response.ReasonPhrase ?? "Conflict occurred while accessing ONTAP API.")
        },
        DefaultExceptionFactory = (statusCode, httpRequestException) => new OntapClientException(statusCode, httpRequestException)
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly IOntapArgFactory _ontapArgFactory = ontapArgFactory;
    private readonly IOntapRequestFactory _ontapRequestFactory = ontapRequestFactory;
    private readonly IHttpResponseHandler _httpResponseHandler = httpResponseHandler;

    public async Task<IActionResult> RenameMaterialAsync(string bearerToken, Guid ontapVolumeUuid, string currentFolderPath, string newFolderPath)
    {
        var request = _ontapRequestFactory.CreateRenameMaterialRequest(_ontapArgFactory.CreateMaterialRenameArg(bearerToken, ontapVolumeUuid, currentFolderPath, newFolderPath));

        var response = await CallOntap(request);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => new OkResult(),
            HttpStatusCode.NotFound => new NotFoundObjectResult($"Material not found at path: {currentFolderPath}"),
            HttpStatusCode.Conflict => new ConflictObjectResult($"Conflict occurred while renaming material from {currentFolderPath} to {newFolderPath}"),
            _ => throw new OntapClientException(response.StatusCode, new HttpRequestException($"Unexpected status code: {response.StatusCode}")),
        };
    }

    private Task<HttpResponseMessage> CallOntap(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
        => _httpResponseHandler.SendAsync(_httpClient, request, HttpResponseHandlerConfig, expectedUnhappyStatusCodes);
}