using System.Net;
using System.Reflection;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Functions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.OpenApi.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class Status(ILogger<Status> logger)
{
    private readonly ILogger<Status> _logger = logger;
  
    [Function(nameof(Status))]
    [OpenApiOperation(operationId: nameof(Status), tags: ["Health"], Description = "Gets the current status of the FileTransfer function app.")]
    [OpenApiNoSecurity]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(AssemblyStatus), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
    {
        _logger.LogDebug("Calling the FileTransfer Status EndPoint.");
        return StatusFunction.GetStatus(Assembly.GetExecutingAssembly());
    }
}