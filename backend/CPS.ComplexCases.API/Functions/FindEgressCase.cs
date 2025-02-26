using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.API.Handlers;

namespace CPS.ComplexCases.API.Functions;

public class FindEgressCase(ILogger<FindEgressCase> logger,
  IEgressClient egressClient,
  IEgressArgFactory egressArgFactory,
  IInitializationHandler initializationHandler,
  IUnhandledExceptionHandler unhandledExceptionHandler)
{
  private readonly ILogger<FindEgressCase> _logger = logger;
  private readonly IEgressClient _egressClient = egressClient;
  private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;
  private readonly IInitializationHandler _initializationHandler = initializationHandler;
  private readonly IUnhandledExceptionHandler _unhandledExceptionHandler = unhandledExceptionHandler;

  [Function(nameof(FindEgressCase))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/egress")] HttpRequest req)
  {
    try
    {
      var validateTokenResult = await _initializationHandler.Initialize(req);

      if (!validateTokenResult.IsValid || string.IsNullOrEmpty(validateTokenResult.Username))
      {
        return new UnauthorizedResult();
      }

      var operationName = req.Query["operationName"];

      var egressArg = _egressArgFactory.CreateFindWorkspaceArg(operationName);

      var response = await _egressClient.FindWorkspace(egressArg, "integration@cps.gov.uk");

      return new OkObjectResult(response);
    }
    catch (Exception ex)
    {
      return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(_logger, nameof(FindEgressCase), ex);
    }
  }
}