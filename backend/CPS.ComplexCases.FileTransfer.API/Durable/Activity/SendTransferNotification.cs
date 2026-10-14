using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Interfaces;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

/// <summary>
/// Emails the user who initiated a transfer via GOV.UK Notify once it reaches a terminal state.
/// Dispatch failures are logged and swallowed: a notification must never fail or retry a transfer.
/// </summary>
public class SendTransferNotification(
    IEnumerable<IAsyncNotificationClient> notificationClients,
    IOptions<GovNotifyOptions> options,
    ITelemetryClient telemetryClient,
    IInitializationHandler initializationHandler,
    ILogger<SendTransferNotification> logger)
{
    // Not registered when GovNotifyOptions:ApiKey is empty.
    private readonly IAsyncNotificationClient? _notificationClient = notificationClients.FirstOrDefault();
    private readonly GovNotifyOptions _options = options.Value;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ILogger<SendTransferNotification> _logger = logger;

    // Overridable in unit tests so the finalisation wait does not use real delays.
    internal TimeSpan FinalStatusRetryDelay { get; init; } = DurableEntityRetry.DefaultVisibilityRetryDelay;
    internal int FinalStatusMaxAttempts { get; init; } = DurableEntityRetry.DefaultVisibilityMaxAttempts;
    internal Func<TimeSpan, CancellationToken, Task> DelayAsync { get; init; } = Task.Delay;

    [Function(nameof(SendTransferNotification))]
    public async Task Run([ActivityTrigger] SendTransferNotificationPayload payload, [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        _initializationHandler.Initialize(payload.UserName!, payload.CorrelationId);

        var notificationEvent = new TransferNotificationEvent
        {
            CaseId = payload.CaseId,
            CorrelationId = payload.CorrelationId ?? Guid.Empty,
            TransferId = payload.TransferId,
            Recipient = payload.UserName,
            TemplateId = payload.IsFailure ? _options.TransferFailedTemplateId : _options.TransferCompletedTemplateId,
            DispatchStatus = TransferNotificationEvent.StatusSkipped,
            EventTimestamp = DateTime.UtcNow
        };

        try
        {
            if (_notificationClient is null)
            {
                notificationEvent.FailureReason = "GOV.UK Notify is not configured.";
                _logger.LogWarning(
                    "GOV.UK Notify is not configured; skipping transfer notification for TransferId {TransferId}, CaseId {CaseId}.",
                    payload.TransferId, payload.CaseId);
                return;
            }

            if (string.IsNullOrWhiteSpace(payload.UserName))
            {
                notificationEvent.FailureReason = "No recipient email address.";
                _logger.LogWarning(
                    "No recipient for transfer notification; skipping for TransferId {TransferId}, CaseId {CaseId}.",
                    payload.TransferId, payload.CaseId);
                return;
            }

            var personalisation = payload.IsFailure
                ? BuildFailedPersonalisation(payload, _options.UiBaseUrl)
                : BuildCompletedPersonalisation(payload, await GetFinalisedEntityAsync(client, payload.TransferId, cancellationToken), _options.UiBaseUrl);

            var response = await _notificationClient.SendEmailAsync(
                payload.UserName,
                notificationEvent.TemplateId,
                personalisation,
                clientReference: payload.TransferId.ToString());

            notificationEvent.NotificationId = response?.id;
            notificationEvent.DispatchStatus = TransferNotificationEvent.StatusSent;

            _logger.LogInformation(
                "Transfer notification sent. NotificationId {NotificationId}, Recipient {Recipient}, TemplateId {TemplateId}, TransferId {TransferId}, CaseId {CaseId}.",
                notificationEvent.NotificationId, payload.UserName, notificationEvent.TemplateId, payload.TransferId, payload.CaseId);
        }
        catch (Exception ex)
        {
            notificationEvent.DispatchStatus = TransferNotificationEvent.StatusFailed;
            notificationEvent.FailureReason = ex.Message;

            _logger.LogWarning(
                ex,
                "Transfer notification failed. Recipient {Recipient}, TemplateId {TemplateId}, TransferId {TransferId}, CaseId {CaseId}.",
                payload.UserName, notificationEvent.TemplateId, payload.TransferId, payload.CaseId);
        }
        finally
        {
            _telemetryClient.TrackEvent(notificationEvent);
        }
    }

    // FinalizeTransfer only signals the entity, so its final status may not be visible yet.
    // After the wait, the latest state is used even if it is not final, so a slow entity still gets an email.
    private async Task<TransferEntity> GetFinalisedEntityAsync(DurableTaskClient client, Guid transferId, CancellationToken cancellationToken)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), transferId.ToString());
        TransferEntity? latest = null;

        for (var attempt = 1; attempt <= FinalStatusMaxAttempts; attempt++)
        {
            var entity = await DurableEntityRetry.ExecuteAsync(
                nameof(SendTransferNotification),
                () => client.Entities.GetEntityAsync<TransferEntity>(entityId, cancellationToken),
                _logger,
                cancellationToken);

            latest = entity?.State ?? latest;
            if (latest is not null && IsFinalised(latest.Status))
            {
                return latest;
            }

            if (attempt < FinalStatusMaxAttempts)
            {
                await DelayAsync(FinalStatusRetryDelay, cancellationToken);
            }
        }

        return latest ?? throw new InvalidOperationException($"Transfer entity with ID {transferId} not found.");
    }

    private static bool IsFinalised(TransferStatus status) =>
        status is TransferStatus.Completed or TransferStatus.PartiallyCompleted;

    internal static Dictionary<string, dynamic> BuildCompletedPersonalisation(
        SendTransferNotificationPayload payload,
        TransferEntity entity,
        string uiBaseUrl) => new()
        {
            ["urn"] = GetDisplayUrn(payload),
            ["direction"] = ToDisplayDirection(payload.TransferDirection),
            ["operation_type"] = ToDisplayOperationType(payload.TransferType),
            ["successful_files"] = entity.SuccessfulFiles.ToString(),
            ["total_files"] = entity.TotalFiles.ToString(),
            ["case_materials_url"] = BuildCaseUrl(uiBaseUrl, payload.CaseId),
            ["has_errors"] = entity.FailedItems.Count > 0 || entity.DeletionErrors.Count > 0 ? "yes" : "no"
        };

    internal static Dictionary<string, dynamic> BuildFailedPersonalisation(
        SendTransferNotificationPayload payload,
        string uiBaseUrl) => new()
        {
            ["urn"] = GetDisplayUrn(payload),
            ["direction"] = ToDisplayDirection(payload.TransferDirection),
            ["operation_type"] = ToDisplayOperationType(payload.TransferType),
            ["case_activity_log_url"] = BuildActivityLogUrl(uiBaseUrl, payload.CaseId)
        };

    internal static string GetDisplayUrn(SendTransferNotificationPayload payload) =>
        string.IsNullOrWhiteSpace(payload.Urn) ? payload.CaseId.ToString() : payload.Urn;

    internal static string ToDisplayDirection(TransferDirection direction) => direction switch
    {
        TransferDirection.NetAppToEgress => "Shared Drive to Egress",
        TransferDirection.EgressToNetApp => "Egress to Shared Drive",
        _ => direction.ToString()
    };

    internal static string ToDisplayOperationType(TransferType transferType) => transferType switch
    {
        TransferType.Copy => "Copy",
        TransferType.Move => "Move",
        _ => transferType.ToString()
    };

    internal static string BuildCaseUrl(string uiBaseUrl, int caseId) =>
        $"{uiBaseUrl.TrimEnd('/')}/case/{caseId}/case-management";

    internal static string BuildActivityLogUrl(string uiBaseUrl, int caseId) =>
        $"{BuildCaseUrl(uiBaseUrl, caseId)}?tab=activity-log";
}
