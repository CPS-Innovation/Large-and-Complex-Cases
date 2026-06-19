using System.Diagnostics;
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
        // TEMPORARY DIAGNOSTIC: verify Key Vault references are resolving
        var usernameHint = string.IsNullOrEmpty(_egressOptions.Username)
            ? "<EMPTY>"
            : $"{_egressOptions.Username[..Math.Min(3, _egressOptions.Username.Length)]}***" +
              $" (len={_egressOptions.Username.Length})";
        var passwordHint = string.IsNullOrEmpty(_egressOptions.Password)
            ? "<EMPTY>"
            : $"{_egressOptions.Password[..Math.Min(3, _egressOptions.Password.Length)]}***" +
              $" (len={_egressOptions.Password.Length})";
        _logger.LogWarning(
            "[DIAG] Egress credential check — Username: {UsernameHint}, Password: {PasswordHint}",
            usernameHint, passwordHint);

        var response = await SendRequestAsync<GetWorkspaceTokenResponse>(
            _egressRequestFactory.GetWorkspaceTokenRequest(_egressOptions.Username, _egressOptions.Password));
        return response.Token;
    }

    protected async Task<T> SendRequestAsync<T>(HttpRequestMessage request,
        [CallerMemberName] string callerMemberName = "")
    {
        using var response = await SendRequestAsync(request, streamResponse: false, callerMemberName: callerMemberName);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(responseContent)
                     ?? throw new InvalidOperationException("Deserialization returned null.");
        return result;
    }

    // The HttpClient for storage operations is registered with an infinite timeout because a
    // streamed download's body read is bound by HttpClient.Timeout. Each request therefore enforces
    // its own timeout here: management calls use ManagementTimeoutSeconds, chunk uploads pass the
    // longer TransferTimeoutSeconds, and for streamed downloads the token only covers the header read
    protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, bool streamResponse = false,
        TimeSpan? timeout = null,
        [CallerMemberName] string callerMemberName = "")
    {
        var completionOption = streamResponse
            ? HttpCompletionOption.ResponseHeadersRead
            : HttpCompletionOption.ResponseContentRead;

        var telemetryEvent = new ExternalApiCallEvent(nameof(EgressClient), request, callerMemberName);

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(_egressOptions.ManagementTimeoutSeconds);
        using var timeoutCts = new CancellationTokenSource(effectiveTimeout);
        var stopwatch = Stopwatch.StartNew();

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, completionOption, timeoutCts.Token);
        }
        catch (OperationCanceledException ex) when (timeoutCts.IsCancellationRequested)
        {
            _logger.LogError(ex,
                "Egress request {Caller} timed out after {ElapsedSeconds:F1}s (configured timeout {TimeoutSeconds}s).",
                callerMemberName, stopwatch.Elapsed.TotalSeconds, effectiveTimeout.TotalSeconds);
            throw;
        }

        telemetryEvent.CallEndTime = DateTime.UtcNow;
        telemetryEvent.ResponseStatusCode = response.StatusCode;
        _telemetryClient.TrackEvent(telemetryEvent);

        // TEMPORARY DIAGNOSTIC: log response body before EnsureSuccessStatusCode discards it
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "[DIAG] Egress non-success response — Status: {StatusCode}, URL: {RequestUrl}, Body: {ResponseBody}",
                (int)response.StatusCode, request.RequestUri, responseBody);
        }

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