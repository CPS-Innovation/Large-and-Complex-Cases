
using System.Net;
using Cps.ComplexCases.DDEI.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class Init(ILogger<Init> logger, IInitService initService)
{
    private readonly ILogger<Init> _logger = logger;
    private readonly IInitService _initService = initService;

    [Function(nameof(Init))]
    [OpenApiOperation(operationId: nameof(Init), tags: ["Authentication"], Description = "Represents a function that is the entry point for the LCC application")]
    [FunctionKeyAuth]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/init")] HttpRequest req, FunctionContext context)
    {
        _logger.LogInformation("Init function processed a request with correlation ID: {CorrelationId}", context.GetRequestContext().CorrelationId);

        req.HttpContext.Response.Cookies.Delete(HttpHeaderKeys.CmsAuthValues);

        string? cc = req.Query["cc"];

        InitResult result = await _initService.ProcessRequest(req, context.GetRequestContext().CorrelationId, cc);

        switch (result.Status)
        {
            case InitResultStatus.BadRequest:
                return new BadRequestObjectResult(result.Message);
            case InitResultStatus.Redirect:
                if (string.IsNullOrEmpty(result.RedirectUrl))
                {
                    _logger.LogError("Redirect URL is null or empty for redirect result.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
                if (result.ShouldSetCookie && !string.IsNullOrEmpty(result.Cc) && !string.IsNullOrEmpty(result.Ct))
                {
                    AppendAuthCookie(req, result.Cc, result.Ct);
                }
                return new RedirectResult(result.RedirectUrl, permanent: false);

            case InitResultStatus.ServerError:
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            default:
                _logger.LogError("Unhandled InitResultStatus: {Status}", result.Status);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private static void AppendAuthCookie(HttpRequest req, string cc, string ct)
    {
        string cookieValue = new AuthenticationContext(cc, ct, DateTime.UtcNow.AddHours(1)).ToString();

        var cookieOptions = req.IsHttps
        ? new CookieOptions
        {
            Path = "/api/",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        }
        : new CookieOptions
        {
            Path = "/api/",
            HttpOnly = true,
        };

        req.HttpContext.Response.Cookies.Append(HttpHeaderKeys.CmsAuthValues, cookieValue, cookieOptions);
    }
}