using System.Text.Json;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.DDEI.Tactical.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Functions.Tactical;

public class CmsLoginDirect(IDdeiClientTactical ddeiClient)
{
  private readonly IDdeiClientTactical _ddeiClient = ddeiClient;

  [Function(nameof(CmsLoginDirectGet))]
  public IActionResult CmsLoginDirectGet([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tactical/login")] HttpRequest req) => new ContentResult
  {
    Content = HtmlHelpers.LoginForm(),
    ContentType = "text/html"
  };

  [Function(nameof(CmsLoginDirectPost))]
  public async Task<IActionResult> CmsLoginDirectPost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tactical/login")] HttpRequest req)
  {
    var username = req.Form["username"].ToString();
    var password = req.Form["password"].ToString();

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
      ContentType = "text/html"
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