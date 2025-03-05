using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class FindNetAppFolder(ILogger<FindNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory)
{
    private readonly ILogger<FindNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    [Function(nameof(FindNetAppFolder))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/{operationName}/netapp")] HttpRequest req, string operationName)
    {
        var arg = _netAppArgFactory.CreateFindBucketArg(operationName!);
        var result = await _netAppClient.FindBucketAsync(arg);

        if (result == null)
        {
            return new NotFoundObjectResult($"Bucket {operationName} not found");
        }

        return new OkObjectResult(result);
    }
}