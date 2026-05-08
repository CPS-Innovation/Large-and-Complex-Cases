using System.Text.Json;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class WriteCopyActivityLogTests
{
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<ILogger<WriteCopyActivityLog>> _loggerMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly WriteCopyActivityLog _activity;

    private const int CaseId = 42;

    public WriteCopyActivityLogTests()
    {
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _loggerMock = new Mock<ILogger<WriteCopyActivityLog>>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<JsonDocument?>()))
            .Returns(Task.CompletedTask);

        _activity = new WriteCopyActivityLog(
            _activityLogServiceMock.Object,
            _loggerMock.Object,
            _initializationHandlerMock.Object);
    }


    [Fact]
    public async Task Run_WhenAllFolderKeysSucceed_LogsFolderOutcomeAsCopied()
    {
        var payload = BuildPayload(
            operations: [FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf", "Cases/123/FolderA/b.pdf"])],
            successfulKeys: ["Cases/123/FolderA/a.pdf", "Cases/123/FolderA/b.pdf"]);

        await _activity.Run(payload);

        var details = CapturedDetails();
        var items = ExtractItems(details);
        Assert.Equal("Copied", items[0].outcome);
    }


    [Fact]
    public async Task Run_WhenSomeFolderKeysSucceed_LogsFolderOutcomeAsPartial()
    {
        var payload = BuildPayload(
            operations: [FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf", "Cases/123/FolderA/b.pdf", "Cases/123/FolderA/c.pdf"])],
            successfulKeys: ["Cases/123/FolderA/a.pdf"]);

        await _activity.Run(payload);

        var details = CapturedDetails();
        var items = ExtractItems(details);
        Assert.Equal("Partial", items[0].outcome);
    }


    [Fact]
    public async Task Run_WhenNoFolderKeysSucceed_DoesNotWriteActivityLog()
    {
        var payload = BuildPayload(
            operations: [FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf", "Cases/123/FolderA/b.pdf"])],
            successfulKeys: []);

        await _activity.Run(payload);

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()), Times.Never);
    }


    [Fact]
    public async Task Run_WhenMaterialKeySucceeds_LogsMaterialOutcomeAsCopied()
    {
        var payload = BuildPayload(
            operations: [MaterialOp("Cases/123/report.pdf")],
            successfulKeys: ["Cases/123/report.pdf"]);

        await _activity.Run(payload);

        var details = CapturedDetails();
        var items = ExtractItems(details);
        Assert.Equal("Copied", items[0].outcome);
    }

    [Fact]
    public async Task Run_WhenMaterialKeyFails_DoesNotWriteActivityLog()
    {
        var payload = BuildPayload(
            operations: [MaterialOp("Cases/123/report.pdf")],
            successfulKeys: []);

        await _activity.Run(payload);

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()), Times.Never);
    }

    [Fact]
    public async Task Run_WithMixedBatch_PartialFolderAndCopiedMaterial_WritesLog()
    {
        var payload = BuildPayload(
            operations:
            [
                FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf", "Cases/123/FolderA/b.pdf"]),
                MaterialOp("Cases/123/report.pdf"),
            ],
            successfulKeys: ["Cases/123/FolderA/a.pdf", "Cases/123/report.pdf"]);

        await _activity.Run(payload);

        var details = CapturedDetails();
        var items = ExtractItems(details);
        Assert.Equal(2, items.Length);

        var folderItem = items.Single(i => i.type == "Folder");
        Assert.Equal("Partial", folderItem.outcome);

        var materialItem = items.Single(i => i.type == "Material");
        Assert.Equal("Copied", materialItem.outcome);
    }

    [Fact]
    public async Task Run_WhenAllOperationsAreNotCopied_DoesNotWriteActivityLog()
    {
        var payload = BuildPayload(
            operations:
            [
                FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf"]),
                MaterialOp("Cases/123/report.pdf"),
            ],
            successfulKeys: []);

        await _activity.Run(payload);

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenOnlyFolderCopied_UsesFolderCopiedActionType()
    {
        var payload = BuildPayload(
            operations: [FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf"])],
            successfulKeys: ["Cases/123/FolderA/a.pdf"]);

        await _activity.Run(payload);

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            ActionType.FolderCopied, It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()), Times.Once);
    }

    [Fact]
    public async Task Run_WhenOnlyMaterialCopied_UsesMaterialCopiedActionType()
    {
        var payload = BuildPayload(
            operations: [MaterialOp("Cases/123/report.pdf")],
            successfulKeys: ["Cases/123/report.pdf"]);

        await _activity.Run(payload);

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            ActionType.MaterialCopied, It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()), Times.Once);
    }

    [Fact]
    public async Task Run_WhenFolderAndMaterialBothReportable_UsesFolderAndMaterialCopiedActionType()
    {
        var payload = BuildPayload(
            operations:
            [
                FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf"]),
                MaterialOp("Cases/123/report.pdf"),
            ],
            successfulKeys: ["Cases/123/FolderA/a.pdf", "Cases/123/report.pdf"]);

        await _activity.Run(payload);

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            ActionType.FolderAndMaterialCopied, It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()), Times.Once);
    }

    [Fact]
    public async Task Run_WhenPartialFolderAndNoMaterial_UsesFolderCopiedActionType()
    {
        var payload = BuildPayload(
            operations: [FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf", "Cases/123/FolderA/b.pdf"])],
            successfulKeys: ["Cases/123/FolderA/a.pdf"]);

        await _activity.Run(payload);

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            ActionType.FolderCopied, It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()), Times.Once);
    }

    [Fact]
    public async Task Run_WhenMaterialCopied_LogsComputedDestinationPath()
    {
        var payload = BuildPayload(
            operations: [MaterialOp("Cases/123/report.pdf", destinationPrefix: "Cases/123/Disclosure/")],
            successfulKeys: ["Cases/123/report.pdf"]);

        await _activity.Run(payload);

        var details = CapturedDetails();
        var items = ExtractItems(details);
        Assert.Equal("Cases/123/Disclosure/report.pdf", items[0].destinationPath);
    }

    [Fact]
    public async Task Run_WhenFolderCopied_LogsDestinationFolderPrefix()
    {
        var payload = BuildPayload(
            operations: [FolderOp("Cases/123/FolderA/", ["Cases/123/FolderA/a.pdf"], destinationPrefix: "Cases/123/Disclosure/FolderA/")],
            successfulKeys: ["Cases/123/FolderA/a.pdf"]);

        await _activity.Run(payload);

        var details = CapturedDetails();
        var items = ExtractItems(details);
        Assert.Equal("Cases/123/Disclosure/FolderA/", items[0].destinationPath);
    }

    private static WriteCopyActivityLogPayload BuildPayload(
        List<CopyBatchOriginalOperation> operations,
        List<string> successfulKeys) => new()
        {
            CaseId = CaseId,
            UserName = "user@example.com",
            CorrelationId = Guid.NewGuid(),
            OriginalOperations = operations,
            SuccessfulSourceKeys = successfulKeys,
        };

    private static CopyBatchOriginalOperation FolderOp(string sourcePath, List<string> expectedSourceKeys, string destinationPrefix = "Cases/123/Dest/FolderA/") => new()
    {
        Type = "Folder",
        SourcePath = sourcePath,
        DestinationPrefix = destinationPrefix,
        ExpectedSourceKeys = expectedSourceKeys,
    };

    private static CopyBatchOriginalOperation MaterialOp(string sourcePath, string destinationPrefix = "Cases/123/Dest/") => new()
    {
        Type = "Material",
        SourcePath = sourcePath,
        DestinationPrefix = destinationPrefix,
    };

    private JsonDocument CapturedDetails()
    {
        JsonDocument? captured = null;

        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<JsonDocument?>()),
            Times.Once);

        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<JsonDocument?>()))
            .Callback<ActionType, ResourceType, int, string, string?, string?, JsonDocument?>(
                (_, _, _, _, _, _, doc) => captured = doc)
            .Returns(Task.CompletedTask);

        var invocation = _activityLogServiceMock.Invocations
            .First(i => i.Method.Name == nameof(IActivityLogService.CreateActivityLogAsync));
        captured = (JsonDocument?)invocation.Arguments[6];

        return captured!;
    }

    private static (string outcome, string type, string destinationPath)[] ExtractItems(JsonDocument details)
    {
        var itemsEl = details.RootElement.GetProperty("items");
        return itemsEl.EnumerateArray()
            .Select(el => (
                outcome: el.GetProperty("outcome").GetString()!,
                type: el.GetProperty("type").GetString()!,
                destinationPath: el.GetProperty("destinationPath").GetString()!))
            .ToArray();
    }
}
