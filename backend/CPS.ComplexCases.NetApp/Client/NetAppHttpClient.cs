using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.NetApp;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppHttpClient(HttpClient httpClient, INetAppRequestFactory netAppRequestFactory, ILogger<NetAppHttpClient> logger) : INetAppHttpClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly INetAppRequestFactory _netAppRequestFactory = netAppRequestFactory;
    private readonly ILogger<NetAppHttpClient> _logger = logger;

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

    private async Task<HttpResponseMessage> CallNetApp(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        var response = await _httpClient.SendAsync(request);
        try
        {
            if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
            {
                return response;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new NetAppUnauthorizedException(response.ReasonPhrase ?? "Unauthorized access to NetApp.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NetAppNotFoundException(response.ReasonPhrase ?? "User not found.");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new NetAppConflictException(response.ReasonPhrase ?? "Conflict occurred while accessing NetApp API.");
            }

            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(content);
        }
        catch (HttpRequestException exception)
        {
            throw new NetAppClientException(response.StatusCode, exception);
        }
    }
}