using Cps.ComplexCases.DDEI.Models;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Services;

public class InitService(ILogger<InitService> logger, IConfiguration configuration, IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory) : IInitService
{
    private readonly ILogger<InitService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IDdeiClient _ddeiClient = ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

    public async Task<InitResult> ProcessRequest(HttpRequest req, Guid correlationId, string? cc)
    {
        var redirectUrlCwa = _configuration["RedirectUrl:CaseworkApp"] ?? string.Empty;
        var redirectUrlLccUi = _configuration["RedirectUrl:LccUi"] ?? string.Empty;

        if (string.IsNullOrEmpty(redirectUrlCwa) || string.IsNullOrEmpty(redirectUrlLccUi))
        {
            _logger.LogError("One or more redirect URL's are missing.");
            return new InitResult
            {
                Status = InitResultStatus.ServerError,
                Message = "One or more redirect URL's are missing"
            };
        }

        // set cookies if cc is present
        if (!string.IsNullOrEmpty(cc))
        {
            string? ct = null;

            try
            {
                var fullCmsAuthValues = new AuthenticationContext(cc, Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1)).ToString();
                ct = await _ddeiClient.GetCmsModernTokenAsync(_ddeiArgFactory.CreateBaseArg(fullCmsAuthValues, correlationId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get ct from ddei GetCmsModernTokenAsync");
            }

            _logger.LogInformation("Redirecting to {RedirectUrlLccUi} with correlationId {CorrelationId}", redirectUrlLccUi, correlationId);

            return new InitResult
            {
                Status = InitResultStatus.Redirect,
                RedirectUrl = redirectUrlLccUi,
                ShouldSetCookie = true,
                Cc = cc,
                Ct = ct
            };
        }

        // redirect to CWA handoff endpoint if cc is missing
        _logger.LogInformation("Redirecting to {RedirectUrlCwa} with correlationId {CorrelationId}", redirectUrlCwa, correlationId);

        return new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = BuildRedirectUrl(req, redirectUrlCwa),
        };
    }

    internal string BuildRedirectUrl(HttpRequest req, string redirectUrlCwa)
    {
        string host = $"{req.Scheme}://{req.Host}";

        string redirectEndpoint = "/api/v1/init";

        string redirectUrl = $"{redirectUrlCwa}{host}{redirectEndpoint}";

        _logger.LogInformation("Built redirect URL: {RedirectUrl}", redirectUrl);

        return redirectUrl;
    }
}