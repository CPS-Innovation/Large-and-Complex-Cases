using System.Net;
using System.Text;
using System.Text.Json;
using AutoFixture;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.Common.Models.Requests;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.API.Tests.Unit.Clients
{
    public class FileTransferClientTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IRequestFactory> _requestFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly FileTransferClient _client;
        private readonly Guid _correlationId;

        public FileTransferClientTests()
        {
            _fixture = new Fixture();
            _requestFactoryMock = new Mock<IRequestFactory>(MockBehavior.Strict);
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://fakebase.com/")
            };

            _client = new FileTransferClient(_requestFactoryMock.Object, _httpClient);
            _correlationId = _fixture.Create<Guid>();
        }

        [Fact]
        public async Task ListFilesForTransferAsync_SendsCorrectRequest()
        {
            // Arrange
            var requestModel = _fixture.Create<ListFilesForTransferRequest>();
            var serializedContent = JsonSerializer.Serialize(requestModel);
            var expectedContent = new StringContent(serializedContent, Encoding.UTF8, ContentType.ApplicationJson);
            var expectedRequest = new HttpRequestMessage(HttpMethod.Post, "transfer/files")
            {
                Content = expectedContent
            };

            _requestFactoryMock
                .Setup(f => f.Create(HttpMethod.Post, "transfer/files", _correlationId,
                    It.Is<StringContent>(sc => ContentMatches(sc, serializedContent))))
                .Returns(expectedRequest);

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("transfer/files")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _client.ListFilesForTransferAsync(requestModel, _correlationId);

            // Assert
            Assert.Equal(expectedResponse, result);
            _requestFactoryMock.VerifyAll();
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task InitiateFileTransferAsync_SendsCorrectRequest()
        {
            // Arrange
            var requestModel = _fixture.Create<TransferRequest>();
            var serializedContent = JsonSerializer.Serialize(requestModel);
            var expectedContent = new StringContent(serializedContent, Encoding.UTF8, ContentType.ApplicationJson);
            var expectedRequest = new HttpRequestMessage(HttpMethod.Post, "transfer")
            {
                Content = expectedContent
            };

            _requestFactoryMock
                .Setup(f => f.Create(HttpMethod.Post, "transfer", _correlationId,
                    It.Is<StringContent>(sc => ContentMatches(sc, serializedContent))))
                .Returns(expectedRequest);

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("transfer")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _client.InitiateFileTransferAsync(requestModel, _correlationId);

            // Assert
            Assert.Equal(expectedResponse, result);
            _requestFactoryMock.VerifyAll();
        }

        [Fact]
        public async Task GetFileTransferStatusAsync_SendsCorrectRequest()
        {
            // Arrange
            var transferId = _fixture.Create<string>();
            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, $"transfer/{transferId}/status");

            _requestFactoryMock
                .Setup(f => f.Create(HttpMethod.Get, $"transfer/{transferId}/status", _correlationId, null))
                .Returns(expectedRequest);

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith($"transfer/{transferId}/status")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _client.GetFileTransferStatusAsync(transferId, _correlationId);

            // Assert
            Assert.Equal(expectedResponse, result);
            _requestFactoryMock.VerifyAll();
        }

        private static bool ContentMatches(StringContent content, string expectedContent)
        {
            var actualContent = content.ReadAsStringAsync().GetAwaiter().GetResult();
            return actualContent == expectedContent;
        }
    }
}