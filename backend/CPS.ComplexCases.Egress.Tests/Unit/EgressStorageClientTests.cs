using System.Net;
using System.Text;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.Egress.Tests.Unit;

public class EgressStorageClientTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<EgressStorageClient>> _loggerMock;
    private readonly Mock<IOptions<EgressOptions>> _optionsMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IEgressRequestFactory> _requestFactoryMock;
    private readonly Mock<ITelemetryClient> _telemtryClientMock;
    private readonly EgressStorageClient _client;
    private const string TestUrl = "https://example.com";

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public EgressStorageClientTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<EgressStorageClient>>>();
        _optionsMock = _fixture.Freeze<Mock<IOptions<EgressOptions>>>();
        _requestFactoryMock = _fixture.Freeze<Mock<IEgressRequestFactory>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _telemtryClientMock = _fixture.Freeze<Mock<ITelemetryClient>>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(TestUrl)
        };

        var egressOptions = new EgressOptions
        {
            Url = TestUrl,
            Username = _fixture.Create<string>(),
            Password = _fixture.Create<string>()
        };
        _optionsMock.Setup(o => o.Value).Returns(egressOptions);

        _client = new EgressStorageClient(
            _loggerMock.Object,
            _optionsMock.Object,
            _httpClient,
            _requestFactoryMock.Object,
            _telemtryClientMock.Object
        );
    }

    [Fact]
    public async Task OpenReadStreamAsync_WithValidParameters_ReturnsStreamAndContentLength()
    {
        // Arrange
        var path = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var fileId = _fixture.Create<string>();
        var token = _fixture.Create<string>();
        var fileContent = "Test file content";
        var expectedContentLength = Encoding.UTF8.GetByteCount(fileContent);

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        SetupTokenRequest(token);
        SetupDocumentRequest(workspaceId, fileId, token);
        SetupHttpMockResponsesWithContentLength(
                ("token", tokenResponse, null),
                ("document", fileContent, expectedContentLength)
            );

        // Act
        var (stream, contentLength) = await _client.OpenReadStreamAsync(path, workspaceId, fileId);

        // Assert
        await using (stream)
        {
            Assert.NotNull(stream);
            Assert.Equal(expectedContentLength, contentLength);

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            Assert.Equal(fileContent, content);
        }

        VerifyTokenRequest();
        VerifyDocumentRequest(workspaceId, fileId, token);
    }

    [Fact]
    public async Task OpenReadStreamAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        // Arrange
        var path = _fixture.Create<string>();
        var fileId = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupHttpMockResponses(("token", tokenResponse));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.OpenReadStreamAsync(path, null, fileId));

        Assert.Equal("workspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null.", exception.Message);
    }

    [Fact]
    public async Task OpenReadStreamAsync_WithNullFileId_ThrowsArgumentNullException()
    {
        // Arrange
        var path = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupHttpMockResponses(("token", tokenResponse));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.OpenReadStreamAsync(path, workspaceId, null));

        Assert.Equal("fileId", exception.ParamName);
        Assert.Contains("File ID cannot be null.", exception.Message);
    }

    [Fact]
    public async Task UploadChunkAsync_WithValidParameters_ReturnsUploadChunkResult()
    {
        // Arrange
        var uploadId = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var session = new UploadSession
        {
            UploadId = uploadId,
            WorkspaceId = workspaceId,
            Md5Hash = _fixture.Create<string>()
        };
        var chunkNumber = _fixture.Create<int>();
        var chunkData = Encoding.UTF8.GetBytes("test chunk data");
        var start = _fixture.Create<long>();
        var end = start + chunkData.Length - 1;
        var totalSize = _fixture.Create<long>();
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupUploadChunkRequest(uploadId, workspaceId, start, end, totalSize, chunkData, token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("chunk", new { Success = true })
        );

        // Act
        var result = await _client.UploadChunkAsync(session, chunkNumber, chunkData, start, end, totalSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TransferDirection.NetAppToEgress, result.TransferDirection);

        VerifyTokenRequest();
        VerifyUploadChunkRequest(uploadId, workspaceId, start, end, totalSize, chunkData, token);
    }

    [Fact]
    public async Task UploadChunkAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        // Arrange
        var session = new UploadSession
        {
            UploadId = _fixture.Create<string>(),
            WorkspaceId = null
        };
        var chunkNumber = _fixture.Create<int>();
        var chunkData = new byte[100];
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupHttpMockResponses(("token", tokenResponse));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.UploadChunkAsync(session, chunkNumber, chunkData));

        Assert.Equal("WorkspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null.", exception.Message);
    }

    [Fact]
    public async Task CompleteUploadAsync_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        var uploadId = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var md5Hash = _fixture.Create<string>();
        var session = new UploadSession
        {
            UploadId = uploadId,
            WorkspaceId = workspaceId,
            Md5Hash = _fixture.Create<string>()
        };
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupCompleteUploadRequest(uploadId, workspaceId, md5Hash, token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("complete", new { Success = true })
        );

        // Act
        await _client.CompleteUploadAsync(session, md5Hash);

        // Assert
        VerifyTokenRequest();
        VerifyCompleteUploadRequest(uploadId, workspaceId, md5Hash, token);
    }

    [Fact]
    public async Task CompleteUploadAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        // Arrange
        var session = new UploadSession
        {
            UploadId = _fixture.Create<string>(),
            WorkspaceId = null
        };
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupHttpMockResponses(("token", tokenResponse));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.CompleteUploadAsync(session));

        Assert.Equal("WorkspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null.", exception.Message);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        // Arrange
        var selectedEntities = new List<TransferEntityDto>
        {
            new TransferEntityDto
            {
                Id = _fixture.Create<string>(),
                Path = _fixture.Create<string>(),
                IsFolder = false
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _client.ListFilesForTransferAsync(selectedEntities, null));

        Assert.Equal("workspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null or empty.", exception.Message);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WithFilesOnly_ReturnsFileTransferInfos()
    {
        // Arrange
        var workspaceId = _fixture.Create<string>();
        var token = _fixture.Create<string>();
        var file1 = new TransferEntityDto
        {
            Id = _fixture.Create<string>(),
            FileId = _fixture.Create<string>(),
            Path = "/path/to/file1.txt",
            IsFolder = false
        };
        var file2 = new TransferEntityDto
        {
            Id = _fixture.Create<string>(),
            FileId = _fixture.Create<string>(),
            Path = "/path/to/file2.txt",
            IsFolder = false
        };

        var selectedEntities = new List<TransferEntityDto> { file1, file2 };

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupHttpMockResponses(("token", tokenResponse));

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, workspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        var resultArray = result.ToArray();
        Assert.Equal(file1.FileId, resultArray[0].Id);
        Assert.Equal(Path.GetFileName(file1.Path), resultArray[0].SourcePath);
        Assert.Equal(file1.Path, resultArray[0].FullFilePath);
        Assert.Equal(file2.FileId, resultArray[1].Id);
        Assert.Equal(Path.GetFileName(file2.Path), resultArray[1].SourcePath);
        Assert.Equal(file2.Path, resultArray[1].FullFilePath);

        VerifyTokenRequest();
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WithSingleFolder_ReturnsAllFilesFromFolder()
    {
        // Arrange
        var workspaceId = _fixture.Create<string>();
        var folderId = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var folder = new TransferEntityDto
        {
            Id = folderId,
            FileId = folderId,
            Path = "/path/to/folder",
            IsFolder = true
        };

        var selectedEntities = new List<TransferEntityDto> { folder };

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var listResponse = new ListCaseMaterialResponse
        {
            Data = new List<ListCaseMaterialDataResponse>
            {
                new ListCaseMaterialDataResponse
                {
                    Id = _fixture.Create<string>(),
                    FileName = "file1.txt",
                    Path = "/path/to/folder/file1.txt",
                    IsFolder = false,
                    Version = 1
                },
                new ListCaseMaterialDataResponse
                {
                    Id = _fixture.Create<string>(),
                    FileName = "file2.txt",
                    Path = "/path/to/folder/file2.txt",
                    IsFolder = false,
                    Version = 1
                }
            },
            DataInfo = new DataInfoResponse
            {
                TotalResults = 2,
                NumReturned = 2,
                Limit = 100,
                Skip = 0
            }
        };

        SetupTokenRequest(token);
        SetupListMaterialRequestWithFolderId(workspaceId, folderId, token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("list", listResponse)
        );

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, workspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        var resultArray = result.ToArray();
        foreach (var file in resultArray)
        {
            Assert.StartsWith("folder/", file.SourcePath);
        }
        Assert.Contains(resultArray, file => file.SourcePath.EndsWith("file1.txt"));
        Assert.Contains(resultArray, file => file.SourcePath.EndsWith("file2.txt"));

        VerifyTokenRequest();
        VerifyListMaterialRequestWithFolderId(workspaceId, folderId, token);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WithFolderContainingSubfolders_ReturnsAllFilesRecursively()
    {
        // Arrange
        var workspaceId = _fixture.Create<string>();
        var parentFolderId = _fixture.Create<string>();
        var subFolderId = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var folder = new TransferEntityDto
        {
            Id = parentFolderId,
            FileId = parentFolderId,
            Path = "/parent",
            IsFolder = true
        };

        var selectedEntities = new List<TransferEntityDto> { folder };

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        // Parent folder contains 1 file and 1 subfolder
        var parentListResponse = new ListCaseMaterialResponse
        {
            Data = new List<ListCaseMaterialDataResponse>
            {
                new ListCaseMaterialDataResponse
                {
                    Id = _fixture.Create<string>(),
                    FileName = "parent-file.txt",
                    Path = "/parent",
                    IsFolder = false,
                    Version = 1
                },
                new ListCaseMaterialDataResponse
                {
                    Id = subFolderId,
                    FileName = "subfolder",
                    Path = "/parent/subfolder",
                    IsFolder = true,
                    Version = 1
                }
            },
            DataInfo = new DataInfoResponse
            {
                TotalResults = 2,
                NumReturned = 2,
                Limit = 100,
                Skip = 0
            }
        };

        // Subfolder contains 2 files
        var subListResponse = new ListCaseMaterialResponse
        {
            Data = new List<ListCaseMaterialDataResponse>
            {
                new ListCaseMaterialDataResponse
                {
                    Id = _fixture.Create<string>(),
                    FileName = "sub-file1.txt",
                    Path = "/parent/subfolder",
                    IsFolder = false,
                    Version = 1
                },
                new ListCaseMaterialDataResponse
                {
                    Id = _fixture.Create<string>(),
                    FileName = "sub-file2.txt",
                    Path = "/parent/subfolder",
                    IsFolder = false,
                    Version = 1
                }
            },
            DataInfo = new DataInfoResponse
            {
                TotalResults = 2,
                NumReturned = 2,
                Limit = 100,
                Skip = 0
            }
        };

        SetupTokenRequest(token);
        SetupListMaterialRequestWithFolderId(workspaceId, parentFolderId, token);
        SetupListMaterialRequestWithFolderId(workspaceId, subFolderId, token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("parentList", parentListResponse),
            ("subList", subListResponse)
        );

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, workspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());

        var resultArray = result.ToArray();
        Assert.Contains(resultArray, file => file.SourcePath == "parent/parent-file.txt");
        Assert.Contains(resultArray, file => file.SourcePath == "parent/subfolder/sub-file1.txt");
        Assert.Contains(resultArray, file => file.SourcePath == "parent/subfolder/sub-file2.txt");

        VerifyTokenRequest();
        VerifyListMaterialRequestWithFolderId(workspaceId, parentFolderId, token);
        VerifyListMaterialRequestWithFolderId(workspaceId, subFolderId, token);
    }

    [Fact]
    public async Task InitiateUploadAsync_WhenFolderCreationFails_ThrowsHttpRequestException()
    {
        // Arrange
        var destinationPath = "uploads/documents";
        var fileSize = 1024L;
        var sourcePath = "test.txt";
        var workspaceId = _fixture.Create<string>();
        var relativePath = "subfolder/test.txt";
        var token = _fixture.Create<string>();

        var fileName = Path.GetFileName(relativePath);
        var sourceDirectory = Path.GetDirectoryName(relativePath) ?? string.Empty;
        var fullDestinationPath = Path.Combine(destinationPath, sourceDirectory).Replace('\\', '/');

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupCreateUploadRequest(fullDestinationPath, fileSize, workspaceId, fileName, token);
        SetupCreateFolderRequest(workspaceId, token);

        // Upload fails with 404, folder creation fails with 500
        SetupHttpMockResponsesWithStatus(
            ("token", tokenResponse, HttpStatusCode.OK),
            ("uploadFail", "Not Found", HttpStatusCode.NotFound),
            ("folderCreateFail", "Internal Server Error", HttpStatusCode.InternalServerError)
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.InitiateUploadAsync(destinationPath, fileSize, sourcePath, workspaceId, relativePath));

        Assert.Contains("500", exception.Message);

        VerifyTokenRequest();
        VerifyCreateUploadRequest(fullDestinationPath, fileSize, workspaceId, fileName, token);
        VerifyCreateFolderRequest(workspaceId, token);
    }

    [Fact]
    public async Task InitiateUploadAsync_WhenFolderCreationSucceedsButRetryUploadFails_ThrowsHttpRequestException()
    {
        // Arrange
        var destinationPath = "uploads/documents";
        var fileSize = 1024L;
        var sourcePath = "test.txt";
        var workspaceId = _fixture.Create<string>();
        var relativePath = "subfolder/test.txt";
        var token = _fixture.Create<string>();

        var fileName = Path.GetFileName(relativePath);
        var sourceDirectory = Path.GetDirectoryName(relativePath) ?? string.Empty;
        var fullDestinationPath = Path.Combine(destinationPath, sourceDirectory).Replace('\\', '/');

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupCreateUploadRequest(fullDestinationPath, fileSize, workspaceId, fileName, token);
        SetupCreateFolderRequest(workspaceId, token);

        // Upload fails with 404, folder creation succeeds, retry upload fails with different error
        SetupHttpMockResponsesWithStatus(
            ("token", tokenResponse, HttpStatusCode.OK),
            ("uploadFail", "Not Found", HttpStatusCode.NotFound),
            ("folderCreate", new { Success = true }, HttpStatusCode.OK),
            ("uploadRetryFail", "Forbidden", HttpStatusCode.Forbidden)
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.InitiateUploadAsync(destinationPath, fileSize, sourcePath, workspaceId, relativePath));

        Assert.Contains("403", exception.Message);

        VerifyTokenRequest();
        VerifyCreateUploadRequest(fullDestinationPath, fileSize, workspaceId, fileName, token);
        VerifyCreateFolderRequest(workspaceId, token);
    }

    [Fact]
    public async Task InitiateUploadAsync_WhenInitialUploadFailsWithNon404Error_DoesNotAttemptFolderCreation()
    {
        // Arrange
        var destinationPath = "uploads/documents";
        var fileSize = 1024L;
        var sourcePath = "test.txt";
        var workspaceId = _fixture.Create<string>();
        var relativePath = "subfolder/test.txt";
        var token = _fixture.Create<string>();


        var fileName = Path.GetFileName(relativePath);
        var sourceDirectory = Path.GetDirectoryName(relativePath) ?? string.Empty;
        var fullDestinationPath = Path.Combine(destinationPath, sourceDirectory).Replace('\\', '/');

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupCreateUploadRequest(fullDestinationPath, fileSize, workspaceId, fileName, token);

        // Upload fails with 403 (not 404)
        SetupHttpMockResponsesWithStatus(
            ("token", tokenResponse, HttpStatusCode.OK),
            ("uploadFail", "Forbidden", HttpStatusCode.Forbidden)
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.InitiateUploadAsync(destinationPath, fileSize, sourcePath, workspaceId, relativePath));

        Assert.Contains("403", exception.Message);

        VerifyTokenRequest();
        VerifyCreateUploadRequest(fullDestinationPath, fileSize, workspaceId, fileName, token);

        // Verify folder creation was NOT attempted
        _requestFactoryMock.Verify(
            f => f.CreateFolderRequest(It.IsAny<CreateFolderArg>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData("root/folder/file.txt", "root/", "folder/file.txt")]
    [InlineData("root\\folder\\file.txt", "root\\", "folder\\file.txt")]
    [InlineData("root/folder/file.txt", "root/folder/", "file.txt")]
    [InlineData("root/folder/file.txt", "root/folder", "file.txt")]
    [InlineData("folder/file.txt", "folder/", "file.txt")]
    [InlineData("folder/file.txt", null, "folder/file.txt")]
    [InlineData("folder/file.txt", "", "folder/file.txt")]
    [InlineData("root/folder/file.txt", "notmatching/", "root/folder/file.txt")]
    [InlineData("/root/folder/file.txt", "/root/", "folder/file.txt")]
    [InlineData("root/folder/file.txt", "root", "folder/file.txt")]
    public void GetRelativePathFromSourceRoot_ReturnsExpected(string relativePath, string? sourceRootFolderPath, string expected)
    {
        var result = EgressStorageClient.GetRelativePathFromSourceRoot(relativePath, sourceRootFolderPath);
        Assert.Equal(expected, result);
    }

    #region Setup Methods

    private void SetupTokenRequest(string token)
    {
        _requestFactoryMock
            .Setup(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(() => new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/auth"));
    }

    private void SetupDocumentRequest(string workspaceId, string fileId, string token)
    {
        _requestFactoryMock
            .Setup(f => f.GetWorkspaceDocumentRequest(
                It.Is<GetWorkspaceDocumentArg>(arg =>
                    arg.WorkspaceId == workspaceId && arg.FileId == fileId),
                token))
            .Returns(() => new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/documents/{fileId}"));
    }

    private void SetupUploadChunkRequest(string uploadId, string workspaceId, long start, long end, long totalSize, byte[] chunkData, string token)
    {
        _requestFactoryMock
            .Setup(f => f.UploadChunkRequest(
                It.Is<UploadChunkArg>(arg =>
                    arg.UploadId == uploadId &&
                    arg.WorkspaceId == workspaceId &&
                    arg.Start == start &&
                    arg.End == end &&
                    arg.TotalSize == totalSize &&
                    arg.ChunkData == chunkData),
                token))
            .Returns(() => new HttpRequestMessage(HttpMethod.Put, $"{TestUrl}/api/v1/uploads/{uploadId}/chunks"));
    }

    private void SetupCompleteUploadRequest(string uploadId, string workspaceId, string md5Hash, string token)
    {
        _requestFactoryMock
            .Setup(f => f.CompleteUploadRequest(
                It.Is<CompleteUploadArg>(arg =>
                    arg.UploadId == uploadId &&
                    arg.WorkspaceId == workspaceId &&
                    arg.Md5Hash == md5Hash),
                token))
            .Returns(() => new HttpRequestMessage(HttpMethod.Post, $"{TestUrl}/api/v1/uploads/{uploadId}/complete"));
    }

    private void SetupListMaterialRequestWithFolderId(string workspaceId, string folderId, string token)
    {
        _requestFactoryMock
            .Setup(f => f.ListEgressMaterialRequest(
                It.Is<ListWorkspaceMaterialArg>(arg =>
                    arg.WorkspaceId == workspaceId &&
                    arg.FolderId == folderId &&
                    arg.Take == 100 &&
                    arg.Skip == 0 &&
                    arg.RecurseSubFolders == false),
                token))
            .Returns(() => new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/materials"));
    }

    private void SetupCreateFolderRequest(string workspaceId, string token)
    {
        _requestFactoryMock
            .Setup(f => f.CreateFolderRequest(It.IsAny<CreateFolderArg>(), token))
            .Returns(() => new HttpRequestMessage(HttpMethod.Post, $"{TestUrl}/api/v1/workspaces/{workspaceId}/folders"));
    }

    private void SetupCreateUploadRequest(string destinationPath, long fileSize, string workspaceId, string fileName, string token)
    {
        _requestFactoryMock
            .Setup(f => f.CreateUploadRequest(
                It.Is<CreateUploadArg>(arg =>
                    arg.FolderPath == destinationPath &&
                    arg.FileSize == fileSize &&
                    arg.WorkspaceId == workspaceId &&
                    arg.FileName == fileName),
                token))
            .Returns(() => new HttpRequestMessage(HttpMethod.Post, $"{TestUrl}/api/v1/uploads"));
    }

    #endregion

    #region Verify Methods

    private void VerifyTokenRequest()
    {
        _requestFactoryMock.Verify(
            f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    private void VerifyDocumentRequest(string workspaceId, string fileId, string token)
    {
        _requestFactoryMock.Verify(
            f => f.GetWorkspaceDocumentRequest(
                It.Is<GetWorkspaceDocumentArg>(arg =>
                    arg.WorkspaceId == workspaceId && arg.FileId == fileId),
                token),
            Times.Once);
    }

    private void VerifyUploadChunkRequest(string uploadId, string workspaceId, long start, long end, long totalSize, byte[] chunkData, string token)
    {
        _requestFactoryMock.Verify(
            f => f.UploadChunkRequest(
                It.Is<UploadChunkArg>(arg =>
                    arg.UploadId == uploadId &&
                    arg.WorkspaceId == workspaceId &&
                    arg.Start == start &&
                    arg.End == end &&
                    arg.TotalSize == totalSize &&
                    arg.ChunkData == chunkData),
                token),
            Times.Once);
    }

    private void VerifyCompleteUploadRequest(string uploadId, string workspaceId, string md5Hash, string token)
    {
        _requestFactoryMock.Verify(
            f => f.CompleteUploadRequest(
                It.Is<CompleteUploadArg>(arg =>
                    arg.UploadId == uploadId &&
                    arg.WorkspaceId == workspaceId &&
                    arg.Md5Hash == md5Hash),
                token),
            Times.Once);
    }

    private void VerifyListMaterialRequestWithFolderId(string workspaceId, string folderId, string token)
    {
        _requestFactoryMock.Verify(
            f => f.ListEgressMaterialRequest(
                It.Is<ListWorkspaceMaterialArg>(arg =>
                    arg.WorkspaceId == workspaceId &&
                    arg.FolderId == folderId &&
                    arg.RecurseSubFolders == false),
                token),
            Times.Once);
    }

    private void VerifyCreateFolderRequest(string workspaceId, string token)
    {
        _requestFactoryMock.Verify(
            f => f.CreateFolderRequest(It.IsAny<CreateFolderArg>(), token),
            Times.AtLeastOnce);
    }

    private void VerifyCreateUploadRequest(string destinationPath, long fileSize, string workspaceId, string fileName, string token)
    {
        _requestFactoryMock.Verify(
            f => f.CreateUploadRequest(
                It.Is<CreateUploadArg>(arg =>
                    arg.FolderPath == destinationPath &&
                    arg.FileSize == fileSize &&
                    arg.WorkspaceId == workspaceId &&
                    arg.FileName == fileName),
                token),
            Times.Once);
    }

    #endregion

    private void SetupHttpMockResponses(params (string type, object response, bool isStream)[] responses)
    {
        var sequence = _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        foreach (var (_, response, isStream) in responses)
        {
            HttpContent content;
            if (isStream && response is string stringContent)
            {
                content = new StringContent(stringContent);
            }
            else
            {
                var jsonContent = JsonSerializer.Serialize(response);
                content = new StringContent(jsonContent);
            }

            sequence = sequence.ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            });
        }
    }

    private void SetupHttpMockResponsesWithContentLength(params (string type, object response, long? contentLength)[] responses)
    {
        var sequence = _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        foreach (var (_, response, contentLength) in responses)
        {
            HttpContent content;

            if (response is string stringContent)
            {
                // For stream responses, use StreamContent
                var bytes = Encoding.UTF8.GetBytes(stringContent);
                var stream = new MemoryStream(bytes);
                content = new StreamContent(stream);

                // Set Content-Length header
                if (contentLength.HasValue)
                {
                    content.Headers.ContentLength = contentLength.Value;
                }
                else
                {
                    content.Headers.ContentLength = bytes.Length;
                }
            }
            else
            {
                // For JSON responses
                var jsonContent = JsonSerializer.Serialize(response);
                content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            sequence = sequence.ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            });
        }
    }

    private void SetupHttpMockResponses(params (string type, object response)[] responses)
    {
        var responsesWithStream = responses.Select(r => (r.type, r.response, false)).ToArray();
        SetupHttpMockResponses(responsesWithStream);
    }

    private void SetupHttpMockResponsesWithStatus(params (string type, object response, HttpStatusCode statusCode)[] responses)
    {
        var sequence = _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        foreach (var (_, response, statusCode) in responses)
        {
            HttpContent content;

            if (response is string stringContent)
            {
                content = new StringContent(stringContent);
            }
            else
            {
                var jsonContent = JsonSerializer.Serialize(response);
                content = new StringContent(jsonContent);
            }

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content
            };

            sequence = sequence.ReturnsAsync(httpResponse);
        }
    }
}