
using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class GetCase(ILogger<GetCase> logger,
  ICaseMetadataService caseClient,
  IDdeiClient ddeiClient,
  IDdeiArgFactory ddeiArgFactory)
{
    private readonly ILogger<GetCase> _logger = logger;
    private readonly ICaseMetadataService _caseClient = caseClient;
    private readonly IDdeiClient _ddeiClient = ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

    [Function(nameof(GetCase))]
    [OpenApiOperation(operationId: nameof(GetCase), tags: ["Cases"], Description = "Gets a case by ID from metadata service.")]
    [FunctionKeyAuth]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: "caseId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The CMS case ID.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(CaseWithMetadataResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/cases/{caseId}")] HttpRequest req, FunctionContext context, int caseId)
    {
        var caseResponse = await _caseClient.GetCaseMetadataForCaseIdAsync(caseId);

        if (caseResponse == null)
        {
            return new NotFoundObjectResult($"Case with ID {caseId} not found.");
        }

        var cmsArg = _ddeiArgFactory.CreateCaseArg(context.GetRequestContext().CmsAuthValues, context.GetRequestContext().CorrelationId, caseId);
        var cmsResponse = await _ddeiClient.GetCaseAsync(cmsArg);

        var response = new CaseWithMetadataResponse
        {
            CaseId = caseResponse.CaseId,
            EgressWorkspaceId = caseResponse.EgressWorkspaceId,
            NetappFolderPath = caseResponse.NetappFolderPath,
            Urn = cmsResponse.Urn,
            OperationName = cmsResponse.OperationName,
            ActiveTransferId = caseResponse.ActiveTransferId,
        };

        return new OkObjectResult(response);
    }
}