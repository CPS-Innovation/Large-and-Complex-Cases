using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
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

public class ListNetAppTemplates(ILogger<ListNetAppTemplates> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IUserBucketAccessService userBucketAccessService,
    ICaseMetadataService caseMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<ListNetAppTemplates> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IUserBucketAccessService _userBucketAccessService = userBucketAccessService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(ListNetAppTemplates))]
    [OpenApiOperation(operationId: nameof(ListNetAppTemplates), tags: ["NetApp"], Description = "Lists the case folder templates in the _templates/ folder directly under the given folder.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.Path, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The parent folder the case folder will be created in. Leave empty for the bucket root.")]
    [OpenApiParameter(name: InputParameters.CaseId, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The case ID, used to read the bucket already connected to the case.")]
    [OpenApiParameter(name: InputParameters.BucketName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The bucket to browse, for use before the case has a connected bucket.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(ListNetAppTemplatesResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/netapp/templates")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var path = req.Query[InputParameters.Path].FirstOrDefault();
        var requestedBucketName = req.Query[InputParameters.BucketName].FirstOrDefault();

        if (!string.IsNullOrEmpty(path) && path.Contains(".."))
        {
            return new BadRequestObjectResult("Path cannot contain '..' to navigate up directories.");
        }

        if (NetAppTemplatePathHelper.IsWithinTemplatesFolder(path))
        {
            return new BadRequestObjectResult("Case folders cannot be created inside a _templates folder.");
        }

        var parentFolderPath = NetAppTemplatePathHelper.NormaliseParentFolderPath(path);

        string? persistedBucketName = null;
        if (int.TryParse(req.Query[InputParameters.CaseId], out var caseId) && caseId > 0)
        {
            var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(caseId);
            persistedBucketName = caseMetadata?.NetappBucketName;
        }

        var bucket = await _userBucketAccessService.ResolveBucketAsync(
            context.BearerToken, persistedBucketName, requestedBucketName);

        var templatesFolderPath = NetAppTemplatePathHelper.GetTemplatesFolderPath(parentFolderPath);
        var templatePaths = new List<string>();

        string? continuationToken = null;
        do
        {
            var arg = _netAppArgFactory.CreateListFoldersInBucketArg(
                context.BearerToken, bucket.BucketName, continuationToken: continuationToken, prefix: templatesFolderPath);

            var response = await _netAppClient.ListFoldersInBucketAsync(arg);

            if (response == null)
            {
                _logger.LogWarning("Unable to list templates at {TemplatesFolderPath} in bucket {BucketName}. Returning no templates.",
                    templatesFolderPath, bucket.BucketName);
                break;
            }

            templatePaths.AddRange(response.Data.FolderData
                .Select(folder => folder.Path)
                .Where(folderPath => !string.IsNullOrEmpty(folderPath)
                    && NetAppTemplatePathHelper.IsTemplateUnderParent(folderPath, parentFolderPath))!);

            continuationToken = response.Pagination.NextContinuationToken;
        } while (!string.IsNullOrEmpty(continuationToken));

        var templates = templatePaths
            .Select(templatePath => new NetAppTemplateResponse
            {
                Name = NetAppTemplatePathHelper.GetFolderName(templatePath),
                Path = templatePath
            })
            .OrderBy(template => template.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _logger.LogInformation("Found {TemplateCount} templates at {TemplatesFolderPath} in bucket {BucketName}.",
            templates.Count, templatesFolderPath, bucket.BucketName);

        return new OkObjectResult(new ListNetAppTemplatesResponse
        {
            ParentFolderPath = parentFolderPath,
            Templates = templates,
            Count = templates.Count
        });
    }
}
