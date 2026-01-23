using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class DeleteNetAppFileOrFolder(ILogger<DeleteNetAppFileOrFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<DeleteNetAppFileOrFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(DeleteNetAppFileOrFolder))]
    [OpenApiOperation(operationId: nameof(DeleteNetAppFileOrFolder), tags: ["NetApp"], Description = "Delete a file or folder in NetApp.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(DeleteNetAppFileOrFolderDto), Description = "Body containing the NetApp file or folder to delete")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/cases/{operationName}/netapp")] HttpRequest req, string operationName, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var deleteNetAppFileOrFolderRequest = await _requestValidator.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(req);

        if (!deleteNetAppFileOrFolderRequest.IsValid)
        {
            return new BadRequestObjectResult(deleteNetAppFileOrFolderRequest.ValidationErrors);
        }

        if (string.IsNullOrWhiteSpace(operationName))
            return new BadRequestObjectResult("Operation name cannot be empty");

        if (operationName.Contains("..") || operationName.StartsWith('/'))
            return new BadRequestObjectResult("Invalid operation name");

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var arg = _netAppArgFactory.CreateDeleteFileOrFolderArg(context.BearerToken, securityGroups.First().BucketName, operationName, operationName + "/" + deleteNetAppFileOrFolderRequest.Value.Path);
        var result = await _netAppClient.DeleteFileOrFolderAsync(arg);

        return new OkObjectResult(result);
    }
}