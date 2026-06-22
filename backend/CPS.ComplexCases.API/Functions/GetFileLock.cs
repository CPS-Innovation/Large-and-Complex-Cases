using System.Net;
using CPS.ComplexCases.NetApp.Models.Requests;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class GetFileLock(ILogger<GetFileLock> logger)
{
    private readonly ILogger<GetFileLock> _logger = logger;

    [Function("GetFileLock")]
    [OpenApiOperation(operationId: "GetFileLock", tags: new[] { "NetApp", "Material" }, Summary = "Gets the lock information for a specified file.", Description = "Returns details about the lock on a file, including who holds the lock.")]
    [OpenApiParameter(name: "volumeName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "The name of the volume containing the file.", Description = "Specify the volume name where the file is located.")]
    [OpenApiParameter(name: "filePath", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "The path to the file within the volume.", Description = "Provide the full path to the file for which you want to retrieve lock information.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GetFileLockRequestDto), Summary = "Successful response with file lock information.", Description = "Returns a JSON object containing details about the file lock.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid request parameters.", Description = "Occurs when required query parameters are missing or invalid.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized access.", Description = "Occurs when authentication fails or credentials are missing.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Summary = "Forbidden access.", Description = "Occurs when the user does not have permission to access the resource.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "File not found.", Description = "Occurs when the specified file does not exist in the given volume.")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req)
    {
        return req.CreateResponse(HttpStatusCode.OK);
    }
}