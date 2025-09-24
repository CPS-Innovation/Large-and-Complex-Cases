using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Services;

public class InitServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<InitService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IDdeiClient> _ddeiClientMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly InitService _service;

    public InitServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<InitService>>>();
        _configurationMock = _fixture.Freeze<Mock<IConfiguration>>();
        _ddeiClientMock = _fixture.Freeze<Mock<IDdeiClient>>();
        _ddeiArgFactoryMock = _fixture.Freeze<Mock<IDdeiArgFactory>>();

        _service = new InitService(
            _loggerMock.Object,
            _configurationMock.Object,
            _ddeiClientMock.Object,
            _ddeiArgFactoryMock.Object);
    }

    [Fact]
    public async Task ProcessRequest_WhenRedirectUrlsAreMissing_ReturnsServerError()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var cc = _fixture.Create<string>();

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(string.Empty);
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(_fixture.Create<string>());

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, cc);

        // Assert
        Assert.Equal(InitResultStatus.ServerError, result.Status);
        Assert.Equal("One or more redirect URL's are missing", result.Message);
        Assert.Null(result.RedirectUrl);
        Assert.False(result.ShouldSetCookie);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("One or more redirect URL's are missing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessRequest_WhenLccUiRedirectUrlIsMissing_ReturnsServerError()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var cc = _fixture.Create<string>();

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(_fixture.Create<string>());
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(string.Empty);

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, cc);

        // Assert
        Assert.Equal(InitResultStatus.ServerError, result.Status);
        Assert.Equal("One or more redirect URL's are missing", result.Message);
    }

    [Fact]
    public async Task ProcessRequest_WhenBothRedirectUrlsAreMissing_ReturnsServerError()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var cc = _fixture.Create<string>();

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(string.Empty);
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(string.Empty);

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, cc);

        // Assert
        Assert.Equal(InitResultStatus.ServerError, result.Status);
        Assert.Equal("One or more redirect URL's are missing", result.Message);
    }

    [Fact]
    public async Task ProcessRequest_WhenCcIsProvidedAndDdeiClientSucceeds_ReturnsRedirectWithCookies()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var cc = _fixture.Create<string>();
        var ct = _fixture.Create<string>();
        var redirectUrlLccUi = _fixture.Create<string>();
        var baseArg = _fixture.Create<DdeiBaseArgDto>();

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(_fixture.Create<string>());
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(redirectUrlLccUi);

        _ddeiArgFactoryMock
            .Setup(f => f.CreateBaseArg(It.IsAny<string>(), correlationId))
            .Returns(baseArg);

        _ddeiClientMock
            .Setup(c => c.GetCmsModernTokenAsync(baseArg))
            .ReturnsAsync(ct);

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, cc);

        // Assert
        Assert.Equal(InitResultStatus.Redirect, result.Status);
        Assert.Equal(redirectUrlLccUi, result.RedirectUrl);
        Assert.True(result.ShouldSetCookie);
        Assert.Equal(cc, result.Cc);
        Assert.Equal(ct, result.Ct);

        _ddeiArgFactoryMock.Verify(
            f => f.CreateBaseArg(It.IsAny<string>(), correlationId),
            Times.Once);

        _ddeiClientMock.Verify(
            c => c.GetCmsModernTokenAsync(baseArg),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Redirecting to {redirectUrlLccUi} with correlationId {correlationId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessRequest_WhenCcIsProvidedAndDdeiClientFails_ReturnsRedirectWithCcButNoCt()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var cc = _fixture.Create<string>();
        var redirectUrlLccUi = _fixture.Create<string>();
        var baseArg = _fixture.Create<DdeiBaseArgDto>();
        var expectedException = new Exception("DDEI client error");

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(_fixture.Create<string>());
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(redirectUrlLccUi);

        _ddeiArgFactoryMock
            .Setup(f => f.CreateBaseArg(It.IsAny<string>(), correlationId))
            .Returns(baseArg);

        _ddeiClientMock
            .Setup(c => c.GetCmsModernTokenAsync(baseArg))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, cc);

        // Assert
        Assert.Equal(InitResultStatus.Redirect, result.Status);
        Assert.Equal(redirectUrlLccUi, result.RedirectUrl);
        Assert.True(result.ShouldSetCookie);
        Assert.Equal(cc, result.Cc);
        Assert.Null(result.Ct);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessRequest_WhenCcIsEmpty_RedirectsToCaseworkApp()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var redirectUrlCwa = _fixture.Create<string>();
        var expectedBuiltRedirectUrl = $"{redirectUrlCwa}https://localhost/api/v1/init";

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(redirectUrlCwa);
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(_fixture.Create<string>());

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, string.Empty);

        // Assert
        Assert.Equal(InitResultStatus.Redirect, result.Status);
        Assert.Equal(expectedBuiltRedirectUrl, result.RedirectUrl);
        Assert.False(result.ShouldSetCookie);
        Assert.Null(result.Cc);
        Assert.Null(result.Ct);

        _ddeiArgFactoryMock.Verify(
            f => f.CreateBaseArg(It.IsAny<string>(), It.IsAny<Guid>()),
            Times.Never);

        _ddeiClientMock.Verify(
            c => c.GetCmsModernTokenAsync(It.IsAny<DdeiBaseArgDto>()),
            Times.Never);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Redirecting to {redirectUrlCwa} with correlationId {correlationId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessRequest_WhenCcIsNull_RedirectsToCaseworkApp()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var redirectUrlCwa = _fixture.Create<string>();

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(redirectUrlCwa);
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(_fixture.Create<string>());

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, null);

        // Assert
        Assert.Equal(InitResultStatus.Redirect, result.Status);
        Assert.False(result.ShouldSetCookie);
        Assert.Null(result.Cc);
        Assert.Null(result.Ct);
    }

    [Fact]
    public void BuildRedirectUrl_WhenValidInputProvided_BuildsCorrectUrl()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var redirectUrlCwa = "https://casework.example.com?r=";

        // Act
        var result = _service.BuildRedirectUrl(request.Object, redirectUrlCwa);

        // Assert
        var expectedUrl = $"{redirectUrlCwa}https://localhost/api/v1/init";
        Assert.Equal(expectedUrl, result);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Built redirect URL: {expectedUrl}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void BuildRedirectUrl_WhenHttpRequest_BuildsCorrectUrl()
    {
        // Arrange
        var request = CreateMockHttpRequest("http", "example.com", 80);
        var redirectUrlCwa = "https://casework.example.com?r=";

        // Act
        var result = _service.BuildRedirectUrl(request.Object, redirectUrlCwa);

        // Assert
        var expectedUrl = $"{redirectUrlCwa}http://example.com/api/v1/init";
        Assert.Equal(expectedUrl, result);
    }

    [Fact]
    public void BuildRedirectUrl_WhenCustomPortRequest_BuildsCorrectUrl()
    {
        // Arrange
        var request = CreateMockHttpRequest("https", "api.example.com", 8443);
        var redirectUrlCwa = "https://casework.example.com?r=";

        // Act
        var result = _service.BuildRedirectUrl(request.Object, redirectUrlCwa);

        // Assert
        var expectedUrl = $"{redirectUrlCwa}https://api.example.com:8443/api/v1/init";
        Assert.Equal(expectedUrl, result);
    }

    [Fact]
    public async Task ProcessRequest_WhenConfigurationValuesAreNull_ReturnsServerError()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var cc = _fixture.Create<string>();

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns((string?)null);
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns((string?)null);

        // Act
        var result = await _service.ProcessRequest(request.Object, correlationId, cc);

        // Assert
        Assert.Equal(InitResultStatus.ServerError, result.Status);
        Assert.Equal("One or more redirect URL's are missing", result.Message);
    }

    [Fact]
    public async Task ProcessRequest_VerifiesAuthenticationContextCreation()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        var correlationId = Guid.NewGuid();
        var cc = _fixture.Create<string>();
        var baseArg = _fixture.Create<DdeiBaseArgDto>();

        _configurationMock.Setup(c => c["RedirectUrl:CaseworkApp"]).Returns(_fixture.Create<string>());
        _configurationMock.Setup(c => c["RedirectUrl:LccUi"]).Returns(_fixture.Create<string>());

        _ddeiArgFactoryMock
            .Setup(f => f.CreateBaseArg(It.IsAny<string>(), correlationId))
            .Returns(baseArg);

        _ddeiClientMock
            .Setup(c => c.GetCmsModernTokenAsync(baseArg))
            .ReturnsAsync(_fixture.Create<string>());

        // Act
        await _service.ProcessRequest(request.Object, correlationId, cc);

        // Assert
        _ddeiArgFactoryMock.Verify(
            f => f.CreateBaseArg(
                It.Is<string>(authContext => !string.IsNullOrEmpty(authContext)),
                correlationId),
            Times.Once);
    }

    private Mock<HttpRequest> CreateMockHttpRequest(string scheme = "https", string host = "localhost", int? port = null)
    {
        var request = new Mock<HttpRequest>();
        var hostString = port.HasValue &&
                        ((scheme == "https" && port != 443) || (scheme == "http" && port != 80))
            ? new HostString(host, port.Value)
            : new HostString(host);

        request.Setup(r => r.Scheme).Returns(scheme);
        request.Setup(r => r.Host).Returns(hostString);

        return request;
    }
}