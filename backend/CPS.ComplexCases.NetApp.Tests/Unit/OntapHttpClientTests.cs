using System.Net;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit;

public class OntapHttpClientTests
{
    private readonly Mock<IOntapRequestFactory> _ontapRequestFactoryMock = new();
    private readonly Mock<IHttpResponseHandler> _httpResponseHandlerMock = new();
    private readonly HttpClient _httpClient = new();
    private readonly OntapHttpClient _client;
    private const string OntapUrl = "https://ontap-url/api/storage/volumes/uuid/files/file-path";

    public OntapHttpClientTests()
    {
        _client = new OntapHttpClient(
            _httpClient,
            _ontapRequestFactoryMock.Object,
            _httpResponseHandlerMock.Object);
    }

    [Fact]
    public async Task RenameMaterialAsync_ReturnsOkResult_WhenResponseIsSuccessful()
    {
        var materialRenameArg = CreateRenameArg();

        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);

        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var result = await _client.RenameMaterialAsync(materialRenameArg);

        Assert.True(result.Success);
        Assert.True(result.WasFound);
        Assert.Equal(1, result.KeysRenamed);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ErrorStatusCode);
        _ontapRequestFactoryMock.Verify(f => f.CreateRenameMaterialRequest(materialRenameArg), Times.Once);
        _httpResponseHandlerMock.Verify(
            h => h.SendAsync(
                _httpClient,
                request,
                It.IsAny<HttpResponseHandlerConfig>(),
                It.Is<HttpStatusCode[]>(codes =>
                    codes.Length == 2 &&
                    codes.Contains(HttpStatusCode.NotFound) &&
                    codes.Contains(HttpStatusCode.Conflict))),
            Times.Once);
    }

    [Fact]
    public async Task RenameMaterialAsync_ReturnsNotFoundObjectResult_WhenResponseIsNotFound()
    {
        var materialRenameArg = CreateRenameArg();
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        var result = await _client.RenameMaterialAsync(materialRenameArg);

        Assert.False(result.Success);
        Assert.False(result.WasFound);
        Assert.Equal(0, result.KeysRenamed);
        Assert.Equal($"Material not found at path: {materialRenameArg.CurrentFilePath}", result.ErrorMessage);
        Assert.Equal((int)HttpStatusCode.NotFound, result.ErrorStatusCode);
    }

    [Fact]
    public async Task RenameMaterialAsync_ReturnsConflictObjectResult_WhenResponseIsConflict()
    {
        var materialRenameArg = CreateRenameArg();
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict });

        var result = await _client.RenameMaterialAsync(materialRenameArg);

        Assert.False(result.Success);
        Assert.True(result.WasFound);
        Assert.Equal(0, result.KeysRenamed);
        Assert.Equal($"Conflict occurred while renaming material from {materialRenameArg.CurrentFilePath} to {materialRenameArg.NewFilePath}", result.ErrorMessage);
        Assert.Equal((int)HttpStatusCode.Conflict, result.ErrorStatusCode);
    }

    [Fact]
    public async Task RenameMaterialAsync_ThrowsOntapClientException_WhenResponseStatusCodeIsUnexpected()
    {
        var materialRenameArg = CreateRenameArg();
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

        var exception = await Assert.ThrowsAsync<OntapClientException>(() =>
            _client.RenameMaterialAsync(materialRenameArg));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("Unexpected status code: BadRequest", exception.InnerException.Message);
    }

    [Fact]
    public async Task RenameMaterialAsync_PassesExpectedUnhappyStatusCodes_ToResponseHandler()
    {
        HttpStatusCode[]? capturedExpectedUnhappyStatusCodes = null;
        var materialRenameArg = CreateRenameArg();
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .Callback<HttpClient, HttpRequestMessage, HttpResponseHandlerConfig, HttpStatusCode[]>((_, _, _, expectedUnhappyStatusCodes) =>
                capturedExpectedUnhappyStatusCodes = expectedUnhappyStatusCodes)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _client.RenameMaterialAsync(materialRenameArg);

        Assert.NotNull(capturedExpectedUnhappyStatusCodes);
        Assert.Equal(2, capturedExpectedUnhappyStatusCodes.Length);
        Assert.Contains(HttpStatusCode.NotFound, capturedExpectedUnhappyStatusCodes);
        Assert.Contains(HttpStatusCode.Conflict, capturedExpectedUnhappyStatusCodes);
    }

    [Fact]
    public async Task RenameMaterialAsync_PassesExpectedStatusCodeExceptionFactory_ForUnauthorized_ToResponseHandler()
    {
        HttpResponseHandlerConfig? capturedConfig = null;
        var materialRenameArg = CreateRenameArg();
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .Callback<HttpClient, HttpRequestMessage, HttpResponseHandlerConfig, HttpStatusCode[]>((_, _, config, _) => capturedConfig = config)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _client.RenameMaterialAsync(materialRenameArg);

        Assert.NotNull(capturedConfig);

        var unauthorized = capturedConfig.StatusCodeExceptionFactories[HttpStatusCode.Unauthorized](
            new HttpResponseMessage { ReasonPhrase = "Unauthorized reason" });
        Assert.IsType<OntapUnauthorizedException>(unauthorized);
        Assert.Equal("Unauthorized reason", unauthorized.Message);
    }

    [Fact]
    public async Task RenameMaterialAsync_PassesDefaultExceptionFactory_ToResponseHandler()
    {
        HttpResponseHandlerConfig? capturedConfig = null;
        var materialRenameArg = CreateRenameArg();
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .Callback<HttpClient, HttpRequestMessage, HttpResponseHandlerConfig, HttpStatusCode[]>((_, _, config, _) => capturedConfig = config)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _client.RenameMaterialAsync(materialRenameArg);

        Assert.NotNull(capturedConfig);

        var innerException = new HttpRequestException("request failed");
        var exception = capturedConfig.DefaultExceptionFactory(HttpStatusCode.InternalServerError, innerException);

        var ontapException = Assert.IsType<OntapClientException>(exception);
        Assert.Equal(HttpStatusCode.InternalServerError, ontapException.StatusCode);
        Assert.Same(innerException, ontapException.InnerException);
    }

    [Fact]
    public async Task RenameMaterialAsync_WithRealHttpResponseHandler_ReturnsNotFoundObjectResult_WhenOntapReturnsNotFound()
    {
        var materialRenameArg = CreateRenameArg();

        var client = CreateClientWithRealResponseHandler(HttpStatusCode.NotFound);

        var result = await client.RenameMaterialAsync(materialRenameArg);

        Assert.False(result.Success);
        Assert.False(result.WasFound);
        Assert.Equal(0, result.KeysRenamed);
        Assert.Equal($"Material not found at path: {materialRenameArg.CurrentFilePath}", result.ErrorMessage);
        Assert.Equal((int)HttpStatusCode.NotFound, result.ErrorStatusCode);
    }

    [Fact]
    public async Task RenameMaterialAsync_WithRealHttpResponseHandler_ReturnsConflictObjectResult_WhenOntapReturnsConflict()
    {
        var materialRenameArg = CreateRenameArg();

        var client = CreateClientWithRealResponseHandler(HttpStatusCode.Conflict);

        var result = await client.RenameMaterialAsync(materialRenameArg);

        Assert.False(result.Success);
        Assert.True(result.WasFound);
        Assert.Equal(0, result.KeysRenamed);
        Assert.Equal($"Conflict occurred while renaming material from {materialRenameArg.CurrentFilePath} to {materialRenameArg.NewFilePath}", result.ErrorMessage);
        Assert.Equal((int)HttpStatusCode.Conflict, result.ErrorStatusCode);
    }

    private static OntapHttpClient CreateClientWithRealResponseHandler(HttpStatusCode responseStatusCode)
    {
        var requestFactoryMock = new Mock<IOntapRequestFactory>();

        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);
        requestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(It.IsAny<MaterialRenameArg>()))
            .Returns(request);

        var httpClient = new HttpClient(new StubHttpMessageHandler(responseStatusCode));

        return new OntapHttpClient(httpClient, requestFactoryMock.Object, new HttpResponseHandler());
    }

    private static MaterialRenameArg CreateRenameArg() => new()
    {
        BearerToken = "token",
        OntapVolumeUuid = Guid.NewGuid(),
        CurrentFilePath = "path/current-name.pdf",
        NewFilePath = "path/new-name.pdf"
    };

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage { StatusCode = statusCode });
    }
}