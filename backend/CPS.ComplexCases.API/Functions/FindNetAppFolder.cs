using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class FindNetAppFolder
{
    private readonly INetAppClient _netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory;

    public FindNetAppFolder(INetAppClient netAppClient, INetAppArgFactory netAppArgFactory)
    {
        _netAppClient = netAppClient;
        _netAppArgFactory = netAppArgFactory;
    }

    [Function(nameof(FindNetAppFolder))]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "netapp/find-folder")] HttpRequest req)
    {
        var bucketName = req.Query["bucketName"];

        if (string.IsNullOrEmpty(bucketName))
        {
            return new BadRequestObjectResult("Please provide bucketName.");
        }

        var arg = _netAppArgFactory.CreateFindBucketArg(bucketName!);
        var result = _netAppClient.FindBucketAsync(arg);

        if (result == null)
        {
            return new NotFoundObjectResult($"Bucket {bucketName} not found");
        }

        return new OkObjectResult(result);
    }
}