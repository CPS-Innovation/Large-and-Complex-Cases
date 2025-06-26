using System.Net;
using System.Reflection;
using CPS.ComplexCases.API.Attributes;
using CPS.ComplexCases.API.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using CPS.ComplexCases.API.Constants;

namespace CPS.ComplexCases.API.Functions;

public static class Status
{
  [Function(nameof(Status))]
  [OpenApiOperation(operationId: nameof(Status), tags: ["Health"], Description = "Gets the current status of the function app.")]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
  public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
  {
    return Assembly.GetExecutingAssembly().CurrentStatus();
  }
}