using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class GetUserCmsArea(ILogger<GetUserCmsArea> logger,
  IDdeiClient ddeiClient,
  IDdeiArgFactory ddeiArgFactory)
{
  private readonly ILogger<GetUserCmsArea> _logger = logger;
  private readonly IDdeiClient _ddeiClient = ddeiClient;
  private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

  [Function(nameof(GetUserCmsArea))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user-areas")] HttpRequest req, FunctionContext functionContext)
  {
    var context = functionContext.GetRequestContext();

    var result = await _ddeiClient.GetUserCmsAreasAsync(_ddeiArgFactory.CreateBaseArg(context.CmsAuthValues, context.CorrelationId));

    return new OkObjectResult(result);
  }
}