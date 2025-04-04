using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class FindNetAppFolder(ILogger<FindNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory)
{
    private readonly ILogger<FindNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    [Function(nameof(FindNetAppFolder))]
    [OpenApiOperation(operationId: nameof(FindNetAppFolder), tags: ["NetApp"], Description = "Finds a case in NetApp based on operation name.")]
    // TODO: move the operation name to be a query string parameter.
    // [OpenApiParameter(name: QueryStringParameters.OperationName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The operation name to search for.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/{operationName}/netapp")] HttpRequest req, string operationName)
    {
        var arg = _netAppArgFactory.CreateFindBucketArg(operationName!);
        var result = await _netAppClient.FindBucketAsync(arg);

        if (result == null)
        {
            return new NotFoundObjectResult($"Bucket {operationName} not found");
        }

        return new OkObjectResult(result);
    }
}