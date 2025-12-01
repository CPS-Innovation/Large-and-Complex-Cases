using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class CreateNetAppFolder(ILogger<CreateNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory)
{
    private readonly ILogger<CreateNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    [Function(nameof(CreateNetAppFolder))]
    [OpenApiOperation(operationId: nameof(CreateEgressConnection), tags: ["NetApp"], Description = "Create a folder in NetApp.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/cases/{operationName}/netapp")] HttpRequest req, string operationName, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        var arg = _netAppArgFactory.CreateFindBucketArg(context.BearerToken, operationName!);
        var result = await _netAppClient.FindBucketAsync(arg);

        if (result == null)
        {
            return new NotFoundObjectResult($"Bucket {operationName} not found");
        }

        return new OkObjectResult(result);
    }
}