using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Responses;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class InitiateBatchMoveTests
{
    private readonly Mock<ILogger<InitiateBatchMove>> _loggerMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<ICaseActiveManageMaterialsService> _caseActiveManageMaterialsServiceMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly DurableEntityClientStub _entityClientStub;
    private readonly DurableTaskClientStub _durableClientStub;
    private readonly InitiateBatchMove _function;
    private readonly Fixture _fixture;

    private const string BearerToken = "test-bearer-token";
    private const string BucketName = "test-bucket";
    private const int CaseId = 42;
    private const string DestinationPrefix = "CaseRoot/Dest/";
    private const string MaterialSourcePath = "CaseRoot/Source/report.pdf";
    private const string MaterialDestinationKey = "CaseRoot/Dest/report.pdf";
    private const string FolderSourcePath = "CaseRoot/OldFolder/";

    public InitiateBatchMoveTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<InitiateBatchMove>>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _caseActiveManageMaterialsServiceMock = new Mock<ICaseActiveManageMaterialsService>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _entityClientStub = new DurableEntityClientStub("TestClient");
        _durableClientStub = new DurableTaskClientStub(_entityClientStub);

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string bearer, string bucket, string key, string? etag) =>
                new GetObjectArg { BearerToken = bearer, BucketName = bucket, ObjectKey = key });

        _netAppArgFactoryMock
            .Setup(f => f.CreateListObjectsInBucketArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns((string bearer, string bucket, string? ct, int? maxKeys, string? prefix, bool delimiter) =>
                new ListObjectsInBucketArg
                {
                    BearerToken = bearer,
                    BucketName = bucket,
                    ContinuationToken = ct,
                    MaxKeys = maxKeys?.ToString(),
                    Prefix = prefix
                });

        _caseActiveManageMaterialsServiceMock
            .Setup(s => s.CheckConflictAndInsertAsync(
                It.IsAny<CaseActiveManageMaterialsOperation>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(true);

        _function = new InitiateBatchMove(
            _loggerMock.Object,
            _caseMetadataServiceMock.Object,
            _caseActiveManageMaterialsServiceMock.Object,
            _requestValidatorMock.Object,
            _initializationHandlerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object);
    }

    [Fact]
    public async Task Run_WithInvalidRequest_ReturnsBadRequest()
    {
        SetupInvalidRequest();

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(bad.Value);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task Run_WhenActiveTransferIsInProgress_ReturnsConflict()
    {
        var transferId = _fixture.Create<Guid>();
        SetupValidRequest();
        SetupActiveTransferInProgress(transferId);

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenActiveTransferIsNotInProgress_DoesNotBlock()
    {
        var transferId = _fixture.Create<Guid>();
        SetupValidRequest();
        SetupActiveTransferNotInProgress(transferId);
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out _);

        var result = await _function.Run(CreateHttpRequest(), client.Object);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task Run_WhenCaseMetadataIsNull_ActiveTransferCheckIsSkipped()
    {
        SetupValidRequest();
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(CaseId))
            .ReturnsAsync((CaseMetadata?)null);
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out _);

        var result = await _function.Run(CreateHttpRequest(), client.Object);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task Run_WhenConflictingManageMaterialsOperation_ReturnsConflict()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        _caseActiveManageMaterialsServiceMock
            .Setup(s => s.CheckConflictAndInsertAsync(
                It.IsAny<CaseActiveManageMaterialsOperation>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(false);

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Run_ConflictCheck_PassesSourcePathsAndDestinationPrefixToService()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out _);

        await _function.Run(CreateHttpRequest(), client.Object);

        _caseActiveManageMaterialsServiceMock.Verify(s =>
            s.CheckConflictAndInsertAsync(
                It.IsAny<CaseActiveManageMaterialsOperation>(),
                It.Is<IEnumerable<string>>(paths => paths.Contains(MaterialSourcePath)),
                It.Is<IEnumerable<string>>(paths => paths.Contains(DestinationPrefix))),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenMaterialSourceNotFound_ReturnsNotFound()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceMissing();

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(notFound.Value);
        Assert.Contains(errors, e => e.Contains(MaterialSourcePath));
    }

    [Fact]
    public async Task Run_WhenMaterialDestinationAlreadyExists_ReturnsConflict()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationExists();

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(conflict.Value);
        Assert.Contains(errors, e => e.Contains(MaterialDestinationKey));
    }

    [Fact]
    public async Task Run_WhenCaseInsensitiveClashAtDestination_ReturnsConflict()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == DestinationPrefix && a.MaxKeys == null)))
            .ReturnsAsync(MakeListResult([$"{DestinationPrefix}REPORT.PDF"]));

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenFolderSourceNotFound_ReturnsNotFound()
    {
        SetupValidRequest(CreateFolderRequest());
        SetupNoActiveTransfer();
        SetupFolderMissing(FolderSourcePath);

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(notFound.Value);
        Assert.Contains(errors, e => e.Contains(FolderSourcePath.TrimEnd('/')));
    }

    [Fact]
    public async Task Run_WhenFolderMoveDestinationFileAlreadyExists_ReturnsConflict()
    {
        var destFolderPrefix = $"{DestinationPrefix}OldFolder/";
        var sourceFile = $"{FolderSourcePath}a.pdf";
        var conflictingDestKey = $"{destFolderPrefix}a.pdf";

        SetupValidRequest(CreateFolderRequest());
        SetupNoActiveTransfer();
        SetupFolderExists(FolderSourcePath, sourceFile);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == destFolderPrefix && a.MaxKeys == null)))
            .ReturnsAsync(MakeListResult([conflictingDestKey]));

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(conflict.Value);
        Assert.Contains(errors, e => e.Contains(conflictingDestKey));
    }

    [Fact]
    public async Task Run_WithValidMaterialRequest_Returns202Accepted()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out _);

        var result = await _function.Run(CreateHttpRequest(), client.Object);

        var accepted = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<TransferResponse>(accepted.Value);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(TransferStatus.Initiated, response.Status);
        Assert.True((DateTime.UtcNow - response.CreatedAt).TotalSeconds < 10);
        Assert.Equal($"/api/v1/filetransfer/{response.Id}/status", accepted.Location);
    }

    [Fact]
    public async Task Run_WithValidMaterialRequest_SchedulesMoveOrchestratorWithCorrectPayload()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out var capturedCalls);

        await _function.Run(CreateHttpRequest(), client.Object);

        Assert.Single(capturedCalls);
        Assert.Equal(nameof(MoveOrchestrator), capturedCalls[0].Name);

        var payload = Assert.IsType<MoveBatchPayload>(capturedCalls[0].Input);
        Assert.Equal(CaseId, payload.CaseId);
        Assert.Equal(BearerToken, payload.BearerToken);
        Assert.Equal(BucketName, payload.BucketName);
        Assert.Single(payload.Files);
        Assert.Equal(MaterialSourcePath, payload.Files[0].SourceKey);
        Assert.Equal(DestinationPrefix, payload.Files[0].DestinationPrefix);
        Assert.Equal("report.pdf", payload.Files[0].DestinationFileName);
    }

    [Fact]
    public async Task Run_WithValidMaterialRequest_InsertsBatchMoveOperationRow()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out _);

        await _function.Run(CreateHttpRequest(), client.Object);

        _caseActiveManageMaterialsServiceMock.Verify(s =>
            s.CheckConflictAndInsertAsync(
                It.Is<CaseActiveManageMaterialsOperation>(op =>
                    op.CaseId == CaseId && op.OperationType == "BatchMove"),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithValidMaterialRequest_ManageMaterialsOperationIdMatchesTransferId()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        CaseActiveManageMaterialsOperation? capturedOp = null;
        _caseActiveManageMaterialsServiceMock
            .Setup(s => s.CheckConflictAndInsertAsync(
                It.IsAny<CaseActiveManageMaterialsOperation>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()))
            .Callback<CaseActiveManageMaterialsOperation, IEnumerable<string>, IEnumerable<string>>(
                (op, _, _) => capturedOp = op)
            .ReturnsAsync(true);
        var client = CreateSchedulingClient(out var capturedCalls);

        await _function.Run(CreateHttpRequest(), client.Object);

        Assert.NotNull(capturedOp);
        var payload = Assert.IsType<MoveBatchPayload>(capturedCalls[0].Input);
        Assert.Equal(capturedOp!.Id, payload.TransferId);
        Assert.Equal(capturedOp.Id, payload.ManageMaterialsOperationId);
    }

    [Fact]
    public async Task Run_WithValidMaterialRequest_OriginalOperationsArePresentInPayload()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out var capturedCalls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<MoveBatchPayload>(capturedCalls[0].Input);
        Assert.Single(payload.OriginalOperations);
        Assert.Equal("Material", payload.OriginalOperations[0].Type);
        Assert.Equal(MaterialSourcePath, payload.OriginalOperations[0].SourcePath);
    }

    [Fact]
    public async Task Run_WithValidMaterialRequest_OrchestrationInstanceIdMatchesResponseTransferId()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out var capturedCalls);

        var result = await _function.Run(CreateHttpRequest(), client.Object);

        var accepted = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<TransferResponse>(accepted.Value);
        Assert.Equal(capturedCalls[0].Options!.InstanceId, response.Id.ToString());
    }

    [Fact]
    public async Task Run_WithFolderRequest_Returns202Accepted()
    {
        SetupValidRequest(CreateFolderRequest());
        SetupNoActiveTransfer();
        SetupFolderExists(FolderSourcePath, $"{FolderSourcePath}file.pdf");
        var client = CreateSchedulingClient(out _);

        var result = await _function.Run(CreateHttpRequest(), client.Object);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task Run_WithFolderRequest_ExpandsSourceKeysToPerFileItems()
    {
        var file1 = $"{FolderSourcePath}doc1.pdf";
        var file2 = $"{FolderSourcePath}sub/doc2.docx";
        SetupValidRequest(CreateFolderRequest());
        SetupNoActiveTransfer();
        SetupFolderExists(FolderSourcePath, file1, file2);
        var client = CreateSchedulingClient(out var capturedCalls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<MoveBatchPayload>(capturedCalls[0].Input);
        Assert.Equal(2, payload.Files.Count);
        var expectedDestPrefix = $"{DestinationPrefix}OldFolder/";
        Assert.All(payload.Files, f => Assert.Equal(expectedDestPrefix, f.DestinationPrefix));
        Assert.Contains(payload.Files, f => f.SourceKey == file1 && f.DestinationFileName == "doc1.pdf");
        Assert.Contains(payload.Files, f => f.SourceKey == file2 && f.DestinationFileName == "sub/doc2.docx");
    }

    [Fact]
    public async Task Run_WithFolderRequest_PopulatesExpectedSourceKeysOnFolderOperation()
    {
        var file1 = $"{FolderSourcePath}doc1.pdf";
        var file2 = $"{FolderSourcePath}sub/doc2.docx";
        SetupValidRequest(CreateFolderRequest());
        SetupNoActiveTransfer();
        SetupFolderExists(FolderSourcePath, file1, file2);
        var client = CreateSchedulingClient(out var capturedCalls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<MoveBatchPayload>(capturedCalls[0].Input);
        Assert.Single(payload.OriginalOperations);
        var folderOp = payload.OriginalOperations[0];
        Assert.Equal("Folder", folderOp.Type);
        Assert.Equal(2, folderOp.ExpectedSourceKeys.Count);
        Assert.Contains(file1, folderOp.ExpectedSourceKeys);
        Assert.Contains(file2, folderOp.ExpectedSourceKeys);
    }

    [Fact]
    public async Task Run_WithValidMaterialRequest_ExpectedSourceKeysIsEmpty()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();
        var client = CreateSchedulingClient(out var capturedCalls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<MoveBatchPayload>(capturedCalls[0].Input);
        Assert.Single(payload.OriginalOperations);
        Assert.Empty(payload.OriginalOperations[0].ExpectedSourceKeys);
    }

    [Fact]
    public async Task Run_WhenSchedulingThrows_DeletesActiveOperationRowAndRethrows()
    {
        SetupValidRequest();
        SetupNoActiveTransfer();
        SetupMaterialSourceExists();
        SetupMaterialDestinationMissing();
        SetupNoClashAtDestination();

        Guid? capturedTransferId = null;
        _caseActiveManageMaterialsServiceMock
            .Setup(s => s.CheckConflictAndInsertAsync(
                It.IsAny<CaseActiveManageMaterialsOperation>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()))
            .Callback<CaseActiveManageMaterialsOperation, IEnumerable<string>, IEnumerable<string>>(
                (op, _, _) => capturedTransferId = op.Id)
            .ReturnsAsync(true);

        var failingClient = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };
        failingClient.Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<StartOrchestrationOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Durable storage unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _function.Run(CreateHttpRequest(), failingClient.Object));

        Assert.NotNull(capturedTransferId);
        _caseActiveManageMaterialsServiceMock.Verify(
            s => s.DeleteOperationAsync(capturedTransferId!.Value),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenNoFilesFoundAfterFolderExpansion_ReturnsBadRequest()
    {
        SetupValidRequest(CreateFolderRequest());
        SetupNoActiveTransfer();

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == FolderSourcePath && a.MaxKeys == "1")))
            .ReturnsAsync(MakeListResult([$"{FolderSourcePath}file.pdf"]));

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == FolderSourcePath && a.MaxKeys == null)))
            .ReturnsAsync(EmptyListResult());

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private void SetupValidRequest(MoveNetAppBatchRequest? request = null)
    {
        request ??= CreateMaterialRequest();
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MoveNetAppBatchRequest, MoveNetAppBatchRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MoveNetAppBatchRequest>
            {
                Value = request,
                IsValid = true,
                ValidationErrors = []
            });
    }

    private void SetupInvalidRequest()
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MoveNetAppBatchRequest, MoveNetAppBatchRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MoveNetAppBatchRequest>
            {
                Value = null!,
                IsValid = false,
                ValidationErrors = ["Operations cannot be empty."]
            });
    }

    private void SetupNoActiveTransfer()
    {
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(CaseId))
            .ReturnsAsync(new CaseMetadata { CaseId = CaseId, ActiveTransferId = null });
    }

    private void SetupActiveTransferInProgress(Guid transferId)
    {
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(CaseId))
            .ReturnsAsync(new CaseMetadata { CaseId = CaseId, ActiveTransferId = transferId });

        _entityClientStub.OnGetEntityAsync = (id, _) =>
        {
            var entity = new TransferEntity { Status = TransferStatus.InProgress, BearerToken = "x", DestinationPath = "/" };
            return Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, entity));
        };
    }

    private void SetupActiveTransferNotInProgress(Guid transferId)
    {
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(CaseId))
            .ReturnsAsync(new CaseMetadata { CaseId = CaseId, ActiveTransferId = transferId });

        _entityClientStub.OnGetEntityAsync = (id, _) =>
        {
            var entity = new TransferEntity { Status = TransferStatus.Completed, BearerToken = "x", DestinationPath = "/" };
            return Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, entity));
        };
    }

    private void SetupMaterialSourceExists() =>
        _netAppClientMock
            .Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == MaterialSourcePath)))
            .ReturnsAsync(true);

    private void SetupMaterialSourceMissing() =>
        _netAppClientMock
            .Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == MaterialSourcePath)))
            .ReturnsAsync(false);

    private void SetupMaterialDestinationMissing() =>
        _netAppClientMock
            .Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == MaterialDestinationKey)))
            .ReturnsAsync(false);

    private void SetupMaterialDestinationExists() =>
        _netAppClientMock
            .Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == MaterialDestinationKey)))
            .ReturnsAsync(true);

    private void SetupNoClashAtDestination() =>
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == DestinationPrefix && a.MaxKeys == null)))
            .ReturnsAsync(EmptyListResult());

    private void SetupFolderExists(string folderPrefix, params string[] filePaths)
    {
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == folderPrefix && a.MaxKeys == "1")))
            .ReturnsAsync(MakeListResult(filePaths.Take(1).ToArray()));

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == folderPrefix && a.MaxKeys == null)))
            .ReturnsAsync(MakeListResult(filePaths));
    }

    private void SetupFolderMissing(string folderPrefix) =>
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a =>
                a.Prefix == folderPrefix && a.MaxKeys == "1")))
            .ReturnsAsync(EmptyListResult());

    private Mock<DurableTaskClientStub> CreateSchedulingClient(
        out List<(string Name, object? Input, StartOrchestrationOptions? Options)> capturedCalls)
    {
        var calls = new List<(string, object?, StartOrchestrationOptions?)>();
        capturedCalls = calls;

        var mock = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };
        mock.Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<StartOrchestrationOptions>(), It.IsAny<CancellationToken>()))
            .Callback<TaskName, object, StartOrchestrationOptions, CancellationToken>(
                (name, input, options, _) => calls.Add((name.Name, input, options)))
            .ReturnsAsync("test-instance-id");

        return mock;
    }

    private static MoveNetAppBatchRequest CreateMaterialRequest() => new()
    {
        CaseId = CaseId,
        DestinationPrefix = DestinationPrefix,
        Operations =
        [
            new MoveNetAppBatchOperationRequest { Type = "Material", SourcePath = MaterialSourcePath }
        ],
        BearerToken = BearerToken,
        BucketName = BucketName,
        UserName = "user@example.com",
    };

    private static MoveNetAppBatchRequest CreateFolderRequest() => new()
    {
        CaseId = CaseId,
        DestinationPrefix = DestinationPrefix,
        Operations =
        [
            new MoveNetAppBatchOperationRequest { Type = "Folder", SourcePath = FolderSourcePath }
        ],
        BearerToken = BearerToken,
        BucketName = BucketName,
        UserName = "user@example.com",
    };

    private static HttpRequest CreateHttpRequest()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HttpHeaderKeys.CorrelationId] = Guid.NewGuid().ToString();
        return context.Request;
    }

    private static ListNetAppObjectsDto EmptyListResult() => new()
    {
        Data = new ListNetAppDataDto
        {
            BucketName = BucketName,
            FileData = [],
            FolderData = []
        },
        Pagination = new PaginationDto()
    };

    private static ListNetAppObjectsDto MakeListResult(string[] paths) => new()
    {
        Data = new ListNetAppDataDto
        {
            BucketName = BucketName,
            FileData = paths.Select(p => new ListNetAppFileDataDto
            {
                Path = p,
                Etag = "etag",
                Filesize = 1024,
                LastModified = DateTime.UtcNow
            }),
            FolderData = []
        },
        Pagination = new PaginationDto()
    };
}
