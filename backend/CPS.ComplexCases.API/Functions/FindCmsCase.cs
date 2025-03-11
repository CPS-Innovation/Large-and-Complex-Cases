using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.API.Functions;

public class FindCmsCase(ILogger<FindCmsCase> logger,
  IDdeiClient ddeiClient,
  IDdeiArgFactory ddeiArgFactory) : BaseFunction()
{
  private readonly ILogger<FindCmsCase> _logger = logger;
  private readonly IDdeiClient _ddeiClient = ddeiClient;
  private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

  [Function(nameof(FindCmsCase))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "case-search")] HttpRequest req)
  {
    var correlationId = EstablishCorrelation(req);
    var cmsAuthValues = EstablishCmsAuthValues(req);

    var operationName = req.Query["operation-name"].FirstOrDefault();
    var urn = req.Query["urn"].FirstOrDefault();
    var defendantName = req.Query["defendant-name"].FirstOrDefault();
    var cmsAreaCode = req.Query["area"].FirstOrDefault();

    IEnumerable<CaseDto> result;
    if (!string.IsNullOrEmpty(urn))
    {
      var arg = _ddeiArgFactory.CreateUrnArg(cmsAuthValues, correlationId, urn);
      result = await _ddeiClient.ListCasesByUrnAsync(arg);
    }
    else if (!string.IsNullOrEmpty(operationName) && !string.IsNullOrEmpty(cmsAreaCode))
    {
      var arg = _ddeiArgFactory.CreateOperationNameArg(cmsAuthValues, correlationId, operationName, cmsAreaCode);
      result = await _ddeiClient.ListCasesByOperationNameAsync(arg);
    }
    else if (!string.IsNullOrEmpty(defendantName) && !string.IsNullOrEmpty(cmsAreaCode))
    {
      var arg = _ddeiArgFactory.CreateDefendantArg(cmsAuthValues, correlationId, defendantName, cmsAreaCode);
      result = await _ddeiClient.ListCasesByDefendantNameAsync(arg);
    }
    else
    {
      return new BadRequestObjectResult("Search Parameters Invalid");
    }

    return new OkObjectResult(result);
  }
}