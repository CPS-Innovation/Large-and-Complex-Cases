using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class InitTests
{
    private readonly Mock<ILogger<Init>> _loggerMock;
    private readonly Mock<IInitService> _initServiceMock;
    private readonly Fixture _fixture;
    private readonly Init _function;
    private readonly Guid _correlationId;
    private readonly string _cc;
    private readonly string _ct;
    private readonly string _redirectUrl;

    public InitTests()
    {
        _loggerMock = new Mock<ILogger<Init>>();
        _initServiceMock = new Mock<IInitService>();
        _fixture = new Fixture();
        _correlationId = _fixture.Create<Guid>();
        _cc = _fixture.Create<string>();
        _ct = _fixture.Create<string>();
        _redirectUrl = _fixture.Create<string>();

        _function = new Init(
            _loggerMock.Object,
            _initServiceMock.Object);
    }

    [Fact]
    public async Task Run_ReturnsRedirectResult_WhenInitServiceReturnsRedirectStatus()
    {
        // Arrange
        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = false
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(_redirectUrl, redirectResult.Url);
        Assert.False(redirectResult.Permanent);

        _initServiceMock.Verify(s => s.ProcessRequest(httpRequest, _correlationId, _cc), Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Init function processed a request with correlation ID: {_correlationId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_ReturnsRedirectResultWithCookie_WhenInitServiceReturnsRedirectWithCookieData()
    {
        // Arrange
        var cookiesMock = new Mock<IResponseCookies>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = true,
            Cc = _cc,
            Ct = _ct
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId, cookiesMock);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(_redirectUrl, redirectResult.Url);

        cookiesMock.Verify(
            c => c.Append(
                HttpHeaderKeys.CmsAuthValues,
                It.Is<string>(value => !string.IsNullOrEmpty(value)),
                It.Is<CookieOptions>(options =>
                    options.HttpOnly &&
                    options.Secure &&
                    options.SameSite == SameSiteMode.None &&
                    options.Path == "/api/")),
            Times.Once);

        _initServiceMock.Verify(s => s.ProcessRequest(httpRequest, _correlationId, _cc), Times.Once);
    }

    [Fact]
    public async Task Run_ReturnsRedirectResultWithHttpCookie_WhenRequestIsHttp()
    {
        // Arrange
        var cookiesMock = new Mock<IResponseCookies>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = true,
            Cc = _cc,
            Ct = _ct
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId, cookiesMock);

        // Force HTTP
        httpRequest.Scheme = "http";

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(_redirectUrl, redirectResult.Url);

        cookiesMock.Verify(
            c => c.Append(
                HttpHeaderKeys.CmsAuthValues,
                It.Is<string>(value => !string.IsNullOrEmpty(value)),
                It.Is<CookieOptions>(options =>
                    options.HttpOnly &&
                    !options.Secure &&
                    options.Path == "/api/")),
            Times.Once);
    }

    [Fact]
    public async Task Run_DoesNotSetCookie_WhenShouldSetCookieIsFalse()
    {
        // Arrange
        var cookiesMock = new Mock<IResponseCookies>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = false
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId, cookiesMock);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(_redirectUrl, redirectResult.Url);

        cookiesMock.Verify(
            c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_DoesNotSetCookie_WhenCcIsEmpty()
    {
        // Arrange
        var cookiesMock = new Mock<IResponseCookies>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = true,
            Cc = string.Empty,
            Ct = _ct
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, string.Empty))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", string.Empty } }, _correlationId, cookiesMock);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(_redirectUrl, redirectResult.Url);

        cookiesMock.Verify(
            c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_DoesNotSetCookie_WhenCtIsEmpty()
    {
        // Arrange
        var cookiesMock = new Mock<IResponseCookies>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = true,
            Cc = _cc,
            Ct = string.Empty
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId, cookiesMock);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(_redirectUrl, redirectResult.Url);

        cookiesMock.Verify(
            c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_Returns500_WhenRedirectResultHasNullRedirectUrl()
    {
        // Arrange
        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = null,
            ShouldSetCookie = false
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Run_Returns500_WhenRedirectResultHasEmptyRedirectUrl()
    {
        // Arrange
        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = string.Empty,
            ShouldSetCookie = false
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenInitServiceReturnsBadRequestStatus()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.BadRequest,
            Message = errorMessage
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }

    [Fact]
    public async Task Run_Returns500_WhenInitServiceReturnsServerErrorStatus()
    {
        // Arrange
        var initResult = new InitResult
        {
            Status = InitResultStatus.ServerError
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Run_Returns500_WhenInitServiceReturnsUnhandledStatus()
    {
        // Arrange

        var initResult = new InitResult
        {
            Status = (InitResultStatus)999
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Run_HandlesNullCcParameter_PassesNullToService()
    {
        // Arrange
        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = false
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, null))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), _correlationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(_redirectUrl, redirectResult.Url);
    }

    [Fact]
    public async Task Run_DeletesCmsAuthValuesCookie_OnEveryRequest()
    {
        // Arrange
        var cookiesMock = new Mock<IResponseCookies>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = false
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId, cookiesMock);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        cookiesMock.Verify(c => c.Delete(HttpHeaderKeys.CmsAuthValues), Times.Once);
    }

    [Fact]
    public async Task Run_LogsInformationMessage_OnEveryRequest()
    {
        // Arrange
        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = false
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Init function processed a request with correlation ID: {_correlationId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_VerifiesAuthenticationContextCreation_WhenSettingCookie()
    {
        // Arrange
        var cookiesMock = new Mock<IResponseCookies>();

        var initResult = new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = _redirectUrl,
            ShouldSetCookie = true,
            Cc = _cc,
            Ct = _ct
        };

        _initServiceMock
            .Setup(s => s.ProcessRequest(It.IsAny<HttpRequest>(), _correlationId, _cc))
            .ReturnsAsync(initResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { { "cc", _cc } }, _correlationId, cookiesMock);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        cookiesMock.Verify(
            c => c.Append(
                HttpHeaderKeys.CmsAuthValues,
                It.Is<string>(value => !string.IsNullOrEmpty(value)),
                It.IsAny<CookieOptions>()),
            Times.Once);
    }
}
