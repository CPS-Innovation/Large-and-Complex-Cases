using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class GetNetAppMaterialTags(
    ILogger<GetNetAppMaterialTags> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<GetNetAppMaterialTags> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(GetNetAppMaterialTags))]
    [OpenApiOperation(operationId: nameof(GetNetAppMaterialTags), tags: ["NetApp"], Description = "Returns the S3 object tags for a material file in NetApp.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.Path, In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Relative path to the file from the operation root, e.g. Folder/document.pdf")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(Dictionary<string, string>), Description = "Dictionary of tag key/value pairs on the object.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/cases/{operationName}/netapp/material/tags")] HttpRequest req,
        string operationName,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        if (string.IsNullOrWhiteSpace(operationName))
            return new BadRequestObjectResult("Operation name cannot be empty.");

        if (operationName.Contains("..") || operationName.StartsWith('/'))
            return new BadRequestObjectResult("Invalid operation name.");

        var path = req.Query[InputParameters.Path].ToString();

        if (string.IsNullOrWhiteSpace(path))
            return new BadRequestObjectResult("'path' query parameter is required.");

        if (path.Contains("..") || path.StartsWith('/'))
            return new BadRequestObjectResult("Invalid path.");

        var objectKey = $"{operationName}/{path}";

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var bucketName = securityGroups.First().BucketName;

        var existsArg = _netAppArgFactory.CreateGetObjectArg(context.BearerToken, bucketName, objectKey);
        if (!await _netAppClient.DoesObjectExistAsync(existsArg))
        {
            _logger.LogWarning("GetNetAppMaterialTags: object not found. OperationName={OperationName}, ObjectKey={ObjectKey}", operationName, objectKey);
            return new NotFoundObjectResult($"File '{path}' not found.");
        }

        _logger.LogInformation("Fetching tags for object. OperationName={OperationName}, ObjectKey={ObjectKey}", operationName, objectKey);

        var tagsArg = _netAppArgFactory.CreateGetObjectArg(context.BearerToken, bucketName, objectKey);
        var tags = await _netAppClient.GetObjectTaggingAsync(tagsArg);

        return new OkObjectResult(tags);
    }
}
