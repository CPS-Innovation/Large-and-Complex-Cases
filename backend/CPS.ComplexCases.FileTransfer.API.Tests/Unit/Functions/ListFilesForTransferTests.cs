using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Results;
using CPS.ComplexCases.FileTransfer.API.Validators;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class ListFilesForTransferTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<ListFilesForTransfer>> _loggerMock;
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock;
    private readonly Mock<IStorageClient> _storageClientMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<IEgressClient> _egressClientMock;
    private readonly ListFilesForTransfer _function;
    private readonly int _testCaseId;
    private readonly string _testWorkspaceId;
    private readonly Guid _testCorrelationId;

    public ListFilesForTransferTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<ListFilesForTransfer>>();
        _storageClientFactoryMock = new Mock<IStorageClientFactory>();
        _storageClientMock = new Mock<IStorageClient>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _egressClientMock = new Mock<IEgressClient>();

        _testWorkspaceId = _fixture.Create<string>();
        _testCaseId = _fixture.Create<int>();
        _testCorrelationId = _fixture.Create<Guid>();

        _function = new ListFilesForTransfer(_loggerMock.Object, _storageClientFactoryMock.Object, _requestValidatorMock.Object, _egressClientMock.Object);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;

        reqMock.Setup(r => r.Body).Returns(new MemoryStream());

        var validationErrors = new List<string> { "Invalid request" };

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = false,
            Value = CreateRequest(TransferDirection.EgressToNetApp),
            ValidationErrors = validationErrors
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(validationResult.ValidationErrors, badRequest.Value);
    }

    [Fact]
    public async Task Run_ReturnsOk_WithFilesForTransferResult()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = true,
            Value = CreateRequest(TransferDirection.EgressToNetApp),
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        var filesForTransfer = new List<FileTransferInfo>
        {
            new () {
                SourcePath = "source/file1.txt",
                RelativePath = "file1.txt"
             }
        };

        _storageClientFactoryMock.Setup(f => f.GetSourceClientForDirection(TransferDirection.EgressToNetApp))
            .Returns(_storageClientMock.Object);

        _storageClientMock.Setup(c => c.ListFilesForTransferAsync(
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId, null, null))
            .ReturnsAsync(filesForTransfer);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        Assert.NotNull(filesResult);
        Assert.False(filesResult.IsInvalid);
    }

    [Fact]
    public async Task Run_EgressToNetApp_WithInvalidPaths_SetsIsInvalidCorrectly()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;
        var destinationPath = new string('a', 261);
        var sourcePath = "file1.txt";
        var expectedErrorMessage = $"{destinationPath}/{sourcePath}: exceeds the 260 characters limit.";

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = true,
            Value = CreateRequest(TransferDirection.EgressToNetApp, destinationPath),
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        var filesForTransfer = new List<FileTransferInfo>
        {
            new () {
                SourcePath = sourcePath,
            }
        };

        _storageClientFactoryMock.Setup(f => f.GetSourceClientForDirection(TransferDirection.EgressToNetApp))
            .Returns(_storageClientMock.Object);

        _storageClientMock.Setup(c => c.ListFilesForTransferAsync(
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId, null, null))
            .ReturnsAsync(filesForTransfer);

        var destinationPaths = filesForTransfer.Select(x => new DestinationPath
        {
            Path = $"{destinationPath}{x.RelativePath}"
        }).ToList();

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        Assert.True(filesResult.IsInvalid);

        Assert.NotNull(filesResult.ValidationErrors);
        var error = filesResult.ValidationErrors.First();
        Assert.NotNull(error);
        Assert.Equal(TransferFailedType.PathLengthExceeded, error.ErrorType);
        Assert.Equal(expectedErrorMessage, error.Message);
        Assert.Equal(sourcePath, error.SourcePath);
    }

    [Fact]
    public async Task Run_NetAppToEgress_SkipsPathValidation()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;
        var destinationPath = new string('a', 256);
        var relativePath = "file1.txt";

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = true,
            Value = CreateRequest(TransferDirection.NetAppToEgress),
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        var filesForTransfer = new List<FileTransferInfo>
        {
            new () {
                SourcePath = $"source/{relativePath}",
                RelativePath = relativePath
            }
        };

        _storageClientFactoryMock.Setup(f => f.GetSourceClientForDirection(TransferDirection.NetAppToEgress))
            .Returns(_storageClientMock.Object);

        _storageClientMock.Setup(c => c.ListFilesForTransferAsync(
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId, null, null))
            .ReturnsAsync(filesForTransfer);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        Assert.False(filesResult.IsInvalid);
        Assert.True(filesResult.ValidationErrors == null || !filesResult.ValidationErrors.Any());
    }

    [Fact]
    public async Task Run_MoveFrom_EgressToNetApp_WithUserPermissionCheck_ReturnsEgressPermissionExceptionResult_WhenUserLacksPermission()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = true,
            Value = CreateRequest(TransferDirection.EgressToNetApp, "valid/path/", TransferType.Move)
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        _egressClientMock.Setup(c => c.GetWorkspacePermission(It.IsAny<GetWorkspacePermissionArg>()))
            .ReturnsAsync(false);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        Assert.IsType<EgressPermissionExceptionResult>(result);
    }


    [Fact]
    public async Task Run_MoveFrom_EgressToNetApp_WithUserPermissionCheck_DoesNotReturnEgressPermissionExceptionResult_WhenUserHasPermission()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;
        var relativePath = "file1.txt";

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = true,
            Value = CreateRequest(TransferDirection.EgressToNetApp, "valid/path/", TransferType.Move)
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        _egressClientMock.Setup(c => c.GetWorkspacePermission(It.IsAny<GetWorkspacePermissionArg>()))
            .ReturnsAsync(true);

        var filesForTransfer = new List<FileTransferInfo>
        {
            new () {
                SourcePath = $"source/{relativePath}",
                RelativePath = relativePath
            }
        };

        _storageClientFactoryMock.Setup(f => f.GetSourceClientForDirection(TransferDirection.EgressToNetApp))
            .Returns(_storageClientMock.Object);

        _storageClientMock.Setup(c => c.ListFilesForTransferAsync(
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId, null, null))
            .ReturnsAsync(filesForTransfer);

        // Act
        var result = await Record.ExceptionAsync(() => _function.Run(reqMock.Object, context));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Run_WithEmptySourcePaths_HandlesGracefully()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = true,
            Value = CreateRequest(TransferDirection.EgressToNetApp, "valid/path/"),
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        _storageClientFactoryMock.Setup(f => f.GetSourceClientForDirection(TransferDirection.EgressToNetApp))
            .Returns(_storageClientMock.Object);

        _storageClientMock.Setup(c => c.ListFilesForTransferAsync(
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId, null, null))
            .ReturnsAsync(new List<FileTransferInfo>());

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        Assert.Empty(filesResult.Files);
    }

    [Fact]
    public async Task Run_MapsTransferEntityDtoCorrectly()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;

        var validationResult = new ValidatableRequest<ListFilesForTransferRequest>
        {
            IsValid = true,
            Value = CreateRequest(TransferDirection.EgressToNetApp),
        };

        _requestValidatorMock.Setup(v => v.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(validationResult);

        var sourcePaths = new List<SelectedSourcePath>
        {
            new () { Path = "source/file1.txt", FileId = "file1", IsFolder = false },
            new () { Path = "source/folder1/", FileId = "folder1", IsFolder = true }
        };

        validationResult.Value.SourcePaths = sourcePaths;

        var filesForTransfer = new List<FileTransferInfo>
        {
            new () {
                Id = "file1",
                SourcePath = "source/file1.txt",
                RelativePath = "file1.txt"
             },
            new () {
                Id = "folder1",
                SourcePath = "source/folder1/file2.txt",
                RelativePath = "folder1/file2.txt"
             }
        };
        _storageClientFactoryMock.Setup(f => f.GetSourceClientForDirection(TransferDirection.EgressToNetApp))
            .Returns(_storageClientMock.Object);

        _storageClientMock.Setup(c => c.ListFilesForTransferAsync(
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId, null, null))
            .ReturnsAsync(filesForTransfer);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        Assert.Equal(2, filesResult.Files.Count());
        Assert.Equal(_testCaseId, filesResult.CaseId);
        Assert.Equal(_testWorkspaceId, filesResult.WorkspaceId);
        Assert.Equal(TransferDirection.EgressToNetApp.ToString(), filesResult.TransferDirection);
        Assert.False(filesResult.IsInvalid);
        Assert.Single(filesResult.Files, f => f.Id == "file1" && f.SourcePath == "source/file1.txt" && f.RelativePath == "file1.txt");
        Assert.Single(filesResult.Files, f => f.Id == "folder1" && f.SourcePath == "source/folder1/file2.txt" && f.RelativePath == "folder1/file2.txt");
    }

    private ListFilesForTransferRequest CreateRequest(TransferDirection transferDirection, string destinationPath = "valid/path/", TransferType transferType = TransferType.Copy)
    {
        return new ListFilesForTransferRequest
        {
            CaseId = _testCaseId,
            CorrelationId = _testCorrelationId,
            TransferDirection = transferDirection,
            TransferType = transferType,
            DestinationPath = destinationPath,
            WorkspaceId = _testWorkspaceId,
            SourcePaths = _fixture.Create<List<SelectedSourcePath>>()
        };
    }
}

