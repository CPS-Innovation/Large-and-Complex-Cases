using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Handlers;
using CPS.ComplexCases.Common.Attributes;

using ContentType = CPS.ComplexCases.API.Constants.ContentType;
using ApiResponseDescriptions = CPS.ComplexCases.API.Constants.ApiResponseDescriptions;

namespace CPS.ComplexCases.API.Functions;

public class DisconnectNetAppConnection(IDisconnectConnectionHandler disconnectConnectionHandler)
{
    private readonly IDisconnectConnectionHandler _disconnectConnectionHandler = disconnectConnectionHandler;

    [Function(nameof(DisconnectNetAppConnection))]
    [OpenApiOperation(operationId: nameof(DisconnectNetAppConnection), tags: ["NetApp"], Description = "Disconnect a NetApp folder from a case.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.CaseId, In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "The case ID to disconnect from.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/netapp/connections")] HttpRequest req, FunctionContext functionContext)
    {
        return await _disconnectConnectionHandler.RunAsync(
            req,
            functionContext,
            StorageConnectionType.NetApp);
    }
}