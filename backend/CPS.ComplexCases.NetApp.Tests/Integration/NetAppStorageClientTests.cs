using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.WireMock.Mappings;
using CPS.ComplexCases.NetApp.Wrappers;
using CPS.ComplexCases.WireMock.Core;
using Moq;
using WireMock.Server;
using WireMock.Settings;

namespace CPS.ComplexCases.NetApp.Tests.Integration;

public class NetAppStorageClientTests : IDisposable
{
    private bool _disposed = false;
    private readonly WireMockServer _server;
    private readonly NetAppClient _netAppClient;
    private readonly NetAppStorageClient _client;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<INetAppRequestFactory> _netAppRequestFactoryMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<IS3ClientFactory> _s3ClientFactoryMock;

    private const string BucketName = "test-bucket";
    private const string ObjectKey = "test-document.pdf";
    private const string UploadId = "upload-id-49e18525de9c";
    private const int CaseId = 2164817;
    private const string BearerToken = "fakeBearerToken";

    public NetAppStorageClientTests()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        _server = WireMockServer.Start(new WireMockServerSettings
        {
            UseSSL = true
        }).LoadMappings(
            new ObjectMapping(),
            new UploadMapping()
        );

        var _netAppOptions = Options.Create(new NetAppOptions
        {
            Url = _server.Urls[0].Replace("http://", "https://"),
            RegionName = "eu-west-2"
        });

        var credentials = new BasicAWSCredentials("fakeAccessKey", "fakeSecretKey");

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        // S3 client configured to accept self-signed certs (test only)
        var s3ClientConfig = new AmazonS3Config
        {
            ServiceURL = _server.Urls[0].Replace("http://", "https://"),
            ForcePathStyle = true,
            UseHttp = false,
            HttpClientFactory = new HttpClientFactoryWrapper(httpHandler)
        };


        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _netAppRequestFactoryMock = new Mock<INetAppRequestFactory>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _s3ClientFactoryMock = new Mock<IS3ClientFactory>();

        _s3ClientFactoryMock
            .Setup(f => f.GetS3ClientAsync(BearerToken))
            .ReturnsAsync(new AmazonS3Client(credentials, s3ClientConfig));

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NetAppClient>();

