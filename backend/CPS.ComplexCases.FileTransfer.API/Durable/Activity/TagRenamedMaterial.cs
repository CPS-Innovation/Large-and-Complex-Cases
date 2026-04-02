using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class TagRenamedMaterial(INetAppClient netAppClient, INetAppArgFactory netAppArgFactory, IInitializationHandler initializationHandler, ILogger<TagRenamedMaterial> logger)
{
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ILogger<TagRenamedMaterial> _logger = logger;

    private const string RenamedFromTagKey = "lcc-renamed-from";

    [Function(nameof(TagRenamedMaterial))]
    public async Task Run([ActivityTrigger] TagRenamedMaterialPayload payload)
    {
        _initializationHandler.Initialize(payload.UserName!, payload.CorrelationId);

        _logger.LogInformation(
            "Tagging renamed object. DestinationKey={DestinationKey}, OriginalKey={OriginalKey}, BucketName={BucketName}",
            payload.DestinationKey, payload.OriginalKey, payload.BucketName);

        var arg = _netAppArgFactory.CreatePutObjectTaggingArg(
            payload.BearerToken,
            payload.BucketName,
            payload.DestinationKey,
            new Dictionary<string, string>
            {
                [RenamedFromTagKey] = payload.OriginalKey
            });

        var success = await _netAppClient.PutObjectTaggingAsync(arg);

        if (success)
        {
            _logger.LogInformation(
                "Successfully applied '{TagKey}' tag to {DestinationKey} in bucket {BucketName}.",
                RenamedFromTagKey, payload.DestinationKey, payload.BucketName);
        }
        else
        {
            _logger.LogWarning(
                "Tagging returned a non-success response for {DestinationKey} in bucket {BucketName}.",
                payload.DestinationKey, payload.BucketName);
        }
    }
}
