using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Dto;

namespace CPS.ComplexCases.API.Functions;

public class ListEgressTemplates(ILogger<ListEgressTemplates> logger,
    IEgressClient egressClient,
    IEgressArgFactory egressArgFactory,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<ListEgressTemplates> _logger = logger;
    private readonly IEgressClient _egressClient = egressClient;
    private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(ListEgressTemplates))]
    [OpenApiOperation(operationId: nameof(ListEgressTemplates), tags: ["Egress"], Description = "Paginated list of workspace templates from Egress")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.Skip, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to skip.")]
    [OpenApiParameter(name: InputParameters.Take, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to take.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(ListTemplatesDto), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/egress/templates")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var skip = int.TryParse(req.Query[InputParameters.Skip], out var skipValue) ? skipValue : 0;
        var take = int.TryParse(req.Query[InputParameters.Take], out var takeValue) ? takeValue : 100;

        var listTemplatesArg = _egressArgFactory.CreatePaginationArg(skip, take);

        var response = await _egressClient.ListTemplatesAsync(listTemplatesArg);

        return new OkObjectResult(response);
    }
}