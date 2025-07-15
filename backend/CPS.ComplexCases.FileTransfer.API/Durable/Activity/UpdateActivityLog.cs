using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Models;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class UpdateActivityLog(IActivityLogService activityLogService)
{
    private readonly IActivityLogService _activityLogService = activityLogService;

    [Function(nameof(UpdateActivityLog))]
    public async Task Run([ActivityTrigger] UpdateActivityLogPayload payload, [DurableClient] DurableTaskClient client)
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "UpdateActivityLog payload cannot be null.");
        }

        var entityId = new EntityInstanceId(nameof(TransferEntityState), payload.TransferId.ToString());
        var entity = await client.Entities.GetEntityAsync<TransferEntity>(entityId);

        if (entity == null)
        {
            throw new InvalidOperationException($"Transfer entity with ID {payload.TransferId} not found.");
        }

        var successfulItems = entity.State.SuccessfulItems.Select(x => new FileTransferItem
        {
            Path = x.SourcePath,
            Size = x.Size,
            IsRenamed = x.IsRenamed
        }).ToList();

        var errorItems = entity.State.FailedItems.Select(x => new FileTransferError
        {
            Path = x.SourcePath,
            ErrorCode = x.ErrorCode.ToString(),
            ErrorMessage = x.ErrorMessage
        }).ToList();

        var sourcePath = Path.GetDirectoryName(entity.State.SourcePaths.First().RelativePath);
        var deletionErrors = new List<FileTransferError>();

        if (entity.State.TransferType == TransferType.Move && payload.ActionType != ActionType.TransferInitiated)
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
            DeletionErrors = deletionErrors,
            ExceptionMessage = payload.ExceptionMessage
        };

        await _activityLogService.CreateActivityLogAsync(
            actionType: payload.ActionType,
            resourceType: ResourceType.FileTransfer,
            resourceId: entity.State.Id.ToString(),
            resourceName: entity.State.Direction.ToString(),
            caseId: entity.State.CaseId,
            userName: payload.UserName,
            details: _activityLogService.ConvertToJsonDocument(fileTransferDetails)
        );
    }
}