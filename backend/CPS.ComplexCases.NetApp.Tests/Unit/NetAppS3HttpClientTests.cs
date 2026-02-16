using System.Net;
using Microsoft.Extensions.Options;
using AutoFixture;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Services;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.NetApp.Tests.Unit;

public class NetAppS3HttpClientTests
{
    private readonly Fixture _fixture;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IS3CredentialService> _s3CredentialServiceMock;
    private readonly Mock<IOptions<NetAppOptions>> _optionsMock;
    private readonly NetAppS3HttpClient _client;
    private readonly HttpClient _httpClient;
    private readonly string BearerToken;
    private readonly string BucketName;
    private readonly string ObjectKey;
    private readonly string AccessKey;
    private readonly string SecretKey;

    public NetAppS3HttpClientTests()
    {
        _fixture = new Fixture();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _s3CredentialServiceMock = new Mock<IS3CredentialService>();
        _optionsMock = new Mock<IOptions<NetAppOptions>>();

        BearerToken = _fixture.Create<string>();
        BucketName = _fixture.Create<string>();
        ObjectKey = _fixture.Create<string>();
        AccessKey = _fixture.Create<string>();
        SecretKey = _fixture.Create<string>();

        var options = new NetAppOptions
        {
            Url = "https://netapp.example.com/",
            RegionName = "eu-west-1",
        };

        _optionsMock.Setup(o => o.Value).Returns(options);

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://netapp.example.com/")
        };

        _client = new NetAppS3HttpClient(_httpClient, _s3CredentialServiceMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async Task GetHeadObjectAsync_ReturnsHeadObjectResponse_OnSuccess()
    {
        // Arrange
        var rawETag = "\"abc123\""; // ETag values are typically quoted
        var expectedETag = "abc123";

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Headers =
            {
                ETag = new System.Net.Http.Headers.EntityTagHeaderValue(rawETag)
            }
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(expectedETag, result.ETag);
    }

    [Fact]
    public async Task GetHeadObjectAsync_ReturnsEmptyETag_WhenETagIsNull()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(string.Empty, result.ETag);
    }

    [Fact]
    public async Task GetHeadObjectAsync_CallsCredentialService_WithBearerToken()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var responseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        await _client.GetHeadObjectAsync(arg);

        // Assert
        _s3CredentialServiceMock.Verify(
            s => s.GetCredentialKeysAsync(BearerToken),
            Times.Once);
    }

    [Fact]
    public async Task GetHeadObjectAsync_ThrowsInvalidOperationException_WhenBaseAddressNotConfigured()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        // BaseAddress is not set
        var client = new NetAppS3HttpClient(httpClient, _s3CredentialServiceMock.Object, _optionsMock.Object);

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetHeadObjectAsync(arg));
        Assert.Contains("BaseAddress", exception.Message);
    }

    [Fact]
    public async Task GetHeadObjectAsync_CreatesAuthorizationHeader_WithCorrectFormat()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.StartsWith("AWS4-HMAC-SHA256 Credential=", authHeader);
        Assert.Contains("SignedHeaders=", authHeader);
        Assert.Contains("Signature=", authHeader);
    }

    [Fact]
    public async Task GetHeadObjectAsync_IncludesRequiredHeaders_InRequest()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("x-amz-date"), "Missing x-amz-date header");
        Assert.True(capturedRequest.Headers.Contains("x-amz-content-sha256"), "Missing x-amz-content-sha256 header");
        Assert.True(capturedRequest.Headers.Contains("Authorization"), "Missing Authorization header");
    }

    [Fact]
    public async Task GetHeadObjectAsync_UsesHeadMethod_ForRequest()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Head, capturedRequest.Method);
    }

    [Fact]
    public async Task GetHeadObjectAsync_ConstructsCorrectRequestUri_WithBucketAndObjectKey()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.RequestUri);
        Assert.Contains($"{BucketName}/{ObjectKey}", capturedRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetHeadObjectAsync_Returns404StatusCode_WhenObjectNotFound()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetHeadObjectAsync_IncludesCredentialScope_InAuthorizationHeader()
    {
        // Arrange
        var regionName = "eu-west-1";

        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(capturedRequest);
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.Contains($"/{regionName}/s3/aws4_request", authHeader);
    }

    [Fact]
    public async Task GetHeadObjectAsync_IncludesSignedHeaders_InAuthorizationHeader()
    {
        // Arrange
        _s3CredentialServiceMock
            .Setup(s => s.GetCredentialKeysAsync(BearerToken))
            .ReturnsAsync((AccessKey, SecretKey));

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _client.GetHeadObjectAsync(arg);

        // Assert
        Assert.NotNull(capturedRequest);
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.Contains("SignedHeaders=host;x-amz-content-sha256;x-amz-date", authHeader);
    }
}
