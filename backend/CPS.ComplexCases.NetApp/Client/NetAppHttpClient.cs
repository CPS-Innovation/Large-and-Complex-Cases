using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.NetApp;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppHttpClient(
    HttpClient httpClient,
    INetAppRequestFactory netAppRequestFactory,
    ILogger<NetAppHttpClient> logger,
    IHttpResponseHandler httpResponseHandler) : INetAppHttpClient
{
    private static readonly HttpResponseHandlerConfig HttpResponseHandlerConfig = new()
    {
        StatusCodeExceptionFactories = new Dictionary<HttpStatusCode, Func<HttpResponseMessage, Exception>>
        {
            [HttpStatusCode.Unauthorized] = response => new NetAppUnauthorizedException(response.ReasonPhrase ?? "Unauthorized access to NetApp."),
            [HttpStatusCode.NotFound] = response => new NetAppNotFoundException(response.ReasonPhrase ?? "User not found."),
            [HttpStatusCode.Conflict] = response => new NetAppConflictException(response.ReasonPhrase ?? "Conflict occurred while accessing NetApp API.")
        },
        DefaultExceptionFactory = (statusCode, httpRequestException) => new NetAppClientException(statusCode, httpRequestException)
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly INetAppRequestFactory _netAppRequestFactory = netAppRequestFactory;
    private readonly ILogger<NetAppHttpClient> _logger = logger;
    private readonly IHttpResponseHandler _httpResponseHandler = httpResponseHandler;

    public Task<NetAppUserResponse> RegisterUserAsync(RegisterUserArg arg)
    {
        var request = _netAppRequestFactory.CreateRegisterUserRequest(arg);
        return CallNetApp<NetAppUserResponse>(request);
    }

    public Task<NetAppUserResponse> RegenerateUserKeysAsync(RegenerateUserKeysArg arg)
    {
        var request = _netAppRequestFactory.CreateRegenerateUserKeysRequest(arg);
        return CallNetApp<NetAppUserResponse>(request);
    }

    private async Task<T> CallNetApp<T>(HttpRequestMessage request)
    {
        using var response = await CallNetApp(request);
        var content = await response.Content.ReadAsStringAsync();

        T? result;
        try
        {
            result = JsonSerializer.Deserialize<T>(content);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize NetApp response to {TypeName}. Content: {Content}", typeof(T).Name, content);
            throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}", ex);
        }

        if (result == null)
        {
            _logger.LogError("Deserialization of NetApp response to {TypeName} returned null. Content: {Content}", typeof(T).Name, content);
            throw new InvalidOperationException($"Deserialization to {typeof(T).Name} returned null.");
        }
        return result;
    }

    private Task<HttpResponseMessage> CallNetApp(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
        => _httpResponseHandler.SendAsync(_httpClient, request, HttpResponseHandlerConfig, expectedUnhappyStatusCodes);
}