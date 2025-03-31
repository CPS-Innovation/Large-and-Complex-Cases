using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;

namespace CPS.ComplexCases.API.Functions;

public class FindEgressCase(ILogger<FindEgressCase> logger,
  IEgressClient egressClient,
  IEgressArgFactory egressArgFactory)
{
  private readonly ILogger<FindEgressCase> _logger = logger;
  private readonly IEgressClient _egressClient = egressClient;
  
  private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;

  [Function(nameof(FindEgressCase))]
  [OpenApiOperation(operationId: nameof(FindEgressCase), tags: ["Egress", "Search"], Description = "Finds a case in Egress based on operation name.")]
  [OpenApiParameter(name: InputParameters.OperationName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The operation name to search for.")]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/egress")] HttpRequest req, FunctionContext context)
  {
    var operationName = req.Query[InputParameters.OperationName];

    var egressArg = _egressArgFactory.CreateFindWorkspaceArg(operationName);

    // todo: change this to the username out of the context
    var response = await _egressClient.FindWorkspace(egressArg, context.GetRequestContext().Username);

    return new OkObjectResult(response);
  }
}