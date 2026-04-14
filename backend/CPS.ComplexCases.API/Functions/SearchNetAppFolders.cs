using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Enums;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Models.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class SearchNetAppFolders(
    ILogger<SearchNetAppFolders> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    ICaseMetadataService caseMetadataService,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IInitializationHandler initializationHandler,
    IValidator<SearchNetAppFoldersDto> validator)
{
    private readonly ILogger<SearchNetAppFolders> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IValidator<SearchNetAppFoldersDto> _validator = validator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(SearchNetAppFolders))]
    [OpenApiOperation(operationId: nameof(SearchNetAppFolders), tags: ["NetApp"], Description = "Search for files and folders in NetApp folders.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.CaseId, In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The case ID to search within.")]
    [OpenApiParameter(name: InputParameters.Query, In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The query to search for.")]
    [OpenApiParameter(name: InputParameters.Mode, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The mode to use for the search (prefix or substring).")]
    [OpenApiParameter(name: InputParameters.MaxResults, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The maximum number of results to return.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(SearchResultsDto), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/netapp/search")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var request = new SearchNetAppFoldersDto
        {
            CaseId = int.TryParse(req.Query[InputParameters.CaseId], out var caseIdValue) ? caseIdValue : 0,
            Query = req.Query[InputParameters.Query],
            Mode = Enum.TryParse<SearchModes>(req.Query[InputParameters.Mode], true, out var modeValue) ? modeValue : SearchModes.Prefix,
            MaxResults = int.TryParse(req.Query[InputParameters.MaxResults], out var maxResultsValue) ? maxResultsValue : 1000
        };

        _logger.LogInformation("Received request to search NetApp folders for CaseId: {CaseId}", request.CaseId);

        var validationErrors = _validator.Validate(request).Errors;
        if (validationErrors.Count != 0)
        {
            return new BadRequestObjectResult(string.Join(", ", validationErrors.Select(e => e.ErrorMessage)));
        }

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(request.CaseId);

        if (caseMetadata == null || string.IsNullOrEmpty(caseMetadata.NetappFolderPath))
        {
            return new BadRequestObjectResult("Case metadata or NetApp folder path is missing.");
        }

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var arg = _netAppArgFactory.CreateSearchArg(context.BearerToken, securityGroups.First().BucketName, caseMetadata.NetappFolderPath!, request.Query, request.MaxResults, request.Mode);
        var response = await _netAppClient.SearchObjectsInBucketAsync(arg);

        return new OkObjectResult(response);
    }
}
