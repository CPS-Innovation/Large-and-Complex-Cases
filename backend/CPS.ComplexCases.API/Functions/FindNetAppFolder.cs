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

namespace CPS.ComplexCases.API.Functions;

public class FindNetAppFolder(ILogger<FindNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    ICaseEnrichmentService caseEnrichmentService,
    IOptions<NetAppOptions> netAppOptions)
{
    private readonly ILogger<FindNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ICaseEnrichmentService _caseEnrichmentService = caseEnrichmentService;
    private readonly NetAppOptions _netAppOptions = netAppOptions.Value;

    [Function(nameof(FindNetAppFolder))]
    [OpenApiOperation(operationId: nameof(FindNetAppFolder), tags: ["NetApp"], Description = "Finds a case in NetApp based on operation name.")]
    [OpenApiParameter(name: InputParameters.OperationName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The operation name to search for.")]
    [OpenApiParameter(name: InputParameters.Path, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "")]
    [OpenApiParameter(name: InputParameters.Take, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "")]
    [OpenApiParameter(name: InputParameters.ContinuationToken, In = ParameterLocation.Query, Type = typeof(string), Description = "")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "netapp/folders")] HttpRequest req)
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