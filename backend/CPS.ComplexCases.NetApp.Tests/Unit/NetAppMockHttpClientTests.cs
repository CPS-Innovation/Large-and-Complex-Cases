using System.Net;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.S3.Result;
using Moq;
using Moq.Protected;

public class NetAppMockHttpClientTests
{
    private readonly Mock<ILogger<NetAppMockHttpClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<INetAppMockHttpRequestFactory> _requestFactoryMock;
    private readonly NetAppMockHttpClient _client;

    public NetAppMockHttpClientTests()
    {
        _loggerMock = new Mock<ILogger<NetAppMockHttpClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _requestFactoryMock = new Mock<INetAppMockHttpRequestFactory>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        {
            httpClient.BaseAddress = new Uri("http://localhost/");
        }
        _client = new NetAppMockHttpClient(_loggerMock.Object, httpClient, _requestFactoryMock.Object);
    }

    [Fact]
    public async Task CreateBucketAsync_BucketAlreadyExists_ReturnsFalse()
    {
        // Arrange
        var bucketName = "existing-bucket";
        var createBucketArg = new CreateBucketArg { BucketName = bucketName };
        var findBucketArg = new FindBucketArg { BucketName = bucketName };
        var expectedExceptionMessage = $"A bucket with the name {bucketName} already exists.";

        _requestFactoryMock
            .Setup(f => f.FindBucketRequest(It.IsAny<FindBucketArg>()))
            .Returns(new HttpRequestMessage());
        _requestFactoryMock
            .Setup(f => f.ListBucketsRequest(It.IsAny<ListBucketsArg>()))
            .Returns(new HttpRequestMessage());
        _requestFactoryMock
            .Setup(f => f.CreateBucketRequest(It.IsAny<CreateBucketArg>()))
            .Returns(new HttpRequestMessage());

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\"?><ListAllMyBucketsResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"><Buckets><Bucket><Name>existing-bucket</Name></Bucket></Buckets></ListAllMyBucketsResult>", Encoding.UTF8, "application/xml")
            });

        // Act
        var result = await _client.CreateBucketAsync(createBucketArg);

        // Assert
        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
            It.Is<EventId>(eventId => eventId.Id == 0),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedExceptionMessage),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task ListFoldersInBucketAsync_ValidRequest_ReturnsFolders()
    {
        // Arrange
        var bucketName = "test-bucket";
        var listBucketResult = new ListBucketResult
        {
            CommonPrefixes = [new() { Prefix = "folder1/" }]
        };
        var serializedResponse = SerializeToXml(listBucketResult);

        _requestFactoryMock
            .Setup(f => f.ListFoldersInBucketRequest(It.IsAny<ListFoldersInBucketArg>()))
            .Returns(new HttpRequestMessage());

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(serializedResponse, Encoding.UTF8, "application/xml")
            });

        var arg = new ListFoldersInBucketArg { BucketName = bucketName };

        // Act
        var result = await _client.ListFoldersInBucketAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bucketName, result.Data.BucketName);
        Assert.Single(result.Data.FolderData, x => x.Path == "folder1/");
    }

    [Fact]
    public async Task SendRequestAsync_UnauthorizedResponse_ThrowsHttpRequestException()
    {
        // Arrange
        _requestFactoryMock
            .Setup(f => f.GetACLForBucketRequest(It.IsAny<string>()))
            .Returns(new HttpRequestMessage());

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetACLForBucketAsync("test-bucket"));
    }

    private static string SerializeToXml<T>(T obj)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, obj);
        return stringWriter.ToString();
    }
}