


using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Common.OpenApi;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class ListEgressMaterials(ILogger<ListEgressMaterials> logger,
    IEgressClient egressClient,
    IEgressArgFactory egressArgFactory)
{
    private readonly ILogger<ListEgressMaterials> _logger = logger;
    private readonly IEgressClient _egressClient = egressClient;
    private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;
    [Function(nameof(ListEgressMaterials))]
    [OpenApiOperation(operationId: nameof(ListEgressMaterials), tags: ["Egress"], Description = "Lists files and folders in Egress for a workspace.")]
    [OpenApiSecurity("implicit_auth", SecuritySchemeType.OAuth2, Flows = typeof(ImplicitAuthFlow))]
    [OpenApiParameter(name: InputParameters.FolderId, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The folder-id to search for files/folders within.")]
    [OpenApiParameter(name: InputParameters.Skip, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to skip.")]
    [OpenApiParameter(name: InputParameters.Take, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to take.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/egress/workspaces/{workspaceId}/files")] HttpRequest req, FunctionContext context, string workspaceId)
    {
        var folderId = req.Query[InputParameters.FolderId];
        var skip = int.TryParse(req.Query[InputParameters.Skip], out var skipValue) ? skipValue : 0;
        var take = int.TryParse(req.Query[InputParameters.Take], out var takeValue) ? takeValue : 100;

        var listMaterialsArg = _egressArgFactory.CreateListWorkspaceMaterialArg(workspaceId, skip, take, folderId, null);
        var permissionsArg = _egressArgFactory.CreateGetWorkspacePermissionArg(workspaceId, context.GetRequestContext().Username);

        try
        {
            var hasEgressPermission = await _egressClient.GetWorkspacePermission(permissionsArg);

            if (!hasEgressPermission)
            {
                return new UnauthorizedResult();
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }

        var response = await _egressClient.ListCaseMaterialAsync(listMaterialsArg);

        return new OkObjectResult(response);
    }
}