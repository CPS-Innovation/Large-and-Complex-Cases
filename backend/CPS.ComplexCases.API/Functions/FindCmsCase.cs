using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class FindCmsCase(ILogger<FindCmsCase> logger,
  IDdeiClient ddeiClient,
  IDdeiArgFactory ddeiArgFactory)
{
  private readonly ILogger<FindCmsCase> _logger = logger;
  private readonly IDdeiClient _ddeiClient = ddeiClient;
  private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

  [Function(nameof(FindCmsCase))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "case-search")] HttpRequest req, FunctionContext functionContext)
  {
    var context = functionContext.GetRequestContext();

    var operationName = req.Query["operation-name"].FirstOrDefault();
    var urn = req.Query["urn"].FirstOrDefault();
    var defendantName = req.Query["defendant-name"].FirstOrDefault();
    var cmsAreaCode = req.Query["area"].FirstOrDefault();

    IEnumerable<CaseDto> result;
    if (!string.IsNullOrEmpty(urn))
    {
      var arg = _ddeiArgFactory.CreateUrnArg(context.CmsAuthValues, context.CorrelationId, urn);
      result = await _ddeiClient.ListCasesByUrnAsync(arg);
    }
    else if (!string.IsNullOrEmpty(operationName) && !string.IsNullOrEmpty(cmsAreaCode))
    {
      var arg = _ddeiArgFactory.CreateOperationNameArg(context.CmsAuthValues, context.CorrelationId, operationName, cmsAreaCode);
      result = await _ddeiClient.ListCasesByOperationNameAsync(arg);
    }
    else if (!string.IsNullOrEmpty(defendantName) && !string.IsNullOrEmpty(cmsAreaCode))
    {
      var arg = _ddeiArgFactory.CreateDefendantArg(context.CmsAuthValues, context.CorrelationId, defendantName, cmsAreaCode);
      result = await _ddeiClient.ListCasesByDefendantNameAsync(arg);
    }
    else
    {
      return new BadRequestObjectResult("Search Parameters Invalid");
    }

    return new OkObjectResult(result);
  }
}