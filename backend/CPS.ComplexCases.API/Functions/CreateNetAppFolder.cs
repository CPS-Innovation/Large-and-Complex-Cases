using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Functions;

public class CreateNetAppFolder(ILogger<CreateNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory)
{
    private readonly ILogger<CreateNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    [Function(nameof(CreateNetAppFolder))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/cases/{operationName}/netapp")] HttpRequest req, string operationName)
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