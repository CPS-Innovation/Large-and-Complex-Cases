using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.API.Handlers;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class FindNetAppFolder(ILogger<FindNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IInitializationHandler initializationHandler,
    IUnhandledExceptionHandler unhandledExceptionHandler)
{
    private readonly ILogger<FindNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly IUnhandledExceptionHandler _unhandledExceptionHandler = unhandledExceptionHandler;

    [Function(nameof(FindNetAppFolder))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/{operationName}/netapp")] HttpRequest req, string operationName)
    {
        try
        {
            var validateTokenResult = await _initializationHandler.Initialize(req);

            if (!validateTokenResult.IsValid || string.IsNullOrEmpty(validateTokenResult.Username))
            {
                return new UnauthorizedResult();
            }

            var arg = _netAppArgFactory.CreateFindBucketArg(operationName!);
            var result = await _netAppClient.FindBucketAsync(arg);

            if (result == null)
            {
                return new NotFoundObjectResult($"Bucket {operationName} not found");
            }

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(_logger, nameof(FindNetAppFolder), ex);
        }
    }
}