        _netAppClient = new NetAppClient(logger, new AmazonS3UtilsWrapper(), _netAppRequestFactoryMock.Object, _s3ClientFactoryMock.Object);
        _client = new NetAppStorageClient(_netAppClient, _netAppArgFactoryMock.Object, _caseMetadataServiceMock.Object);
    }

    [Fact]
    public async Task InitiateUploadAsync_ReturnsUploadSession()
    {
        const string destinationPath = "";
        const string sourcePath = ObjectKey;
        var fullPath = Path.Combine(destinationPath, sourcePath).Replace('\\', '/');

        var arg = new InitiateMultipartUploadArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = fullPath
        };

        var request = new InitiateMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = fullPath
        };

        _netAppArgFactoryMock.Setup(f => f.CreateInitiateMultipartUploadArg(BearerToken, BucketName, fullPath)).Returns(arg);
        _netAppRequestFactoryMock.Setup(f => f.CreateMultipartUploadRequest(arg)).Returns(request);

        var result = await _client.InitiateUploadAsync(destinationPath, 123, sourcePath, null, null, null, BearerToken, BucketName);

        Assert.NotNull(result);
        Assert.Equal(UploadId, result.UploadId);
        Assert.Equal(fullPath, result.WorkspaceId);
    }

    [Fact]
    public async Task UploadChunkAsync_ReturnsUploadChunkResult()
    {
        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var chunkData = new byte[] { 1, 2, 3 };

        var arg = new UploadPartArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            PartNumber = 2,
            PartData = chunkData,
            UploadId = UploadId
        };

        var request = new UploadPartRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            PartNumber = 2,
            UploadId = UploadId,
            InputStream = new MemoryStream(chunkData)
        };

        _netAppArgFactoryMock.Setup(f => f.CreateUploadPartArg(BearerToken, BucketName, ObjectKey, chunkData, 2, UploadId)).Returns(arg);
        _netAppRequestFactoryMock.Setup(c => c.UploadPartRequest(arg)).Returns(request);

        var result = await _client.UploadChunkAsync(session, 2, chunkData, null, null, null, BearerToken, BucketName);

        Assert.Equal("etag-12345", result.ETag);
        Assert.Equal(TransferDirection.EgressToNetApp, result.TransferDirection);
    }

    [Fact]
    public async Task CompleteUploadAsync_CallsClientWithCorrectArgs()
    {
        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var etags = new Dictionary<int, string> { [1] = "etag-12345" };

        var arg = new CompleteMultipartUploadArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            UploadId = UploadId,
            CompletedParts = []
        };

        var request = new CompleteMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            UploadId = UploadId,
            PartETags = []
        };

        _netAppArgFactoryMock.Setup(f => f.CreateCompleteMultipartUploadArg(BearerToken, BucketName, ObjectKey, UploadId, etags)).Returns(arg);
        _netAppRequestFactoryMock.Setup(c => c.CompleteMultipartUploadRequest(arg)).Returns(request);

        await _client.CompleteUploadAsync(session, null, etags, BearerToken, BucketName);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_ReturnsFileTransferInfo()
    {
        var folderName = "/nested-objects/";
        var maxKeys = 1000;

        var selectedEntities = new List<TransferEntityDto>
    {
        new() { Path = ObjectKey },
        new() { Path = folderName }
    };

        var arg = new ListObjectsInBucketArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ContinuationToken = null,
            MaxKeys = maxKeys.ToString(),
            Prefix = folderName
        };

        var request = new ListObjectsV2Request
        {
            BucketName = BucketName,
            ContinuationToken = null,
            MaxKeys = maxKeys,
            Prefix = folderName
        };

        var caseMetadata = new CaseMetadata
        {
            CaseId = CaseId,
            EgressWorkspaceId = "egress-workspace-id-123",
            NetappFolderPath = "test-folder/",
        };

        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(CaseId)).ReturnsAsync(caseMetadata);

        _netAppArgFactoryMock
            .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, maxKeys, folderName, false))
            .Returns(arg);

        _netAppRequestFactoryMock
            .Setup(f => f.ListObjectsInBucketRequest(arg))
            .Returns(request);

        _netAppArgFactoryMock
            .Setup(f => f.CreateListObjectsInBucketArg(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .Returns((string token, string bucket, string continuation, int max, string prefix, bool delimiter) =>
                new ListObjectsInBucketArg
                {
                    BearerToken = token,
                    BucketName = bucket,
                    ContinuationToken = continuation,
                    MaxKeys = max.ToString(),
                    Prefix = prefix
                });

        _netAppRequestFactoryMock
            .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
            .Returns((ListObjectsInBucketArg a) => new ListObjectsV2Request
            {
                BucketName = a.BucketName,
                ContinuationToken = a.ContinuationToken,
                MaxKeys = int.Parse(a.MaxKeys ?? "1000"),
                Prefix = a.Prefix
            });

        var result = await _client.ListFilesForTransferAsync(selectedEntities, null, CaseId, BearerToken, BucketName);

        Assert.NotNull(result);
        Assert.True(result.Any());
    }

    [Fact]
    public async Task OpenReadStreamAsync_ReturnsStream()
    {
        var arg = new GetObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var request = new GetObjectRequest
        {
            BucketName = BucketName,
            Key = ObjectKey
        };

        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BearerToken, BucketName, ObjectKey)).Returns(arg);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectRequest(arg)).Returns(request);

        var result = await _client.OpenReadStreamAsync(ObjectKey, null, null, BearerToken, BucketName);

        Assert.NotNull(result.Stream);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _server.Stop();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~NetAppStorageClientTests()
    {
        Dispose(false);
    }
}

public class HttpClientFactoryWrapper : Amazon.Runtime.HttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public HttpClientFactoryWrapper(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
        return new HttpClient(_handler, disposeHandler: false);
    }
}