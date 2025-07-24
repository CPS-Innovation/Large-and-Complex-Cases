using System.Net;
using CPS.ComplexCases.Common.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace CPS.ComplexCases.FileTransfer.API.Middleware;

public sealed partial class RequestValidationMiddleware() : IFunctionsWorkerMiddleware
{
  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var httpRequestData = await context.GetHttpRequestDataAsync() ?? throw new ArgumentNullException(nameof(context), "Context does not contains HttpRequestData");

    // Only block Swagger in production
    if (SwaggerRouteHelper.IsProduction && SwaggerRouteHelper.IsSwaggerRoute(httpRequestData.Url.AbsolutePath))
    {
      var response = httpRequestData.CreateResponse(HttpStatusCode.NotFound);
      await response.WriteStringAsync("Not Found");
      context.GetInvocationResult().Value = response;
      return;
    }

    await next(context);
  }
}