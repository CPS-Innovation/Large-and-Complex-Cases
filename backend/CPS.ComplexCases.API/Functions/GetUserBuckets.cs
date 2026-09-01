using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class GetUserBuckets(
    ILogger<GetUserBuckets> logger,
    IUserBucketAccessService userBucketAccessService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<GetUserBuckets> _logger = logger;
    private readonly IUserBucketAccessService _userBucketAccessService = userBucketAccessService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(GetUserBuckets))]
    [OpenApiOperation(operationId: nameof(GetUserBuckets), tags: ["NetApp"], Description = "Lists the NetApp buckets the user is entitled to, based on their token groups claim.")]
    [BearerTokenAuth]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(ListUserBucketsResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/netapp/buckets")] HttpRequest req,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var entitledBuckets = await _userBucketAccessService.GetEntitledBucketsAsync(context.BearerToken);

        _logger.LogInformation("Resolved {BucketCount} entitled NetApp buckets for the user.", entitledBuckets.Count);

        return new OkObjectResult(new ListUserBucketsResponse
        {
            Buckets = entitledBuckets.Select(bucket => new UserBucketResponse
            {
                Id = bucket.Id,
                Name = bucket.BucketName,
                DisplayName = bucket.DisplayName
            }).ToList()
        });
    }
}
