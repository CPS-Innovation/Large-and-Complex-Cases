using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;

namespace CPS.ComplexCases.API.Functions;

public class FindEgressCase(ILogger<FindEgressCase> logger, IEgressClient egressClient, IEgressArgFactory egressArgFactory)
{
  private readonly ILogger<FindEgressCase> _logger = logger;
  private readonly IEgressClient _egressClient = egressClient;
  private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;

  [Function(nameof(FindEgressCase))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/{operationName}/egress")] HttpRequest req, string operationName)
  {
    try
    {
      var egressArg = _egressArgFactory.CreateFindWorkspaceArg(operationName);

      // todo: once msal auth is in place, replace the email with the authenticated user's email
      var response = await _egressClient.FindWorkspace(egressArg, "integration@cps.gov.uk");

      return new OkObjectResult(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"An error occurred while searching for case with name: {operationName}");
      return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
  }
}