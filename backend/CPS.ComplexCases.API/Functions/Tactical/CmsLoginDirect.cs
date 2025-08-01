using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.DDEI.Tactical.Client;
using CPS.ComplexCases.DDEI.Tactical.Models.Request;
using CPS.ComplexCases.Common.OpenApi.Filters;

namespace CPS.ComplexCases.API.Functions.Tactical;

public class CmsLoginDirect(IDdeiClientTactical ddeiClient)
{
  private readonly IDdeiClientTactical _ddeiClient = ddeiClient;

  [Function(nameof(CmsLoginDirectGet))]
  [OpenApiOperation(operationId: nameof(CmsLoginDirectGet), tags: ["CMS", "Authentication"], Description = "Returns the developer CMS login form.")]
  [OpenApiNoSecurity]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.TextPlain, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
  public IActionResult CmsLoginDirectGet([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tactical/login")] HttpRequest req) => new ContentResult
  {
    Content = HtmlHelpers.LoginForm(),
    ContentType = ContentType.TextHtml
  };

  [Function(nameof(CmsLoginDirectPost))]
  [OpenApiOperation(operationId: nameof(CmsLoginDirectPost), tags: ["CMS", "Authentication"], Description = "Authenticates a user in CMS and returns a token and cookie.")]
  [OpenApiNoSecurity]
  [OpenApiRequestBody(contentType: ContentType.MultipartFormData, bodyType: typeof(AuthenticationRequest), Description = "Form data including username and password.")]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.TextHtml, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
  public async Task<IActionResult> CmsLoginDirectPost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tactical/login")] HttpRequest req)
  {
    var username = req.Form[InputParameters.Username].ToString();
    var password = req.Form[InputParameters.Password].ToString();

    string? content;
    try
    {
      var response = await _ddeiClient.AuthenticateAsync(username, password);
      var cookieString = JsonSerializer.Serialize(response);
      AppendAuthCookie(req, cookieString);

      content = HtmlHelpers.LoginFormResult(username, true, cookieString);
    }
    catch (Exception e)
    {
      content = HtmlHelpers.LoginFormResult(username, false, e.Message);
    }

    return new ContentResult
    {
      Content = content,
      ContentType = ContentType.TextHtml
    };
  }

  private static void AppendAuthCookie(HttpRequest req, string cookiesString)
  {
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

    req.HttpContext.Response.Cookies.Append(HttpHeaderKeys.CmsAuthValues, cookiesString, cookieOptions);
  }
}