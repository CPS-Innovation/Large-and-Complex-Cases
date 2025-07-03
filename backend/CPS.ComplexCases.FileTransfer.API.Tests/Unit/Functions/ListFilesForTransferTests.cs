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
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using CPS.ComplexCases.FileTransfer.API.Validators;
using FluentAssertions;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class ListFilesForTransferTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<ListFilesForTransfer>> _loggerMock;
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock;
    private readonly Mock<IStorageClient> _storageClientMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
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

        _testWorkspaceId = _fixture.Create<string>();
        _testCaseId = _fixture.Create<int>();
        _testCorrelationId = _fixture.Create<Guid>();

        _function = new ListFilesForTransfer(_loggerMock.Object, _storageClientFactoryMock.Object, _requestValidatorMock.Object);
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
        badRequest.Value.Should().BeEquivalentTo(validationResult.ValidationErrors);
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
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId))
            .ReturnsAsync(filesForTransfer);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        filesResult.Should().NotBeNull();
        filesResult.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task Run_EgressToNetApp_WithInvalidPaths_SetsIsInvalidCorrectly()
    {
        // Arrange
        var reqMock = new Mock<HttpRequest>();
        var context = new Mock<FunctionContext>().Object;
        var destinationPath = new string('a', 256);
        var sourcePath = "file1.txt";
        var expectedErrorMessage = $"{destinationPath}/{sourcePath}: exceeds the 255 characters limit.";

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
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId))
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
        filesResult.IsInvalid.Should().BeTrue();

        filesResult.ValidationErrors.Should().NotBeNull();
        var error = filesResult.ValidationErrors.First();
        error.Should().NotBeNull();
        error.ErrorType.Should().Be(TransferFailedType.PathLengthExceeded);
        error.Message.Should().Be(expectedErrorMessage);
        error.SourcePath.Should().Be(sourcePath);
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
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId))
            .ReturnsAsync(filesForTransfer);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        filesResult.IsInvalid.Should().BeFalse();
        filesResult.ValidationErrors.Should().BeNullOrEmpty();
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
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId))
            .ReturnsAsync(new List<FileTransferInfo>());

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        filesResult.Files.Should().BeEmpty();
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
            It.IsAny<List<TransferEntityDto>>(), _testWorkspaceId, _testCaseId))
            .ReturnsAsync(filesForTransfer);

        // Act
        var result = await _function.Run(reqMock.Object, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var filesResult = Assert.IsType<FilesForTransferResult>(okResult.Value);
        filesResult.Files.Should().HaveCount(2);
        filesResult.CaseId.Should().Be(_testCaseId);
        filesResult.WorkspaceId.Should().Be(_testWorkspaceId);
        filesResult.TransferDirection.Should().Be(TransferDirection.EgressToNetApp.ToString());
        filesResult.IsInvalid.Should().BeFalse();
        filesResult.Files.Should().ContainSingle(f => f.Id == "file1" && f.SourcePath == "source/file1.txt" && f.RelativePath == "file1.txt");
        filesResult.Files.Should().ContainSingle(f => f.Id == "folder1" && f.SourcePath == "source/folder1/file2.txt" && f.RelativePath == "folder1/file2.txt");
    }

    private ListFilesForTransferRequest CreateRequest(TransferDirection transferDirection, string destinationPath = "valid/path/")
    {
        return new ListFilesForTransferRequest
        {
            CaseId = _testCaseId,
            CorrelationId = _testCorrelationId,
            TransferDirection = transferDirection,
            DestinationPath = destinationPath,
            WorkspaceId = _testWorkspaceId,
            SourcePaths = _fixture.Create<List<SelectedSourcePath>>()
        };
    }
}

