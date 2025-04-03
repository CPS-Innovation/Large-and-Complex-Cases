using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Validators;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.Data.Services;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class CreateEgressConnection(ICaseMetadataService caseMetadataService, IEgressClient egressClient, IEgressArgFactory egressArgFactory, ILogger<CreateEgressConnection> logger)
{
  private readonly ILogger<CreateEgressConnection> _logger = logger;
  private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
  private readonly IEgressClient _egressClient = egressClient;
  private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;

  [Function(nameof(CreateEgressConnection))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "egress/connections")] HttpRequest req, FunctionContext functionContext)
  {
    var context = functionContext.GetRequestContext();

    var egressConnectionRequest = await ValidatorHelper.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(req);

    if (!egressConnectionRequest.IsValid)
    {
      return new BadRequestObjectResult(egressConnectionRequest.ValidationErrors);
    }

    var egressArg = _egressArgFactory.CreateGetWorkspacePermissionArg(egressConnectionRequest.Value.EgressWorkspaceId, context.Username);
    var hasEgressPermission = await _egressClient.GetWorkspacePermission(egressArg);

    if (!hasEgressPermission)
    {
      return new UnauthorizedResult();
    }

    await _caseMetadataService.CreateEgressConnectionAsync(egressConnectionRequest.Value);

    return new OkResult();
  }
}