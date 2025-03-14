using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Functions.Tactical;

public class CmsLoginDirect()
{
  [Function(nameof(CmsLoginDirectGet))]
  public IActionResult CmsLoginDirectGet([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tactical/login")] HttpRequest req) => new ContentResult
  {
    Content = HtmlHelpers.LoginForm(),
    ContentType = "text/html"
  };
}