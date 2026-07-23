using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.ActivityLog.Models;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class UpdateActivityLog(IActivityLogService activityLogService, ILogger<UpdateActivityLog> logger, IInitializationHandler initializationHandler, ICaseMetadataService caseMetadataService)
{
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly ILogger<UpdateActivityLog> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;

    [Function(nameof(UpdateActivityLog))]
    public async Task Run([ActivityTrigger] UpdateActivityLogPayload payload, [DurableClient] DurableTaskClient client)
    {
        _initializationHandler.Initialize(payload?.UserName!, payload?.CorrelationId);

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "UpdateActivityLog payload cannot be null.");
        }

        var entityId = new EntityInstanceId(nameof(TransferEntityState), payload.TransferId.ToString());
        var entity = await DurableEntityRetry.ExecuteAsync(
            nameof(UpdateActivityLog),
            () => client.Entities.GetEntityAsync<TransferEntity>(entityId),
            _logger);

        if (entity == null)
        {
            throw new InvalidOperationException($"Transfer entity with ID {payload.TransferId} not found.");
        }

        // For NetApp to Egress the case's NetApp root folder is stored in case metadata. The file
        // paths in state are full NetApp paths that include it, and the source root in state has it
        // stripped. Fetch the root so the source can be shown as the full path (below), matching how
        // the NetApp destination is shown for the reverse transfer. The full source is a display
        // nicety, so a lookup failure falls back to the case-root-relative source rather than
        // stopping the activity log being written.
        string? netappRootFolderPath = null;
        if (entity.State.Direction == TransferDirection.NetAppToEgress)
        {
            try
            {
                var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(entity.State.CaseId);
                netappRootFolderPath = caseMetadata?.NetappFolderPath;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read the NetApp folder path for case {CaseId}. The activity log source will be shown relative to the case root.", entity.State.CaseId);
            }
        }

        var successfulItems = entity.State.SuccessfulItems.Select(x => new FileTransferItem
        {
            Path = x.SourcePath,
            Size = x.Size,
            IsRenamed = x.IsRenamed,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
        }).ToList();

        var errorItems = entity.State.FailedItems.Select(x => new FileTransferError
        {
            Path = x.SourcePath,
            ErrorCode = x.ErrorCode.ToString(),
            ErrorMessage = x.ErrorMessage
        }).ToList();

        var skippedItems = entity.State.SkippedItems.Select(x => new FileTransferError
        {
            Path = x.SourcePath,
            ErrorCode = "EmptyFileSkipped",
            ErrorMessage = "Empty (0-byte) files cannot be uploaded to Egress and were skipped."
        }).ToList();

        var sourcePath = entity.State.SourceRootFolderPath
            ?? Path.GetDirectoryName(entity.State.SourcePaths[0].FullFilePath)
            ?? entity.State.SourcePaths[0].Path;
        sourcePath = sourcePath?.Replace('\\', '/') ?? throw new InvalidOperationException("Source path cannot be null or empty.");

        // Show the NetApp source as the full path including the case root folder, so it lines up with
        // the full file paths above (the UI lists files relative to it) and matches how the NetApp
        // destination is shown for the reverse direction. The source root in state is relative to the
        // case root, so add the root back for display.
        if (!string.IsNullOrEmpty(netappRootFolderPath))
        {
            sourcePath = PrependNetappRootFolder(netappRootFolderPath, sourcePath);
        }

        var deletionErrors = new List<FileTransferError>();

        if (entity.State.TransferType == TransferType.Move && entity.State.Direction == TransferDirection.EgressToNetApp && payload.ActionType != ActionType.TransferInitiated)
        {
            deletionErrors = entity.State.DeletionErrors.Select(x => new FileTransferError
            {
                Path = x.FileId,
                ErrorMessage = x.ErrorMessage
            }).ToList();
            errorItems.AddRange(deletionErrors);
        }

        var fileTransferDetails = new FileTransferDetails
        {
            TransferId = payload.TransferId,
            TransferDirection = entity.State.Direction.ToString(),
            TransferType = entity.State.TransferType.ToString(),
            TotalFiles = entity.State.TotalFiles,
            SourcePath = sourcePath!,
            DestinationPath = entity.State.DestinationPath,
            Files = successfulItems,
            Errors = errorItems,
            Skipped = skippedItems,
            DeletionErrors = deletionErrors,
            ExceptionMessage = payload.ExceptionMessage,
            StartTime = entity.State.StartedAt,
            EndTime = entity.State.CompletedAt
        };

        await _activityLogService.CreateActivityLogAsync(
            actionType: payload.ActionType,
            resourceType: ResourceType.FileTransfer,
            resourceId: entity.State.Id.ToString(),
            resourceName: entity.State.Direction.ToString(),
            caseId: entity.State.CaseId,
            userName: payload.UserName,
            details: fileTransferDetails.SerializeToJsonDocument(_logger)
        );
    }

    private static string PrependNetappRootFolder(string netappRootFolderPath, string sourcePath)
    {
        var root = netappRootFolderPath.Replace('\\', '/').TrimEnd('/');
        var relative = sourcePath.TrimStart('/');

        if (string.IsNullOrEmpty(relative))
        {
            return root;
        }

        // The source root was not stripped (for example, case metadata was missing when the files
        // were listed), so it already includes the root. Do not add it a second time.
        if (relative.Equals(root, StringComparison.OrdinalIgnoreCase)
            || relative.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
        {
            return relative;
        }

        return $"{root}/{relative}";
    }
}