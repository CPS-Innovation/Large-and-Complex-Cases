using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.Common.Enums;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Handlers;

public class DisconnectConnectionHandler(ILogger<DisconnectConnectionHandler> logger, IInitializationHandler initializationHandler, ICaseMetadataService caseMetadataService, IActivityLogService activityLogService) : IDisconnectConnectionHandler
{
    private readonly ILogger<DisconnectConnectionHandler> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IActivityLogService _activityLogService = activityLogService;

    public async Task<IActionResult> RunAsync(HttpRequest req, FunctionContext functionContext, StorageConnectionType connectionType)
    {
        var context = functionContext.GetRequestContext();

        if (!req.TryGetCaseId(out var caseId, out var caseIdError))
        {
            return caseIdError!;
        }

        _initializationHandler.Initialize(context.Username, context.CorrelationId, caseId);

        var config = GetConfig(connectionType);
        var result = await config.ClearConnection(caseId);

        if (result.State == CaseMetadataState.NoCaseMetadataFound)
        {
            return new NotFoundObjectResult(config.NotFoundMessage(caseId));
        }

        if (result.State == CaseMetadataState.TransferIsActive)
        {
            return new ConflictObjectResult(config.ActiveTransferMessage(caseId));
        }

        if (result.State == config.MissingConnectionState)
        {
            return new BadRequestObjectResult(config.MissingConnectionMessage(caseId));
        }

        try
        {
            await _activityLogService.CreateActivityLogAsync(
                config.ActivityLogAction,
                ResourceType.StorageConnection,
                caseId,
                result.ClearedPath!,
                result.ClearedPath,
                context.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, config.ActivityLogFailureMessage, caseId);
        }

        return new OkResult();
    }

    private DisconnectConnectionConfig GetConfig(StorageConnectionType connectionType)
    {
        return connectionType switch
        {
            StorageConnectionType.NetApp => new DisconnectConnectionConfig(
                ClearConnection: _caseMetadataService.ClearNetAppFolderPathAsync,
                MissingConnectionState: CaseMetadataState.NetAppFolderPathIsNull,
                ActivityLogAction: ActionType.DisconnectionFromNetApp,
                NotFoundMessage: caseId => $"No NetApp connection found for case ID {caseId}.",
                ActiveTransferMessage: caseId => $"Cannot disconnect NetApp connection for case ID {caseId} because there is an active transfer.",
                MissingConnectionMessage: caseId => $"Case ID {caseId} does not have an active NetApp connection.",
                ActivityLogFailureMessage: "Failed to write activity log for NetApp connection disconnection for case {CaseId}."
            ),

            StorageConnectionType.Egress => new DisconnectConnectionConfig(
                ClearConnection: _caseMetadataService.ClearEgressConnectionAsync,
                MissingConnectionState: CaseMetadataState.EgressConnectionIsNull,
                ActivityLogAction: ActionType.DisconnectionFromEgress,
                NotFoundMessage: caseId => $"No Egress workspace connection found for case ID {caseId}.",
                ActiveTransferMessage: caseId => $"Cannot disconnect Egress workspace connection for case ID {caseId} because there is an active transfer.",
                MissingConnectionMessage: caseId => $"Case ID {caseId} does not have an active Egress workspace connection.",
                ActivityLogFailureMessage: "Failed to write activity log for Egress workspace connection disconnection for case {CaseId}."
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
        };
    }

}