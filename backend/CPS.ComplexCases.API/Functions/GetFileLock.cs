using System.Net;
using System.Text.Json;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Requests;
using CPS.ComplexCases.NetApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class GetFileLock(
    ILogger<GetFileLock> logger,
    ICaseMetadataService caseMetadataService,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IOntapArgFactory ontapArgFactory,
    IOntapService ontapService)
{
    private readonly ILogger<GetFileLock> _logger = logger;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IOntapArgFactory _ontapArgFactory = ontapArgFactory;
    private readonly IOntapService _ontapService = ontapService;

    [Function("GetFileLock")]
    [OpenApiOperation(operationId: "GetFileLock", tags: new[] { "NetApp" }, Summary = "Gets the lock information for a specified file.", Description = "Returns details about the lock on a file, including who holds the lock.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GetFileLockRequestDto), Summary = "Successful response with file lock information.", Description = "Returns a JSON object containing details about the file lock.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid request parameters.", Description = "Occurs when required query parameters are missing or invalid.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized access.", Description = "Occurs when authentication fails or credentials are missing.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Summary = "Forbidden access.", Description = "Occurs when the user does not have permission to access the resource.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "File not found.", Description = "Occurs when the specified file does not exist in the given volume.")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/netapp/filelock")] HttpRequestData req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        GetFileLockRequestDto getFileLockRequest;

        try
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            getFileLockRequest = JsonSerializer.Deserialize<GetFileLockRequestDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Request body is invalid.");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize request body.");
            return new BadRequestObjectResult("Invalid request body format.");
        }

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(getFileLockRequest.CaseId);

        if (caseMetadata == null || string.IsNullOrEmpty(caseMetadata.NetappFolderPath))
            return new BadRequestObjectResult("Case metadata or NetApp folder path is missing.");

        var casePrefix = caseMetadata.NetappFolderPath.EndsWith('/')
            ? caseMetadata.NetappFolderPath
            : caseMetadata.NetappFolderPath + "/";

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var bucketName = securityGroups[0].BucketName;
        var volumeUuid = securityGroups[0].VolumeUuid;

        if (!getFileLockRequest.Path.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
            return new BadRequestObjectResult("File path is outside the allowed case folder.");

        var arg = _ontapArgFactory.CreateGetFileLockArg(context.BearerToken, bucketName, volumeUuid, getFileLockRequest.Path);
        var result = await _ontapService.GetFileLockAsync(arg);

        return new OkObjectResult(result);
    }
}