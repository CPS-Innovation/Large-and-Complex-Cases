using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.Extensions.Logging;
using Amazon.Runtime;

namespace CPS.ComplexCases.API.Functions;

public class CreateNetAppFolder(ILogger<CreateNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory)
{
    private readonly ILogger<CreateNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    [Function(nameof(CreateNetAppFolder))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "netapp/{operationName}")] HttpRequest req, string operationName)
    {
        var arg = _netAppArgFactory.CreateCreateBucketArg(operationName!);
        var result = await _netAppClient.CreateBucketAsync(arg);

        if (!result)
        {
            return new BadRequestObjectResult($"Bucket {operationName} could not be created.");
        }

        return new OkObjectResult(result);
    }
}