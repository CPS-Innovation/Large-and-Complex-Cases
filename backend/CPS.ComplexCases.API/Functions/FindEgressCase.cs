using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class FindEgressCase(ILogger<FindEgressCase> logger,
  IEgressClient egressClient,
  IEgressArgFactory egressArgFactory)
{
  private readonly ILogger<FindEgressCase> _logger = logger;
  private readonly IEgressClient _egressClient = egressClient;
  private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;

  [Function(nameof(FindEgressCase))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/egress")] HttpRequest req, FunctionContext context)
  {
    var operationName = req.Query["operationName"];

    var egressArg = _egressArgFactory.CreateFindWorkspaceArg(operationName);

    // todo: change this to the username out of the context
    var response = await _egressClient.FindWorkspace(egressArg, context.GetRequestContext().Username);

    return new OkObjectResult(response);
  }
}