using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Data.Repositories;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class CreateEgressConnection(ILogger<CreateEgressConnection> logger)
{
  private readonly ILogger<CreateEgressConnection> _logger = logger;

  [Function(nameof(CreateEgressConnection))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "egress/connections")] HttpRequest req, FunctionContext functionContext)
  {
    var context = functionContext.GetRequestContext();

    // todo: check the authed user has permission to the workspace in egress


    return new OkResult();
  }
}