using System.Diagnostics;
using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Dto;
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

        ListNetAppObjectsDto? response;
        try
        {
            response = await _netAppClient.ListFoldersInBucketAsync(arg);
        }
        catch (NetAppAccessDeniedException) when (string.IsNullOrEmpty(path) && bucket.NormalisedEntryPrefixes.Count > 0)
        {
            // The caller's NTFS permissions are scoped to subfolders, so the bucket root is not
            // listable. Offer the configured entry prefixes they can actually reach instead.
            return await ListAccessibleEntryPrefixesAsync(context.BearerToken, bucket);
        }

        if (response == null)
        {
            return new BadRequestResult();
        }

        var enrichedResponse = await _caseEnrichmentService.EnrichNetAppFoldersWithMetadataAsync(response);

        return new OkObjectResult(enrichedResponse);
    }

    private async Task<IActionResult> ListAccessibleEntryPrefixesAsync(string bearerToken, SecurityGroup bucket)
    {
        var entryPrefixes = bucket.NormalisedEntryPrefixes;
        var stopwatch = Stopwatch.StartNew();

        var probeResults = await Task.WhenAll(entryPrefixes.Select(async prefix =>
        {
            var probeArg = _netAppArgFactory.CreateListFoldersInBucketArg(
                bearerToken, bucket.BucketName, maxKeys: 1, prefix: prefix);
            return (Prefix: prefix, IsAccessible: await _netAppClient.CanListPrefixAsync(probeArg));
        }));

        stopwatch.Stop();

        var accessiblePrefixes = probeResults
            .Where(result => result.IsAccessible)
            .Select(result => result.Prefix)
            .ToList();

        _logger.LogInformation(
            "Bucket root listing was denied for bucket {BucketName}. Probed {ProbeCount} entry prefixes in {ElapsedMs}ms and found {AccessibleCount} accessible: {AccessiblePrefixes}",
            bucket.BucketName, entryPrefixes.Count, stopwatch.ElapsedMilliseconds, accessiblePrefixes.Count,
            string.Join(", ", accessiblePrefixes));

        if (accessiblePrefixes.Count == 0)
        {
            return new ObjectResult("No accessible folders found in this bucket")
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        var entryPrefixResponse = new ListNetAppObjectsDto
        {
            Data = new ListNetAppDataDto
            {
                BucketName = bucket.BucketName,
                RootPath = string.Empty,
                FolderData = accessiblePrefixes.Select(prefix => new ListNetAppFolderDataDto { Path = prefix }),
                FileData = []
            },
            Pagination = new PaginationDto
            {
                ContinuationToken = null,
                NextContinuationToken = null,
                MaxKeys = accessiblePrefixes.Count,
                KeyCount = accessiblePrefixes.Count
            }
        };

        var enrichedResponse = await _caseEnrichmentService.EnrichNetAppFoldersWithMetadataAsync(entryPrefixResponse);
        enrichedResponse.Data.IsRestrictedRoot = true;
        enrichedResponse.Data.AccessibleRoots = accessiblePrefixes;

        return new OkObjectResult(enrichedResponse);
    }
}
