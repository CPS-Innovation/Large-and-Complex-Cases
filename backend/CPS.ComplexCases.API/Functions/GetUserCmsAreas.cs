using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.API.Functions;

public class GetUserCmsArea(ILogger<GetUserCmsArea> logger,
  IDdeiClient ddeiClient,
  IDdeiArgFactory ddeiArgFactory) : BaseFunction()
{
  private readonly ILogger<GetUserCmsArea> _logger = logger;
  private readonly IDdeiClient _ddeiClient = ddeiClient;
  private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

  [Function(nameof(GetUserCmsArea))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user-areas")] HttpRequest req)
  {
    var correlationId = EstablishCorrelation(req);
    var cmsAuthValues = EstablishCmsAuthValues(req);

    var result = await _ddeiClient.GetUserCmsAreasAsync(_ddeiArgFactory.CreateBaseArg(cmsAuthValues, correlationId));

    return new OkObjectResult(result);
  }
}