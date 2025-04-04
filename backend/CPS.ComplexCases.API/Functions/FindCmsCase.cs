using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.API.Functions;

public class FindCmsCase(ILogger<FindCmsCase> logger,
  IDdeiClient ddeiClient,
  IDdeiArgFactory ddeiArgFactory)
{
  private readonly ILogger<FindCmsCase> _logger = logger;
  private readonly IDdeiClient _ddeiClient = ddeiClient;
  private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

  [Function(nameof(FindCmsCase))]
  [OpenApiOperation(operationId: nameof(FindCmsCase), tags: ["CMS"], Description = "Finds a case in CMS based on operation name, URN, defendant name, and area.")]
  [OpenApiParameter(name: InputParameters.OperationName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The operation name to search for.")]
  [OpenApiParameter(name: InputParameters.Urn, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The URN to search for.")]
  [OpenApiParameter(name: InputParameters.DefendantName, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The defendant name to search for.")]
  [OpenApiParameter(name: InputParameters.Area, In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The area code to search for.")]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "case-search")] HttpRequest req, FunctionContext functionContext)
  {
    var context = functionContext.GetRequestContext();

    var operationName = req.Query[InputParameters.OperationName].FirstOrDefault();
    var urn = req.Query[InputParameters.Urn].FirstOrDefault();
    var defendantName = req.Query[InputParameters.DefendantName].FirstOrDefault();
    var cmsAreaCode = req.Query[InputParameters.Area].FirstOrDefault();

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