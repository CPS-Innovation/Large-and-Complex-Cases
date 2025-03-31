using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class ListEgressWorkspaces(ILogger<ListEgressWorkspaces> logger,
  IEgressClient egressClient,
  IEgressArgFactory egressArgFactory)
{
  private readonly ILogger<ListEgressWorkspaces> _logger = logger;
  private readonly IEgressClient _egressClient = egressClient;
  private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;

  [Function(nameof(ListEgressWorkspaces))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "egress/workspaces")] HttpRequest req, FunctionContext context)
  {
    var operationName = req.Query["workspace-name"];
    var skip = int.TryParse(req.Query["skip"], out var skipValue) ? skipValue : 0;
    var take = int.TryParse(req.Query["take"], out var takeValue) ? takeValue : 100;

    var egressArg = _egressArgFactory.CreateListEgressWorkspacesArg(operationName, skip, take);

    var response = await _egressClient.ListWorkspacesAsync(egressArg, context.GetRequestContext().Username);

    return new OkObjectResult(response);
  }
}