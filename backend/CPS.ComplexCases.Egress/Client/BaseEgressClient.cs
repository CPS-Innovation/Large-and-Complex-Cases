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

    // Cache the workspace token for its lifetime. Previously a fresh token was fetched on every Egress
    // call (one transfer logged ~1009 fetches in 24 minutes, dominated by the per-chunk PATCHes).
    // Caching collapses those to one fetch per token lifetime per client instance.
    private string? _cachedToken;
    private DateTime _cachedTokenExpiresAtUtc = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private static readonly TimeSpan TokenRefreshSkew = TimeSpan.FromSeconds(60);
    private const int DefaultTokenLifetimeSeconds = 300;

    protected async Task<string> GetWorkspaceToken()
    {
        if (_cachedToken is not null && DateTime.UtcNow < _cachedTokenExpiresAtUtc)
        {
            return _cachedToken;
        }

        await _tokenLock.WaitAsync();
        try
        {
            // Re-check after acquiring the lock in case another caller just refreshed the token.
            if (_cachedToken is not null && DateTime.UtcNow < _cachedTokenExpiresAtUtc)
            {
                return _cachedToken;
            }

            var response = await SendRequestAsync<GetWorkspaceTokenResponse>(
                _egressRequestFactory.GetWorkspaceTokenRequest(_egressOptions.Username, _egressOptions.Password));

            var lifetimeSeconds = response.Expiration is > 0 ? response.Expiration.Value : DefaultTokenLifetimeSeconds;
            var ttl = TimeSpan.FromSeconds(lifetimeSeconds) - TokenRefreshSkew;
            if (ttl <= TimeSpan.Zero)
            {
                // Token lives shorter than the refresh skew; keep half of its life to avoid thrashing.
                ttl = TimeSpan.FromSeconds(Math.Max(1, lifetimeSeconds / 2.0));
            }

            _cachedToken = response.Token;
            _cachedTokenExpiresAtUtc = DateTime.UtcNow.Add(ttl);
            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
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
    // longer TransferTimeoutSeconds, and for streamed downloads the token only covers the header read.
    // The body read of a streamed download is bounded separately by the consumer (TransferFile applies
    // a per-read idle timeout to the returned stream).
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

        // Log the response body before EnsureSuccessStatusCode discards it, so Egress-side error
        // detail (e.g. the generic 500 body) is captured in telemetry for diagnosis.
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Egress non-success response — Status: {StatusCode}, URL: {RequestUrl}, Body: {ResponseBody}",
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