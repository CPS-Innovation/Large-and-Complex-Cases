using System.Net;
using System.Text;
using System.Text.Json;
using AutoFixture;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.NetApp;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.NetApp.Tests.Unit.Client;

public class NetAppHttpClientTests
{
    private readonly Fixture _fixture;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<INetAppRequestFactory> _requestFactoryMock = new();
    private readonly NetAppHttpClient _client;

    public NetAppHttpClientTests()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        {
            httpClient.BaseAddress = new Uri("http://localhost/");
        }
        _client = new NetAppHttpClient(httpClient, _requestFactoryMock.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task RegisterUserAsync_ReturnsUserResponse_OnSuccess()
    {
        var expectedName = "New User";
        var expectedAccessKey = _fixture.Create<string>();
        var expectedSecretKey = _fixture.Create<string>();
        var arg = new RegisterUserArg { Username = expectedName };
        var request = new HttpRequestMessage();
        var expectedResponse = new NetAppUserResponse { Records = [new NetAppUserRecord { Name = expectedName, AccessKey = expectedAccessKey, SecretKey = expectedSecretKey }] };
        var responseContent = JsonSerializer.Serialize(expectedResponse);

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _client.RegisterUserAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedName, result.Records[0].Name);
        Assert.Equal(expectedAccessKey, result.Records[0].AccessKey);
        Assert.Equal(expectedSecretKey, result.Records[0].SecretKey);
    }

    [Fact]
    public async Task RegenerateUserKeysAsync_ReturnsUserResponse_OnSuccess()
    {
        // Arrange
        var expectedName = "Existing User";
        var expectedAccessKey = _fixture.Create<string>();
        var expectedSecretKey = _fixture.Create<string>();
        var arg = new RegenerateUserKeysArg { Username = expectedName };
        var request = new HttpRequestMessage();
        var expectedResponse = new NetAppUserResponse { Records = [new NetAppUserRecord { Name = expectedName, AccessKey = expectedAccessKey, SecretKey = expectedSecretKey }] };
        var responseContent = JsonSerializer.Serialize(expectedResponse);

        _requestFactoryMock.Setup(f => f.CreateRegenerateUserKeysRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _client.RegenerateUserKeysAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedName, result.Records[0].Name);
        Assert.Equal(expectedAccessKey, result.Records[0].AccessKey);
        Assert.Equal(expectedSecretKey, result.Records[0].SecretKey);
    }

    [Fact]
    public async Task CallNetApp_ThrowsNetAppUnauthorizedException_OnUnauthorized()
    {
        // Arrange
        var arg = new RegisterUserArg { Username = "Test User" };
        var request = new HttpRequestMessage();

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        // Act & Assert
        await Assert.ThrowsAsync<NetAppUnauthorizedException>(() => _client.RegisterUserAsync(arg));
    }

    [Fact]
    public async Task CallNetApp_ThrowsNetAppNotFoundException_OnNotFound()
    {
        // Arrange
        var arg = new RegisterUserArg { Username = "Test User" };
        var request = new HttpRequestMessage();

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                ReasonPhrase = "User not found"
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NetAppNotFoundException>(() => _client.RegisterUserAsync(arg));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task CallNetApp_ThrowsNetAppConflictException_OnConflict()
    {
        // Arrange
        var arg = new RegisterUserArg { Username = "Test User" };
        var request = new HttpRequestMessage();

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict
            });

        // Act & Assert
        await Assert.ThrowsAsync<NetAppConflictException>(() => _client.RegisterUserAsync(arg));
    }

    [Fact]
    public async Task CallNetApp_ThrowsNetAppClientException_OnOtherError()
    {
        // Arrange
        var arg = new RegisterUserArg { Username = "Test User" };
        var request = new HttpRequestMessage();

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server error", Encoding.UTF8, "text/plain")
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NetAppClientException>(() => _client.RegisterUserAsync(arg));
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public async Task CallNetApp_ThrowsInvalidOperationException_WhenDeserializationReturnsNull()
    {
        // Arrange
        var arg = new RegisterUserArg { Username = "Test User" };
        var request = new HttpRequestMessage();

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _client.RegisterUserAsync(arg));
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowJsonException_WhenResponseIsInvalidJson()
    {
        // Arrange
        var arg = new RegisterUserArg { Username = "Test User" };
        var request = new HttpRequestMessage();
        var responseContent = "invalid-json{]";

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _client.RegisterUserAsync(arg));
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowJsonException_WhenResponseIsMalformedJson()
    {
        // Arrange
        var arg = new RegisterUserArg { Username = "Test User" };
        var request = new HttpRequestMessage();
        var responseContent = "{\"username\":\"Test User\"";

        _requestFactoryMock.Setup(f => f.CreateRegisterUserRequest(arg)).Returns(request);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _client.RegisterUserAsync(arg));
    }
}