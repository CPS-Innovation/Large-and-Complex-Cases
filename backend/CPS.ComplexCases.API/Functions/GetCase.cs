
using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class GetCase(ILogger<GetCase> logger,
  ICaseMetadataService caseClient)
{
    private readonly ILogger<GetCase> _logger = logger;
    private readonly ICaseMetadataService _caseClient = caseClient;

    [Function(nameof(GetCase))]
    [OpenApiOperation(operationId: nameof(GetCase), tags: ["Cases"], Description = "Gets a case by ID from metadata service.")]
    [OpenApiParameter(name: "caseId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The CMS case ID.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/{caseId}")] HttpRequest req, FunctionContext context, int caseId)
    {
        var caseResponse = await _caseClient.GetCaseMetadataForCaseIdAsync(caseId);

        if (caseResponse == null)
        {
            return new NotFoundObjectResult($"Case with ID {caseId} not found.");
        }

        return new OkObjectResult(caseResponse);
    }
}