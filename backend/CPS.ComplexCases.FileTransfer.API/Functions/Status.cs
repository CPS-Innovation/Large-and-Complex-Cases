using System.Net;
using System.Reflection;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Functions;
using CPS.ComplexCases.Common.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public static class Status
{
    [Function(nameof(Status))]
    [OpenApiOperation(operationId: nameof(Status), tags: ["Health"], Description = "Gets the current status of the FileTransfer function app.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(AssemblyStatus), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
    {
        return StatusFunction.GetStatus(Assembly.GetExecutingAssembly());
    }
}