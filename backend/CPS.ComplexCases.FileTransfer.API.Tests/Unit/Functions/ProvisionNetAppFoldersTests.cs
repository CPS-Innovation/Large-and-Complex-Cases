using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class ProvisionNetAppFoldersTests
{
    private readonly Mock<ILogger<ProvisionNetAppFolders>> _loggerMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly DurableEntityClientStub _entityClientStub;
    private readonly DurableTaskClientStub _durableClientStub;
    private readonly ProvisionNetAppFolders _function;

    private const string BearerToken = "test-bearer-token";
    private const string BucketName = "test-bucket";
    private const int CaseId = 42;
    private const string TemplateName = "_templates/appeal/";
    private const string DestinationFolderPath = "CaseRoot/Case-42-Smith/";
    private const string UserName = "user@example.com";

    public ProvisionNetAppFoldersTests()
    {
        _loggerMock = new Mock<ILogger<ProvisionNetAppFolders>>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _entityClientStub = new DurableEntityClientStub("TestClient");
        _durableClientStub = new DurableTaskClientStub(_entityClientStub);

        _netAppArgFactoryMock
            .Setup(f => f.CreateListObjectsInBucketArg(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns((string bearer, string bucket, string? ct, int? maxKeys, string? prefix, bool delimiter) =>
                new ListObjectsInBucketArg
                {
                    BearerToken = bearer,
                    BucketName = bucket,
                    ContinuationToken = ct,
                    MaxKeys = maxKeys?.ToString(),
                    Prefix = prefix
                });

        _function = new ProvisionNetAppFolders(
            _loggerMock.Object,
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
    public async Task Run_WhenTemplateHasNoFiles_ReturnsBadRequest()
    {
        // ListFilesInFolder can return null or an empty list; either way Run
        // short-circuits with a BadRequest before scheduling the orchestrator.
        SetupValidRequest();
        SetupTemplateFiles(); // empty

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_ReturnsOk()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt", "_templates/appeal/file2.txt");
        var client = CreateSchedulingClient(out _);

        var result = await _function.Run(CreateHttpRequest(), client.Object);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_SchedulesCopyOrchestrator()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt");
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        Assert.Single(calls);
        Assert.Equal(nameof(CopyOrchestrator), calls[0].Name);
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_ScheduledPayloadHasCorrectCaseIdAndDestination()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt");
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<CopyBatchPayload>(calls[0].Input);
        Assert.Equal(CaseId, payload.CaseId);
        Assert.Equal(BearerToken, payload.BearerToken);
        Assert.Equal(BucketName, payload.BucketName);
        Assert.Equal(UserName, payload.UserName);
        Assert.All(payload.Files, f => Assert.Equal(DestinationFolderPath, f.DestinationPrefix));
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_ScheduledPayloadHasIncludeEmptyFoldersTrue()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt");
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<CopyBatchPayload>(calls[0].Input);
        Assert.True(payload.IncludeEmptyFolders);
    }

    [Fact]
    public async Task Run_WhenTemplateHasTwoFiles_ScheduledPayloadContainsBothAsFileItems()
    {
        const string file1 = "_templates/appeal/file1.txt";
        const string file2 = "_templates/appeal/file2.txt";
        SetupValidRequest();
        SetupTemplateFiles(file1, file2);
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<CopyBatchPayload>(calls[0].Input);
        Assert.Equal(2, payload.Files.Count);
        Assert.Contains(payload.Files, f => f.SourceKey == file1);
        Assert.Contains(payload.Files, f => f.SourceKey == file2);
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_FileItemsHaveRelativePathsAsDestinationFileName()
    {
        const string file = "_templates/appeal/subfolder/doc.pdf";
        SetupValidRequest();
        SetupTemplateFiles(file);
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<CopyBatchPayload>(calls[0].Input);
        var item = Assert.Single(payload.Files);
        // DestinationFileName should be the path relative to the template root
        Assert.Equal("subfolder/doc.pdf", item.DestinationFileName);
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_FileItemsHaveCorrectDestinationPrefix()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt");
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<CopyBatchPayload>(calls[0].Input);
        var item = Assert.Single(payload.Files);
        Assert.Equal(DestinationFolderPath, item.DestinationPrefix);
    }

    [Fact]
    public async Task Run_WhenSchedulerThrows_ExceptionPropagates()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt");

        var failingClient = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };
        failingClient
            .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(), It.IsAny<object>(),
                It.IsAny<StartOrchestrationOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Durable scheduler unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _function.Run(CreateHttpRequest(), failingClient.Object));
    }

    [Fact]
    public async Task Run_WhenTemplateHasFolderEntries_FolderPathsAreIncludedAsFileItems()
    {
        const string subFolder = "_templates/appeal/subfolder/";
        const string fileInSubfolder = "_templates/appeal/subfolder/doc.pdf";
        SetupValidRequest();
        SetupTemplateWithFolderAndFiles(subFolder, fileInSubfolder);
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<CopyBatchPayload>(calls[0].Input);
        // The folder entry itself and the file inside should both be present
        Assert.Contains(payload.Files, f => f.SourceKey == subFolder);
        Assert.Contains(payload.Files, f => f.SourceKey == fileInSubfolder);
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_InitializesHandlerWithRequestContext()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt");
        var correlationId = Guid.NewGuid();
        var client = CreateSchedulingClient(out _);
        var req = CreateHttpRequest(correlationId);

        await _function.Run(req, client.Object);

        _initializationHandlerMock.Verify(h => h.Initialize(
            UserName,
            correlationId,
            CaseId), Times.Once);
    }

    [Fact]
    public async Task Run_WhenTemplateHasFiles_OrchestrationInstanceIdMatchesTransferId()
    {
        SetupValidRequest();
        SetupTemplateFiles("_templates/appeal/file1.txt");
        var client = CreateSchedulingClient(out var calls);

        await _function.Run(CreateHttpRequest(), client.Object);

        var payload = Assert.IsType<CopyBatchPayload>(calls[0].Input);
        var options = calls[0].Options;
        Assert.NotNull(options);
        Assert.Equal(payload.TransferId.ToString(), options!.InstanceId);
    }

    [Fact]
    public async Task Run_WhenNetAppClientReturnsNullResponse_ReturnsBadRequest()
    {
        // When the NetApp client returns null, ListFilesInFolder returns null/empty
        // and Run short-circuits with a BadRequest before scheduling the orchestrator.
        SetupValidRequest();
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync((ListNetAppObjectsDto?)null);

        var result = await _function.Run(CreateHttpRequest(), _durableClientStub);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ListFilesInFolder_WhenResponseIsNull_ReturnsEmptyList()
    {
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync((ListNetAppObjectsDto?)null);

        var result = await _function.ListFilesInFolder(TemplateName, BearerToken, BucketName);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListFilesInFolder_WithSinglePageOfFiles_ReturnsAllFiles()
    {
        const string file1 = "_templates/appeal/a.txt";
        const string file2 = "_templates/appeal/b.txt";

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(MakeListResult([file1, file2], [], nextToken: null));

        var result = (await _function.ListFilesInFolder(TemplateName, BearerToken, BucketName))!.ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.SourcePath == file1);
        Assert.Contains(result, f => f.SourcePath == file2);
    }

    [Fact]
    public async Task ListFilesInFolder_WithPaginatedResponse_FollowsContinuationToken()
    {
        const string file1 = "_templates/appeal/a.txt";
        const string file2 = "_templates/appeal/b.txt";
        const string token = "page2-token";

        _netAppClientMock
            .SetupSequence(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(MakeListResult([file1], [], nextToken: token))
            .ReturnsAsync(MakeListResult([file2], [], nextToken: null));

        var result = (await _function.ListFilesInFolder(TemplateName, BearerToken, BucketName))!.ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.SourcePath == file1);
        Assert.Contains(result, f => f.SourcePath == file2);
    }

    [Fact]
    public async Task ListFilesInFolder_WithSubfolders_RecursivelyListsSubfolderContents()
    {
        const string subFolder = "_templates/appeal/subfolder/";
        const string fileInSubfolder = "_templates/appeal/subfolder/doc.pdf";

        // Root call returns a subfolder entry
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(
                It.Is<ListObjectsInBucketArg>(a => a.Prefix == TemplateName)))
            .ReturnsAsync(MakeListResult([], [subFolder], nextToken: null));

        // Recursive call for the subfolder returns a file
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(
                It.Is<ListObjectsInBucketArg>(a => a.Prefix == subFolder)))
            .ReturnsAsync(MakeListResult([fileInSubfolder], [], nextToken: null));

        var result = (await _function.ListFilesInFolder(TemplateName, BearerToken, BucketName))!.ToList();

        Assert.Contains(result, f => f.SourcePath == subFolder);
        Assert.Contains(result, f => f.SourcePath == fileInSubfolder);
    }

    [Fact]
    public async Task ListFilesInFolder_WithEmptyResponse_ReturnsEmptyList()
    {
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(MakeListResult([], [], nextToken: null));

        var result = (await _function.ListFilesInFolder(TemplateName, BearerToken, BucketName))!.ToList();

        Assert.Empty(result);
    }

    private void SetupInvalidRequest() =>
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<ProvisionNetAppFoldersRequest, ProvisionNetAppFoldersRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<ProvisionNetAppFoldersRequest>
            {
                Value = null!,
                IsValid = false,
                ValidationErrors = ["TemplateName is required."]
            });

    private void SetupValidRequest() =>
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<ProvisionNetAppFoldersRequest, ProvisionNetAppFoldersRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<ProvisionNetAppFoldersRequest>
            {
                IsValid = true,
                Value = new ProvisionNetAppFoldersRequest
                {
                    CaseId = CaseId,
                    Urn = "12AB3456",
                    TemplateName = TemplateName,
                    DestinationFolderPath = DestinationFolderPath,
                    BucketName = BucketName,
                    BearerToken = BearerToken,
                    UserName = UserName,
                }
            });

    private void SetupTemplateFiles(params string[] filePaths) =>
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(MakeListResult(filePaths, [], nextToken: null));

    private void SetupTemplateWithFolderAndFiles(string subFolder, string fileInSubfolder)
    {
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(
                It.Is<ListObjectsInBucketArg>(a => a.Prefix == TemplateName)))
            .ReturnsAsync(MakeListResult([], [subFolder], nextToken: null));

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(
                It.Is<ListObjectsInBucketArg>(a => a.Prefix == subFolder)))
            .ReturnsAsync(MakeListResult([fileInSubfolder], [], nextToken: null));
    }

    private Mock<DurableTaskClientStub> CreateSchedulingClient(
        out List<(string Name, object? Input, StartOrchestrationOptions? Options)> capturedCalls)
    {
        var calls = new List<(string, object?, StartOrchestrationOptions?)>();
        capturedCalls = calls;

        var mock = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };
        mock.Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(), It.IsAny<object>(),
                It.IsAny<StartOrchestrationOptions>(), It.IsAny<CancellationToken>()))
            .Callback<TaskName, object, StartOrchestrationOptions, CancellationToken>(
                (name, input, options, _) => calls.Add((name.Name, input, options)))
            .ReturnsAsync("test-instance-id");

        return mock;
    }

    private static HttpRequest CreateHttpRequest(Guid? correlationId = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HttpHeaderKeys.CorrelationId] = (correlationId ?? Guid.NewGuid()).ToString();
        return context.Request;
    }

    private static ListNetAppObjectsDto MakeListResult(string[] filePaths, string[] folderPaths, string? nextToken) => new()
    {
        Data = new ListNetAppDataDto
        {
            BucketName = BucketName,
            FileData = filePaths.Select(p => new ListNetAppFileDataDto
            {
                Path = p,
                Etag = "etag",
                Filesize = 1024,
                LastModified = DateTime.UtcNow
            }),
            FolderData = folderPaths.Select(p => new ListNetAppFolderDataDto { Path = p })
        },
        Pagination = new PaginationDto { NextContinuationToken = nextToken }
    };
}
