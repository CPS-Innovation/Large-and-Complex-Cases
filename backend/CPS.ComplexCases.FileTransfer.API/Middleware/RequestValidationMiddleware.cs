using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace CPS.ComplexCases.FileTransfer.API.Middleware;

public sealed partial class RequestValidationMiddleware() : IFunctionsWorkerMiddleware
{
    private static string[] _blockedSwaggerRoutes = [
      "/api/swagger/ui",
    "/api/swagger.json",
    "/api/swagger"
  ];
    private static readonly string? Environment = System.Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
    private static readonly bool IsProduction = !string.IsNullOrEmpty(Environment) && string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase);


    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpRequestData = await context.GetHttpRequestDataAsync() ?? throw new ArgumentNullException(nameof(context), "Context does not contains HttpRequestData");

        // Only block Swagger in production
        if (IsProduction && IsSwaggerRoute(httpRequestData.Url.AbsolutePath))
        {
            var response = httpRequestData.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteStringAsync("Not Found");
            context.GetInvocationResult().Value = response;
            return;
        }

        await next(context);
    }

    private static bool IsSwaggerRoute(string path)
    {
        return _blockedSwaggerRoutes.Any(route =>
            path.StartsWith(route, StringComparison.OrdinalIgnoreCase));
    }
}