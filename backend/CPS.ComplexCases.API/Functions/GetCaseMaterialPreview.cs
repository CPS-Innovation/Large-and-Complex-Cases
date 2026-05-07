using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;

namespace CPS.ComplexCases.API.Functions;

public class GetCaseMaterialPreview(
    ILogger<GetCaseMaterialPreview> logger,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IDocumentService documentService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<GetCaseMaterialPreview> _logger = logger;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IDocumentService _documentService = documentService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(GetCaseMaterialPreview))]
    [OpenApiOperation(operationId: nameof(GetCaseMaterialPreview), tags: ["NetApp"], Description = "Retrieves a case material document from NetApp and returns it as a PDF preview.")]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.Path, In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The NetApp path of the document to preview.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/pdf", bodyType: typeof(FileStreamResult), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/netapp/preview")] HttpRequest req,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var path = req.Query[InputParameters.Path].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(path))
        {
            return new BadRequestObjectResult($"Query parameter '{InputParameters.Path}' is required.");
        }

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var bucketName = securityGroups.First().BucketName;

        var result = await _documentService.GetMaterialPreviewAsync(path, context.BearerToken, bucketName);

        if (result == null)
        {
            _logger.LogWarning("No document found for path [{Path}]", path);
            return new NotFoundObjectResult($"No document found at path [{path}].");
        }

        return result;
    }
}
