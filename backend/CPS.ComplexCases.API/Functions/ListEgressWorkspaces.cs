using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class ListEgressWorkspaces(ILogger<ListEgressWorkspaces> logger,
  IEgressClient egressClient,
  IEgressArgFactory egressArgFactory)
{
  private readonly ILogger<ListEgressWorkspaces> _logger = logger;
  private readonly IEgressClient _egressClient = egressClient;
  private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;

  [Function(nameof(ListEgressWorkspaces))]
  [OpenApiOperation(operationId: nameof(ListEgressWorkspaces), tags: ["Egress"], Description = "Lists workspaces in Egress based on name.")]
  [OpenApiParameter(name: InputParameters.WorkspaceName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The workspace name to search for.")]
  [OpenApiParameter(name: InputParameters.Skip, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to skip.")]
  [OpenApiParameter(name: InputParameters.Take, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to take.")]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]

  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "egress/workspaces")] HttpRequest req, FunctionContext context)
  {
    var operationName = req.Query[InputParameters.WorkspaceName];
    var skip = int.TryParse(req.Query[InputParameters.Skip], out var skipValue) ? skipValue : 0;
    var take = int.TryParse(req.Query[InputParameters.Take], out var takeValue) ? takeValue : 100;

    var egressArg = _egressArgFactory.CreateListEgressWorkspacesArg(operationName, skip, take);

    var response = await _egressClient.ListWorkspacesAsync(egressArg, context.GetRequestContext().Username);

    return new OkObjectResult(response);
  }
}