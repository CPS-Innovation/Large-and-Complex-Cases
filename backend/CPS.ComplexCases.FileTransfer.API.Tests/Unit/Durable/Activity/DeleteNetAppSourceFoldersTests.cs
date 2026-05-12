using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class DeleteNetAppSourceFoldersTests
{
    private readonly Fixture _fixture;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<ILogger<DeleteNetAppSourceFolders>> _loggerMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly DeleteNetAppSourceFolders _activity;

    private const string BearerToken = "test-bearer";
    private const string BucketName = "test-bucket";
    private const string UserName = "testuser";

    public DeleteNetAppSourceFoldersTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _loggerMock = new Mock<ILogger<DeleteNetAppSourceFolders>>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string bearer, string bucket, string op, string path, bool isFolder) =>
                new DeleteFileOrFolderArg { BearerToken = bearer, BucketName = bucket, OperationName = op, Path = path });

        _netAppArgFactoryMock
            .Setup(f => f.CreateListObjectsInBucketArg(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string bearer, string bucket, string? contToken, int? maxKeys, string? prefix, bool includeDelim) =>
                new ListObjectsInBucketArg { BearerToken = bearer, BucketName = bucket, Prefix = prefix });

        // Default: folder is empty — safe to delete the folder marker.
        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(EmptyListResult());

        _activity = new DeleteNetAppSourceFolders(
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _loggerMock.Object,
            _initializationHandlerMock.Object);
    }

    [Fact]
    public async Task Run_WhenPayloadIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _activity.Run(null, CancellationToken.None));
    }

    [Fact]
    public async Task Run_WhenPayloadIsValid_CallsInitializationHandler()
    {
        var payload = CreatePayload(["folder/path"]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _initializationHandlerMock.Verify(
            h => h.Initialize(payload.UserName, payload.CorrelationId, payload.CaseId),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenNoSourceFolderPaths_DoesNotCallDelete()
    {
        var payload = CreatePayload([]);

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenMultipleFolderPaths_CallsDeleteForEachPath()
    {
        var paths = new List<string> { "Case/Folder1", "Case/Folder2", "Case/Folder3" };
        var payload = CreatePayload(paths);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Run_WhenDeletingFolder_CreatesArgWithFolderMarkerKeyAndIsFolderFalse()
    {
        var folderPath = "Case/SourceFolder";
        var payload = CreatePayload([folderPath]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        // The delete targets the folder marker key (trailing slash) non-recursively.
        _netAppArgFactoryMock.Verify(
            f => f.CreateDeleteFileOrFolderArg(
                BearerToken,
                BucketName,
                "DeleteSourceFolder",
                folderPath + "/",
                false),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenFolderPathAlreadyHasTrailingSlash_DoesNotDoubleSlash()
    {
        var folderPath = "Case/SourceFolder/";
        var payload = CreatePayload([folderPath]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppArgFactoryMock.Verify(
            f => f.CreateDeleteFileOrFolderArg(
                BearerToken,
                BucketName,
                "DeleteSourceFolder",
                "Case/SourceFolder/",
                false),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenDeleteSucceeds_CompletesWithoutException()
    {
        var payload = CreatePayload(["Case/Folder"]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        var ex = await Record.ExceptionAsync(() => _activity.Run(payload, CancellationToken.None));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Run_WhenDeleteReturnsUnsuccessfulResult_DoesNotThrow()
    {
        var payload = CreatePayload(["Case/Folder"]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(false, true, 0, "Permission denied", 403));

        var ex = await Record.ExceptionAsync(() => _activity.Run(payload, CancellationToken.None));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Run_WhenDeleteReturnsUnsuccessfulResult_ContinuesToNextFolder()
    {
        var paths = new List<string> { "Case/Failed", "Case/OK" };
        var payload = CreatePayload(paths);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "Case/Failed/")))
            .ReturnsAsync(new DeleteNetAppResult(false, true, 0, "Error", 500));

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "Case/OK/")))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Run_WhenDeleteThrowsException_DoesNotPropagateException()
    {
        var payload = CreatePayload(["Case/Folder"]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ThrowsAsync(new Exception("Unexpected storage failure"));

        var ex = await Record.ExceptionAsync(() => _activity.Run(payload, CancellationToken.None));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Run_WhenDeleteThrowsOnOnePath_ContinuesToNextFolder()
    {
        var paths = new List<string> { "Case/Problem", "Case/OK" };
        var payload = CreatePayload(paths);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "Case/Problem/")))
            .ThrowsAsync(new Exception("Unexpected error"));

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "Case/OK/")))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "Case/OK/")),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenListingFails_SkipsDeletion()
    {
        var payload = CreatePayload(["Case/Folder"]);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync((ListNetAppObjectsDto?)null);

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenListingReturnsOnlyFolderMarker_DeletesMarker()
    {
        var folderPath = "Case/Folder";
        var folderMarkerKey = folderPath + "/";
        var payload = CreatePayload([folderPath]);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(ListResultWithFiles([folderMarkerKey]));

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == folderMarkerKey)),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenListingReturnsFolderMarkerAndRealFiles_SkipsDeletion()
    {
        var folderPath = "Case/Folder";
        var folderMarkerKey = folderPath + "/";
        var payload = CreatePayload([folderPath]);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(ListResultWithFiles([folderMarkerKey, $"{folderMarkerKey}leftover.txt"]));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenFolderContainsAnyFiles_SkipsDeletion()
    {
        var payload = CreatePayload(["Case/Folder"]);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.IsAny<ListObjectsInBucketArg>()))
            .ReturnsAsync(ListResultWithFiles(["Case/Folder/file.txt"]));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenFolderIsEmpty_DeletesFolderMarker()
    {
        var folderPath = "Case/Folder";
        var payload = CreatePayload([folderPath]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == folderPath + "/")),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenOneOfTwoFoldersHasFiles_DeletesOnlyEmptyFolder()
    {
        var paths = new List<string> { "Case/Dirty", "Case/Clean" };
        var payload = CreatePayload(paths);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a => a.Prefix == "Case/Dirty/")))
            .ReturnsAsync(ListResultWithFiles(["Case/Dirty/surprise.txt"]));

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(It.Is<ListObjectsInBucketArg>(a => a.Prefix == "Case/Clean/")))
            .ReturnsAsync(EmptyListResult());

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "Case/Dirty/")),
            Times.Never);
        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "Case/Clean/")),
            Times.Once);
    }

    private DeleteNetAppSourceFoldersPayload CreatePayload(List<string> folderPaths) => new()
    {
        TransferId = _fixture.Create<Guid>(),
        BearerToken = BearerToken,
        BucketName = BucketName,
        UserName = UserName,
        CorrelationId = _fixture.Create<Guid>(),
        CaseId = _fixture.Create<int>(),
        SourceFolders = folderPaths
            .Select(p => new SourceFolderDeleteSpec { FolderPath = p })
            .ToList(),
    };

    private static ListNetAppObjectsDto EmptyListResult() =>
        ListResultWithFiles([]);

    private static ListNetAppObjectsDto ListResultWithFiles(IEnumerable<string> filePaths) => new()
    {
        Data = new ListNetAppDataDto
        {
            BucketName = BucketName,
            FileData = filePaths.Select(p => new ListNetAppFileDataDto
            {
                Path = p,
                Etag = "etag",
                Filesize = 100,
                LastModified = DateTime.UtcNow,
            }),
            FolderData = [],
        },
        Pagination = new PaginationDto(),
    };
}
