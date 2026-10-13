using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class SendTransferNotificationTests
{
    private const string UiBaseUrl = "https://lcc.example/";
    private const string Recipient = "user@cps.gov.uk";
    private const string Urn = "45AA2098221";

    private readonly Mock<IAsyncNotificationClient> _notificationClientMock = new();
    private readonly Mock<ITelemetryClient> _telemetryClientMock = new();
    private readonly Mock<IInitializationHandler> _initializationHandlerMock = new();
    private readonly Mock<ILogger<SendTransferNotification>> _loggerMock = new();
    private readonly DurableEntityClientStub _durableEntityClientStub = new("TestClient");
    private readonly DurableTaskClientStub _durableTaskClientStub;
    private readonly GovNotifyOptions _options = new() { ApiKey = "test-key", UiBaseUrl = UiBaseUrl };

    private string? _sentEmail;
    private string? _sentTemplateId;
    private Dictionary<string, dynamic>? _sentPersonalisation;
    private string? _sentClientReference;
    private TransferNotificationEvent? _trackedEvent;

    public SendTransferNotificationTests()
    {
        _durableTaskClientStub = new DurableTaskClientStub(_durableEntityClientStub);

        _notificationClientMock
            .Setup(c => c.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Callback<string, string, Dictionary<string, dynamic>, string, string, string, List<string>>((email, templateId, personalisation, clientReference, _, _, _) =>
            {
                _sentEmail = email;
                _sentTemplateId = templateId;
                _sentPersonalisation = personalisation;
                _sentClientReference = clientReference;
            })
            .ReturnsAsync(new EmailNotificationResponse { id = "notification-123" });

        _telemetryClientMock
            .Setup(t => t.TrackEvent(It.IsAny<BaseTelemetryEvent>()))
            .Callback<BaseTelemetryEvent>(e => _trackedEvent = e as TransferNotificationEvent);
    }

    [Fact]
    public async Task Run_WhenTransferCompleted_SendsCompletedTemplateWithPersonalisation()
    {
        var payload = CreatePayload(isFailure: false, TransferDirection.NetAppToEgress, TransferType.Copy);
        SetupEntity(CreateEntity(payload, TransferStatus.Completed, successfulFiles: 3, totalFiles: 3));

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.Equal(Recipient, _sentEmail);
        Assert.Equal(_options.TransferCompletedTemplateId, _sentTemplateId);
        Assert.Equal(payload.TransferId.ToString(), _sentClientReference);
        Assert.NotNull(_sentPersonalisation);
        Assert.Equal(
            ["urn", "direction", "operation_type", "successful_files", "total_files", "case_materials_url", "has_errors"],
            _sentPersonalisation!.Keys);
        Assert.Equal(Urn, (string)_sentPersonalisation["urn"]);
        Assert.Equal("Shared Drive to Egress", (string)_sentPersonalisation["direction"]);
        Assert.Equal("Copy", (string)_sentPersonalisation["operation_type"]);
        Assert.Equal("3", (string)_sentPersonalisation["successful_files"]);
        Assert.Equal("3", (string)_sentPersonalisation["total_files"]);
        Assert.Equal($"https://lcc.example/case/{payload.CaseId}/case-management", (string)_sentPersonalisation["case_materials_url"]);
        Assert.Equal("no", (string)_sentPersonalisation["has_errors"]);

        Assert.NotNull(_trackedEvent);
        Assert.Equal(TransferNotificationEvent.StatusSent, _trackedEvent!.DispatchStatus);
        Assert.Equal("notification-123", _trackedEvent.NotificationId);
        Assert.Equal(Recipient, _trackedEvent.Recipient);
        Assert.Equal(payload.TransferId, _trackedEvent.TransferId);
        Assert.Equal(payload.CaseId, _trackedEvent.CaseId);
    }

    [Fact]
    public async Task Run_WhenTransferPartiallyCompleted_SetsHasErrorsToYes()
    {
        var payload = CreatePayload(isFailure: false, TransferDirection.EgressToNetApp, TransferType.Move);
        var entity = CreateEntity(payload, TransferStatus.PartiallyCompleted, successfulFiles: 2, totalFiles: 5);
        entity.FailedItems.Add(new TransferFailedItem { SourcePath = "/secret/suspect-name.pdf", ErrorCode = TransferErrorCode.Transient, ErrorMessage = "boom" });
        SetupEntity(entity);

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.Equal(_options.TransferCompletedTemplateId, _sentTemplateId);
        Assert.Equal("yes", (string)_sentPersonalisation!["has_errors"]);
        Assert.Equal("2", (string)_sentPersonalisation["successful_files"]);
        Assert.Equal("5", (string)_sentPersonalisation["total_files"]);
        Assert.Equal("Egress to Shared Drive", (string)_sentPersonalisation["direction"]);
        Assert.Equal("Move", (string)_sentPersonalisation["operation_type"]);
        Assert.DoesNotContain(_sentPersonalisation.Values, v => ((string)v).Contains("/secret") || ((string)v).Contains("boom"));
    }

    [Fact]
    public async Task Run_WhenDeletionErrorsExist_SetsHasErrorsToYes()
    {
        var payload = CreatePayload(isFailure: false, TransferDirection.EgressToNetApp, TransferType.Move);
        var entity = CreateEntity(payload, TransferStatus.PartiallyCompleted, successfulFiles: 1, totalFiles: 1);
        entity.DeletionErrors.Add(new DeletionError { FileId = "file-1", ErrorMessage = "delete failed" });
        SetupEntity(entity);

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.Equal("yes", (string)_sentPersonalisation!["has_errors"]);
    }

    [Fact]
    public async Task Run_WhenEntityNotYetFinalised_WaitsForFinalStatus()
    {
        var payload = CreatePayload(isFailure: false, TransferDirection.NetAppToEgress, TransferType.Copy);
        var reads = 0;
        _durableEntityClientStub.OnGetEntityAsync = (id, _) =>
        {
            reads++;
            var status = reads < 3 ? TransferStatus.InProgress : TransferStatus.Completed;
            return Task.FromResult<EntityMetadata<TransferEntity>?>(
                new EntityMetadata<TransferEntity>(id, CreateEntity(payload, status, successfulFiles: 4, totalFiles: 4)));
        };

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.Equal(3, reads);
        Assert.Equal("4", (string)_sentPersonalisation!["successful_files"]);
    }

    [Fact]
    public async Task Run_WhenTransferFailed_SendsFailedTemplateWithoutReadingEntity()
    {
        var payload = CreatePayload(isFailure: true, TransferDirection.NetAppToEgress, TransferType.Move);
        var entityRead = false;
        _durableEntityClientStub.OnGetEntityAsync = (_, _) =>
        {
            entityRead = true;
            return Task.FromResult<EntityMetadata<TransferEntity>?>(null);
        };

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.False(entityRead);
        Assert.Equal(_options.TransferFailedTemplateId, _sentTemplateId);
        Assert.Equal(["urn", "direction", "operation_type", "case_activity_log_url"], _sentPersonalisation!.Keys);
        Assert.Equal(Urn, (string)_sentPersonalisation["urn"]);
        Assert.Equal("Shared Drive to Egress", (string)_sentPersonalisation["direction"]);
        Assert.Equal("Move", (string)_sentPersonalisation["operation_type"]);
        Assert.Equal($"https://lcc.example/case/{payload.CaseId}/case-management", (string)_sentPersonalisation["case_activity_log_url"]);
    }

    [Fact]
    public async Task Run_WhenNotifyThrows_SwallowsExceptionAndTracksFailure()
    {
        var payload = CreatePayload(isFailure: true, TransferDirection.EgressToNetApp, TransferType.Copy);
        _notificationClientMock
            .Setup(c => c.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .ThrowsAsync(new NotifyClientException("Status code 503. Service unavailable"));

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.NotNull(_trackedEvent);
        Assert.Equal(TransferNotificationEvent.StatusFailed, _trackedEvent!.DispatchStatus);
        Assert.Contains("503", _trackedEvent.FailureReason);
        _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<NotifyClientException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenEntityNeverFound_SwallowsExceptionAndTracksFailure()
    {
        var payload = CreatePayload(isFailure: false, TransferDirection.EgressToNetApp, TransferType.Copy);
        _durableEntityClientStub.OnGetEntityAsync = (_, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(null);

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.Equal(TransferNotificationEvent.StatusFailed, _trackedEvent!.DispatchStatus);
        _notificationClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Run_WhenNotifyNotConfigured_SkipsDispatch()
    {
        var payload = CreatePayload(isFailure: true, TransferDirection.EgressToNetApp, TransferType.Copy);

        await CreateActivity(withClient: false).Run(payload, _durableTaskClientStub);

        Assert.Null(_sentEmail);
        Assert.Equal(TransferNotificationEvent.StatusSkipped, _trackedEvent!.DispatchStatus);
    }

    [Fact]
    public async Task Run_WhenNoRecipient_SkipsDispatch()
    {
        var payload = CreatePayload(isFailure: true, TransferDirection.EgressToNetApp, TransferType.Copy);
        payload.UserName = null;

        await CreateActivity().Run(payload, _durableTaskClientStub);

        _notificationClientMock.VerifyNoOtherCalls();
        Assert.Equal(TransferNotificationEvent.StatusSkipped, _trackedEvent!.DispatchStatus);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Run_WhenUrnMissing_FallsBackToCaseId(string? urn)
    {
        var payload = CreatePayload(isFailure: true, TransferDirection.EgressToNetApp, TransferType.Copy);
        payload.Urn = urn;

        await CreateActivity().Run(payload, _durableTaskClientStub);

        Assert.Equal(payload.CaseId.ToString(), (string)_sentPersonalisation!["urn"]);
    }

    [Theory]
    [InlineData(TransferDirection.NetAppToEgress, "Shared Drive to Egress")]
    [InlineData(TransferDirection.EgressToNetApp, "Egress to Shared Drive")]
    public void ToDisplayDirection_MapsToTemplateWording(TransferDirection direction, string expected) =>
        Assert.Equal(expected, SendTransferNotification.ToDisplayDirection(direction));

    [Theory]
    [InlineData(TransferType.Copy, "Copy")]
    [InlineData(TransferType.Move, "Move")]
    public void ToDisplayOperationType_MapsToTemplateWording(TransferType transferType, string expected) =>
        Assert.Equal(expected, SendTransferNotification.ToDisplayOperationType(transferType));

    private SendTransferNotification CreateActivity(bool withClient = true) =>
        new(
            withClient ? [_notificationClientMock.Object] : [],
            Options.Create(_options),
            _telemetryClientMock.Object,
            _initializationHandlerMock.Object,
            _loggerMock.Object)
        {
            FinalStatusRetryDelay = TimeSpan.Zero,
            DelayAsync = (_, _) => Task.CompletedTask
        };

    private void SetupEntity(TransferEntity entity) =>
        _durableEntityClientStub.OnGetEntityAsync = (id, _) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, entity));

    private static SendTransferNotificationPayload CreatePayload(bool isFailure, TransferDirection direction, TransferType transferType) =>
        new()
        {
            TransferId = Guid.NewGuid(),
            CaseId = 12345,
            Urn = Urn,
            UserName = Recipient,
            CorrelationId = Guid.NewGuid(),
            IsFailure = isFailure,
            TransferDirection = direction,
            TransferType = transferType
        };

    private static TransferEntity CreateEntity(SendTransferNotificationPayload payload, TransferStatus status, int successfulFiles, int totalFiles) =>
        new()
        {
            Id = payload.TransferId,
            CaseId = payload.CaseId,
            Status = status,
            Direction = payload.TransferDirection,
            TransferType = payload.TransferType,
            DestinationPath = "/dest",
            BearerToken = "token",
            SuccessfulFiles = successfulFiles,
            TotalFiles = totalFiles
        };
}
