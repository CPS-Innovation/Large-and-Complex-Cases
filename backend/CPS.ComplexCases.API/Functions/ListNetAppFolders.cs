using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.API.Domain.Response;

namespace CPS.ComplexCases.API.Functions;

public class ListNetAppFolders(ILogger<ListNetAppFolders> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    ICaseEnrichmentService caseEnrichmentService,
    IOptions<NetAppOptions> options)
{
    private readonly ILogger<ListNetAppFolders> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ICaseEnrichmentService _caseEnrichmentService = caseEnrichmentService;
    private readonly NetAppOptions _netAppOptions = options.Value;

    [Function(nameof(ListNetAppFolders))]
    [OpenApiOperation(operationId: nameof(ListNetAppFolders), tags: ["NetApp"], Description = "Lists folders in NetApp, initially based on operation name.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.OperationName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The operation name to search for.")]
    [OpenApiParameter(name: InputParameters.Path, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The path to the destination folder.")]
    [OpenApiParameter(name: InputParameters.Take, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to take.")]
    [OpenApiParameter(name: InputParameters.ContinuationToken, In = ParameterLocation.Query, Type = typeof(string), Description = "The continuation token for pagination.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(ListNetAppObjectsResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/netapp/folders")] HttpRequest req)
    {
        var operationName = req.Query[InputParameters.OperationName];
        var continuationToken = req.Query[InputParameters.ContinuationToken];
        var take = int.TryParse(req.Query[InputParameters.Take], out var takeValue) ? takeValue : 100;
        var path = req.Query[InputParameters.Path];

        var arg = _netAppArgFactory.CreateListFoldersInBucketArg(_netAppOptions.BucketName, operationName, continuationToken, take, path);
        var response = await _netAppClient.ListFoldersInBucketAsync(arg);

        if (response == null)
        {
            return new BadRequestResult();
        }

        var enrichedResponse = await _caseEnrichmentService.EnrichNetAppFoldersWithMetadataAsync(response);

        return new OkObjectResult(enrichedResponse);
    }
}