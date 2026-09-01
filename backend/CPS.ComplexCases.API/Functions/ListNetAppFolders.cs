using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class ListNetAppFolders(ILogger<ListNetAppFolders> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    ICaseEnrichmentService caseEnrichmentService,
    IUserBucketAccessService userBucketAccessService,
    ICaseMetadataService caseMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<ListNetAppFolders> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ICaseEnrichmentService _caseEnrichmentService = caseEnrichmentService;
    private readonly IUserBucketAccessService _userBucketAccessService = userBucketAccessService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(ListNetAppFolders))]
    [OpenApiOperation(operationId: nameof(ListNetAppFolders), tags: ["NetApp"], Description = "Lists folders in NetApp, initially based on operation name.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.OperationName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The operation name to search for.")]
    [OpenApiParameter(name: InputParameters.Path, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The path to the destination folder.")]
    [OpenApiParameter(name: InputParameters.Take, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to take.")]
    [OpenApiParameter(name: InputParameters.ContinuationToken, In = ParameterLocation.Query, Type = typeof(string), Description = "The continuation token for pagination.")]
    [OpenApiParameter(name: InputParameters.CaseId, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The case ID, used to read the bucket already connected to the case.")]
    [OpenApiParameter(name: InputParameters.BucketName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The bucket to browse, for use before the case has a connected bucket.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(ListNetAppObjectsResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/netapp/folders")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var operationName = req.Query[InputParameters.OperationName];
        var continuationToken = req.Query[InputParameters.ContinuationToken];
        var take = int.TryParse(req.Query[InputParameters.Take], out var takeValue) ? takeValue : 100;
        var path = req.Query[InputParameters.Path];
        var requestedBucketName = req.Query[InputParameters.BucketName].FirstOrDefault();

        string? persistedBucketName = null;
        if (int.TryParse(req.Query[InputParameters.CaseId], out var caseId) && caseId > 0)
        {
            var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(caseId);
            persistedBucketName = caseMetadata?.NetappBucketName;
        }

        var bucket = await _userBucketAccessService.ResolveBucketAsync(
            context.BearerToken, persistedBucketName, requestedBucketName);

        var arg = _netAppArgFactory.CreateListFoldersInBucketArg(context.BearerToken, bucket.BucketName, operationName, continuationToken, take, path);
        var response = await _netAppClient.ListFoldersInBucketAsync(arg);

        if (response == null)
        {
            return new BadRequestResult();
        }

        var enrichedResponse = await _caseEnrichmentService.EnrichNetAppFoldersWithMetadataAsync(response);

        return new OkObjectResult(enrichedResponse);
    }
}