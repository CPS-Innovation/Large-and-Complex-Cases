using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Response;

namespace CPS.ComplexCases.Egress.Client;

public abstract class BaseEgressClient(
    ILogger logger,
    IOptions<EgressOptions> egressOptions,
    HttpClient httpClient,
    IEgressRequestFactory egressRequestFactory,
    ITelemetryClient telemetryClient)
{
    protected readonly ILogger _logger = logger;
    protected readonly EgressOptions _egressOptions = egressOptions.Value;
    protected readonly HttpClient _httpClient = httpClient;
    protected readonly IEgressRequestFactory _egressRequestFactory = egressRequestFactory;
    protected readonly ITelemetryClient _telemetryClient = telemetryClient;

    protected async Task<string> GetWorkspaceToken()
    {
        var response = await SendRequestAsync<GetWorkspaceTokenResponse>(
            _egressRequestFactory.GetWorkspaceTokenRequest(_egressOptions.Username, _egressOptions.Password));
        return response.Token;
    }

    protected async Task<T> SendRequestAsync<T>(HttpRequestMessage request, [CallerMemberName] string callerMemberName = "")
    {
        using var response = await SendRequestAsync(request, false, callerMemberName);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(responseContent)
            ?? throw new InvalidOperationException("Deserialization returned null.");
        return result;
    }

    protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, bool streamResponse = false, [CallerMemberName] string callerMemberName = "")
    {
        var completionOption = streamResponse
            ? HttpCompletionOption.ResponseHeadersRead
            : HttpCompletionOption.ResponseContentRead;

        var telemetryEvent = new ExternalApiCallEvent(nameof(EgressClient), request, callerMemberName);

        var response = await _httpClient.SendAsync(request, completionOption);

        telemetryEvent.CallEndTime = DateTime.UtcNow;
        telemetryEvent.ResponseStatusCode = response.StatusCode;
        _telemetryClient.TrackEvent(telemetryEvent);

        try
        {
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (HttpRequestException ex) when (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Resource not found. Check the workspace ID.");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending request to egress service");
            throw;
        }
    }
}