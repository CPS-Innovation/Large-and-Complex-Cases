using System.Net;
using System.Text.Json;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.NetApp;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppHttpClient(HttpClient httpClient, INetAppRequestFactory netAppRequestFactory) : INetAppHttpClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly INetAppRequestFactory _netAppRequestFactory = netAppRequestFactory;

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
        var result = JsonSerializer.Deserialize<T>(content) ?? throw new InvalidOperationException("Deserialization returned null.");
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
                throw new NetAppUnauthorizedException();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NetAppNotFoundException(response.ReasonPhrase ?? "User not found.");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new NetAppConflictException();
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