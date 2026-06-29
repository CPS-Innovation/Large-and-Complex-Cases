using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class CreateDestinationFolder(ILogger<CreateDestinationFolder> logger, IStorageClientFactory storageClientFactory, IInitializationHandler initializationHandler, ITelemetryClient telemetryClient)
{
    private readonly ILogger<CreateDestinationFolder> _logger = logger;
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;

    [Function(nameof(CreateDestinationFolder))]
    public async Task Run([ActivityTrigger] CreateDestinationFolderPayload? payload, CancellationToken cancellationToken = default)
    {
        _initializationHandler.Initialize(payload?.UserName!, payload?.CorrelationId, payload?.CaseId);

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "CreateDestinationFolderPayload cannot be null.");
        }

        if (string.IsNullOrEmpty(payload.BucketName))
        {
            _logger.LogError("BucketName is required in CreateDestinationFolder activity.");
            throw new ArgumentException("BucketName is required in CreateDestinationFolder activity.", nameof(payload));
        }

        var storageClient = _storageClientFactory.GetSourceClientForDirection(payload.TransferDirection);

        try
        {
            var created = await storageClient.CreateFolderAsync(payload.DestinationFolderPath, null, payload.BearerToken, payload.BucketName);

            if (!created)
            {
                throw new InvalidOperationException(
                    $"Failed to create destination folder '{payload.DestinationFolderPath}' in bucket '{payload.BucketName}'.");
            }

            _logger.LogInformation("Successfully created destination folder: {BucketName}", payload.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create destination folder: {BucketName}", payload.BucketName);
            throw;
        }
    }
}