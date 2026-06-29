using System.Net;
using CPS.ComplexCases.Common.Handlers;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.Common.Tests.Unit;

public class HttpResponseHandlerTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly HttpResponseHandler _handler = new();
    private const string TestUrl = "https://external.api/resource";

    [Fact]
    public async Task SendAsync_ReturnsResponse_WhenStatusCodeIsSuccess()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        var config = new HttpResponseHandlerConfig
        {
            DefaultExceptionFactory = (_, ex) => ex
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var result = await _handler.SendAsync(httpClient, request, config);

        Assert.Same(expectedResponse, result);
    }

    [Fact]
    public async Task SendAsync_ReturnsResponse_WhenStatusCodeIsExpectedUnhappy()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        var config = new HttpResponseHandlerConfig
        {
            DefaultExceptionFactory = (_, ex) => ex
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        var result = await _handler.SendAsync(httpClient, request, config, HttpStatusCode.NotFound);

        Assert.Same(expectedResponse, result);
    }

    [Fact]
    public async Task SendAsync_ThrowsMappedException_WhenStatusCodeFactoryExists()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        var expectedException = new InvalidOperationException("mapped exception");

        var config = new HttpResponseHandlerConfig
        {
            StatusCodeExceptionFactories = new Dictionary<HttpStatusCode, Func<HttpResponseMessage, Exception>>
            {
                [HttpStatusCode.Conflict] = _ => expectedException
            },
            DefaultExceptionFactory = (_, ex) => ex
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.SendAsync(httpClient, request, config));

        Assert.Same(expectedException, exception);
    }

    [Fact]
    public async Task SendAsync_ThrowsDefaultException_WhenStatusCodeFactoryDoesNotExist()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        var responseContent = "failure from api";

        HttpStatusCode? capturedStatusCode = null;
        HttpRequestException? capturedHttpRequestException = null;

        var config = new HttpResponseHandlerConfig
        {
            DefaultExceptionFactory = (statusCode, httpRequestException) =>
            {
                capturedStatusCode = statusCode;
                capturedHttpRequestException = httpRequestException;
                return new ApplicationException("default exception", httpRequestException);
            }
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(responseContent)
            });

        var exception = await Assert.ThrowsAsync<ApplicationException>(() => _handler.SendAsync(httpClient, request, config));

        Assert.Equal("default exception", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, capturedStatusCode);
        Assert.NotNull(capturedHttpRequestException);
        Assert.Equal(responseContent, capturedHttpRequestException.Message);
        Assert.Same(capturedHttpRequestException, exception.InnerException);
    }
}