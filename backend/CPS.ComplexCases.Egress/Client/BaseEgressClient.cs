using System.Net;
using System.Text.Json;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.Egress.Client;

public abstract class BaseEgressClient
{
    protected readonly ILogger _logger;
    protected readonly EgressOptions _egressOptions;
    protected readonly HttpClient _httpClient;
    protected readonly IEgressRequestFactory _egressRequestFactory;

    protected BaseEgressClient(
        ILogger logger,
        IOptions<EgressOptions> egressOptions,
        HttpClient httpClient,
        IEgressRequestFactory egressRequestFactory)
    {
        _logger = logger;
        _egressOptions = egressOptions.Value;
        _httpClient = httpClient;
        _egressRequestFactory = egressRequestFactory;
    }

    protected async Task<string> GetWorkspaceToken()
    {
        var response = await SendRequestAsync<GetWorkspaceTokenResponse>(
            _egressRequestFactory.GetWorkspaceTokenRequest(_egressOptions.Username, _egressOptions.Password));
        return response.Token;
    }

    protected async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
    {
        using var response = await SendRequestAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(responseContent)
            ?? throw new InvalidOperationException("Deserialization returned null.");
        return result;
    }

    protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        try
        {
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (HttpRequestException ex) when (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Workspace not found. Check the workspace ID.");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending request to egress service");
            throw;
        }
    }
}