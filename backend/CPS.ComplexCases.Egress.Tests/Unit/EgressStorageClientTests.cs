using System.Net;
using System.Text;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.Egress.Tests.Unit;

public class EgressStorageClientTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<EgressStorageClient>> _loggerMock;
    private readonly Mock<IOptions<EgressOptions>> _optionsMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IEgressRequestFactory> _requestFactoryMock;
    private readonly EgressStorageClient _client;
    private const string TestUrl = "https://example.com";

    public EgressStorageClientTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<EgressStorageClient>>>();
        _optionsMock = _fixture.Freeze<Mock<IOptions<EgressOptions>>>();
        _requestFactoryMock = _fixture.Freeze<Mock<IEgressRequestFactory>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

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
            _requestFactoryMock.Object
        );
    }

    [Fact]
    public async Task OpenReadStreamAsync_WithValidParameters_ReturnsStream()
    {
        // Arrange
        var path = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var fileId = _fixture.Create<string>();
        var token = _fixture.Create<string>();
        var fileContent = "Test file content";

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupDocumentRequest(workspaceId, fileId, token);
        SetupHttpMockResponses(
            ("token", tokenResponse, false),
            ("document", fileContent, isStream: true)
        );

        // Act
        await using var result = await _client.OpenReadStreamAsync(path, workspaceId, fileId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();

            using var reader = new StreamReader(result);
            var content = await reader.ReadToEndAsync();
            content.Should().Be(fileContent);
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

        exception.ParamName.Should().Be("workspaceId");
        exception.Message.Should().Contain("Workspace ID cannot be null.");
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

        exception.ParamName.Should().Be("fileId");
        exception.Message.Should().Contain("File ID cannot be null.");
    }

    [Fact]
    public async Task InitiateUploadAsync_WithOverwritePolicyOverwrite_SkipsFileExistenceCheck()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var fileSize = _fixture.Create<long>();
        var workspaceId = _fixture.Create<string>();
        var sourcePath = "/path/to/testfile.txt";
        var token = _fixture.Create<string>();
        var uploadId = _fixture.Create<string>();
        var md5Hash = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var uploadResponse = new CreateUploadResponse
        {
            Id = uploadId,
            Md5Hash = md5Hash
        };

        SetupTokenRequest(token);
        SetupCreateUploadRequest(destinationPath, fileSize, workspaceId, "testfile.txt", token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("upload", uploadResponse)
        );

        // Act
        var result = await _client.InitiateUploadAsync(
            destinationPath,
            fileSize,
            workspaceId,
            sourcePath,
            TransferOverwritePolicy.Overwrite);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.UploadId.Should().Be(uploadId);
            result.WorkspaceId.Should().Be(workspaceId);
            result.Md5Hash.Should().Be(md5Hash);
        }

        VerifyTokenRequest();
        VerifyCreateUploadRequest(destinationPath, fileSize, workspaceId, "testfile.txt", token);
        _requestFactoryMock.Verify(
            f => f.ListEgressMaterialRequest(It.IsAny<ListWorkspaceMaterialArg>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task InitiateUploadAsync_WithoutOverwritePolicy_FileDoesNotExist_Succeeds()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var fileSize = _fixture.Create<long>();
        var workspaceId = _fixture.Create<string>();
        var sourcePath = "/path/to/testfile.txt";
        var token = _fixture.Create<string>();
        var uploadId = _fixture.Create<string>();
        var md5Hash = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var listResponse = new ListCaseMaterialResponse
        {
            Data = new List<ListCaseMaterialDataResponse>(),
            DataInfo = new DataInfoResponse()
        };
        var uploadResponse = new CreateUploadResponse
        {
            Id = uploadId,
            Md5Hash = md5Hash
        };

        SetupTokenRequest(token);
        SetupListMaterialRequest(workspaceId, destinationPath, token);
        SetupCreateUploadRequest(destinationPath, fileSize, workspaceId, "testfile.txt", token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("list", listResponse),
            ("upload", uploadResponse)
        );

        // Act
        var result = await _client.InitiateUploadAsync(destinationPath, fileSize, workspaceId, sourcePath);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.UploadId.Should().Be(uploadId);
            result.WorkspaceId.Should().Be(workspaceId);
            result.Md5Hash.Should().Be(md5Hash);
        }

        VerifyTokenRequest();
        VerifyListMaterialRequest(workspaceId, destinationPath, token);
        VerifyCreateUploadRequest(destinationPath, fileSize, workspaceId, "testfile.txt", token);
    }

    [Fact]
    public async Task InitiateUploadAsync_WithoutOverwritePolicy_FileExists_ThrowsFileExistsException()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var fileSize = _fixture.Create<long>();
        var workspaceId = _fixture.Create<string>();
        var sourcePath = "/path/to/testfile.txt";
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var listResponse = new ListCaseMaterialResponse
        {
            Data = new List<ListCaseMaterialDataResponse>
        {
            new ListCaseMaterialDataResponse
            {
                Id = _fixture.Create<string>(),
                FileName = "testfile.txt",
                Path = destinationPath,
                IsFolder = false,
                Version = 1
            },
            new ListCaseMaterialDataResponse
            {
                Id = _fixture.Create<string>(),
                FileName = "otherfile.txt",
                Path = destinationPath,
                IsFolder = false,
                Version = 1
            }
        },
            DataInfo = new DataInfoResponse()
        };

        SetupTokenRequest(token);
        SetupListMaterialRequest(workspaceId, destinationPath, token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("list", listResponse)
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileExistsException>(
            () => _client.InitiateUploadAsync(destinationPath, fileSize, workspaceId, sourcePath));

        exception.Message.Should().Contain("File 'testfile.txt' already exists in the destination path");
        exception.Message.Should().Contain(destinationPath);

        VerifyTokenRequest();
        VerifyListMaterialRequest(workspaceId, destinationPath, token);
        _requestFactoryMock.Verify(
            f => f.CreateUploadRequest(It.IsAny<CreateUploadArg>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task InitiateUploadAsync_WithoutOverwritePolicy_FileExistsCaseInsensitive_ThrowsFileExistsException()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var fileSize = _fixture.Create<long>();
        var workspaceId = _fixture.Create<string>();
        var sourcePath = "/path/to/TestFile.txt";
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var listResponse = new ListCaseMaterialResponse
        {
            Data = new List<ListCaseMaterialDataResponse>
        {
            new ListCaseMaterialDataResponse
            {
                Id = _fixture.Create<string>(),
                FileName = "testfile.txt",
                Path = destinationPath,
                IsFolder = false,
                Version = 1
            }
        },
            DataInfo = new DataInfoResponse()
        };

        SetupTokenRequest(token);
        SetupListMaterialRequest(workspaceId, destinationPath, token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("list", listResponse)
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileExistsException>(
            () => _client.InitiateUploadAsync(destinationPath, fileSize, workspaceId, sourcePath));

        exception.Message.Should().Contain("File 'TestFile.txt' already exists in the destination path");

        VerifyTokenRequest();
        VerifyListMaterialRequest(workspaceId, destinationPath, token);
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
        var contentRange = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        SetupTokenRequest(token);
        SetupUploadChunkRequest(uploadId, workspaceId, contentRange, chunkData, token);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("chunk", new { Success = true })
        );

        // Act
        var result = await _client.UploadChunkAsync(session, chunkNumber, chunkData, contentRange);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.TransferDirection.Should().Be(TransferDirection.NetAppToEgress);
        }

        VerifyTokenRequest();
        VerifyUploadChunkRequest(uploadId, workspaceId, contentRange, chunkData, token);
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

        exception.ParamName.Should().Be("WorkspaceId");
        exception.Message.Should().Contain("Workspace ID cannot be null.");
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

        exception.ParamName.Should().Be("WorkspaceId");
        exception.Message.Should().Contain("Workspace ID cannot be null.");
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

        exception.ParamName.Should().Be("workspaceId");
        exception.Message.Should().Contain("Workspace ID cannot be null or empty.");
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
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var resultArray = result.ToArray();
            resultArray[0].Id.Should().Be(file1.FileId);
            resultArray[0].SourcePath.Should().Be(file1.Path);
            resultArray[1].Id.Should().Be(file2.FileId);
            resultArray[1].SourcePath.Should().Be(file2.Path);
        }

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
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var resultArray = result.ToArray();
            resultArray.Should().AllSatisfy(file => file.SourcePath.Should().StartWith("/path/to/folder/"));
            resultArray.Should().Contain(file => file.SourcePath.EndsWith("file1.txt"));
            resultArray.Should().Contain(file => file.SourcePath.EndsWith("file2.txt"));
        }

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
                Path = "/parent/parent-file.txt",
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
                Path = "/parent/subfolder/sub-file1.txt",
                IsFolder = false,
                Version = 1
            },
            new ListCaseMaterialDataResponse
            {
                Id = _fixture.Create<string>(),
                FileName = "sub-file2.txt",
                Path = "/parent/subfolder/sub-file2.txt",
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
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            var resultArray = result.ToArray();
            resultArray.Should().Contain(file => file.SourcePath == "/parent/parent-file.txt");
            resultArray.Should().Contain(file => file.SourcePath == "/parent/subfolder/sub-file1.txt");
            resultArray.Should().Contain(file => file.SourcePath == "/parent/subfolder/sub-file2.txt");
        }

        VerifyTokenRequest();
        VerifyListMaterialRequestWithFolderId(workspaceId, parentFolderId, token);
        VerifyListMaterialRequestWithFolderId(workspaceId, subFolderId, token);
    }

    #region Setup Methods

    private void SetupTokenRequest(string token)
    {
        _requestFactoryMock
            .Setup(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/auth"));
    }

    private void SetupDocumentRequest(string workspaceId, string fileId, string token)
    {
        _requestFactoryMock
            .Setup(f => f.GetWorkspaceDocumentRequest(
                It.Is<GetWorkspaceDocumentArg>(arg =>
                    arg.WorkspaceId == workspaceId && arg.FileId == fileId),
                token))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/documents/{fileId}"));
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
            .Returns(new HttpRequestMessage(HttpMethod.Post, $"{TestUrl}/api/v1/uploads"));
    }

    private void SetupUploadChunkRequest(string uploadId, string workspaceId, string contentRange, byte[] chunkData, string token)
    {
        _requestFactoryMock
            .Setup(f => f.UploadChunkRequest(
                It.Is<UploadChunkArg>(arg =>
                    arg.UploadId == uploadId &&
                    arg.WorkspaceId == workspaceId &&
                    arg.ContentRange == contentRange &&
                    arg.ChunkData == chunkData),
                token))
            .Returns(new HttpRequestMessage(HttpMethod.Put, $"{TestUrl}/api/v1/uploads/{uploadId}/chunks"));
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
            .Returns(new HttpRequestMessage(HttpMethod.Post, $"{TestUrl}/api/v1/uploads/{uploadId}/complete"));
    }

    private void SetupListMaterialRequest(string workspaceId, string path, string token)
    {
        _requestFactoryMock
            .Setup(f => f.ListEgressMaterialRequest(
                It.Is<ListWorkspaceMaterialArg>(arg =>
                    arg.WorkspaceId == workspaceId && arg.Path == path),
                token))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/materials"));
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
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/materials"));
    }

    private void SetupListMaterialRequestWithFolderIdAndPaging(string workspaceId, string folderId, int skip, string token)
    {
        _requestFactoryMock
            .Setup(f => f.ListEgressMaterialRequest(
                It.Is<ListWorkspaceMaterialArg>(arg =>
                    arg.WorkspaceId == workspaceId &&
                    arg.FolderId == folderId &&
                    arg.Take == 100 &&
                    arg.Skip == skip &&
                    arg.RecurseSubFolders == false),
                token))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/materials"));
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

    private void VerifyUploadChunkRequest(string uploadId, string workspaceId, string contentRange, byte[] chunkData, string token)
    {
        _requestFactoryMock.Verify(
            f => f.UploadChunkRequest(
                It.Is<UploadChunkArg>(arg =>
                    arg.UploadId == uploadId &&
                    arg.WorkspaceId == workspaceId &&
                    arg.ContentRange == contentRange &&
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

    private void VerifyListMaterialRequest(string workspaceId, string path, string token)
    {
        _requestFactoryMock.Verify(
            f => f.ListEgressMaterialRequest(
                It.Is<ListWorkspaceMaterialArg>(arg =>
                    arg.WorkspaceId == workspaceId && arg.Path == path),
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

    private void VerifyListMaterialRequestWithFolderIdAndPaging(string workspaceId, string folderId, int skip, string token)
    {
        _requestFactoryMock.Verify(
            f => f.ListEgressMaterialRequest(
                It.Is<ListWorkspaceMaterialArg>(arg =>
                    arg.WorkspaceId == workspaceId &&
                    arg.FolderId == folderId &&
                    arg.Take == 100 &&
                    arg.Skip == skip &&
                    arg.RecurseSubFolders == false),
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

    private void SetupHttpMockResponses(params (string type, object response)[] responses)
    {
        var responsesWithStream = responses.Select(r => (r.type, r.response, false)).ToArray();
        SetupHttpMockResponses(responsesWithStream);
    }
}