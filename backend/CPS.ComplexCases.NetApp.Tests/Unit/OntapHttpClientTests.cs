using System.Net;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit;

public class OntapHttpClientTests
{
    private readonly Mock<IOntapArgFactory> _ontapArgFactoryMock = new();
    private readonly Mock<IOntapRequestFactory> _ontapRequestFactoryMock = new();
    private readonly Mock<IHttpResponseHandler> _httpResponseHandlerMock = new();
    private readonly HttpClient _httpClient = new();
    private readonly OntapHttpClient _client;
    private const string OntapUrl = "https://ontap-url/api/storage/volumes/uuid/files/file-path";

    public OntapHttpClientTests()
    {
        _client = new OntapHttpClient(
            _httpClient,
            _ontapArgFactoryMock.Object,
            _ontapRequestFactoryMock.Object,
            _httpResponseHandlerMock.Object);
    }

    [Fact]
    public async Task RenameMaterialAsync_ReturnsOkResult_WhenResponseIsSuccessful()
    {
        var bearerToken = "token";
        var ontapVolumeUuid = Guid.NewGuid();
        var currentFolderPath = "path/current-name.pdf";
        var newFolderPath = "path/new-name.pdf";

        var materialRenameArg = new MaterialRenameArg
        {
            BearerToken = bearerToken,
            OntapVolumeUuid = ontapVolumeUuid,
            CurrentFilePath = currentFolderPath,
            NewFilePath = newFolderPath
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(bearerToken, ontapVolumeUuid, currentFolderPath, newFolderPath))
            .Returns(materialRenameArg);

        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);

        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var result = await _client.RenameMaterialAsync(bearerToken, ontapVolumeUuid, currentFolderPath, newFolderPath);

        Assert.IsType<OkResult>(result);
        _ontapArgFactoryMock.Verify(
            f => f.CreateMaterialRenameArg(bearerToken, ontapVolumeUuid, currentFolderPath, newFolderPath),
            Times.Once);
        _ontapRequestFactoryMock.Verify(f => f.CreateRenameMaterialRequest(materialRenameArg), Times.Once);
        _httpResponseHandlerMock.Verify(
            h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.Is<HttpStatusCode[]>(codes => codes.Length == 0)),
            Times.Once);
    }

    [Fact]
    public async Task RenameMaterialAsync_ReturnsNotFoundObjectResult_WhenResponseIsNotFound()
    {
        var ontapVolumeUuid = Guid.NewGuid();
        var currentFolderPath = "path/current-name.pdf";
        var newFolderPath = "path/new-name.pdf";
        var materialRenameArg = new MaterialRenameArg
        {
            BearerToken = "token",
            OntapVolumeUuid = ontapVolumeUuid,
            CurrentFilePath = "path/current-name.pdf",
            NewFilePath = "path/new-name.pdf"
        };
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(materialRenameArg);
        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        var result = await _client.RenameMaterialAsync("token", ontapVolumeUuid, currentFolderPath, newFolderPath);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Material not found at path: {currentFolderPath}", notFoundResult.Value);
    }

    [Fact]
    public async Task RenameMaterialAsync_ReturnsConflictObjectResult_WhenResponseIsConflict()
    {
        var ontapVolumeUuid = Guid.NewGuid();
        var currentFolderPath = "path/current-name.pdf";
        var newFolderPath = "path/new-name.pdf";
        var materialRenameArg = new MaterialRenameArg
        {
            BearerToken = "token",
            OntapVolumeUuid = ontapVolumeUuid,
            CurrentFilePath = currentFolderPath,
            NewFilePath = newFolderPath
        };
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(materialRenameArg);
        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict });

        var result = await _client.RenameMaterialAsync("token", ontapVolumeUuid, currentFolderPath, newFolderPath);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal($"Conflict occurred while renaming material from {currentFolderPath} to {newFolderPath}", conflictResult.Value);
    }

    [Fact]
    public async Task RenameMaterialAsync_ThrowsOntapClientException_WhenResponseStatusCodeIsUnexpected()
    {
        var materialRenameArg = new MaterialRenameArg
        {
            BearerToken = "token",
            OntapVolumeUuid = Guid.NewGuid(),
            CurrentFilePath = "path/current-name.pdf",
            NewFilePath = "path/new-name.pdf"
        };
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(materialRenameArg);
        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

        var exception = await Assert.ThrowsAsync<OntapClientException>(() =>
            _client.RenameMaterialAsync("token", Guid.NewGuid(), "path/current-name.pdf", "path/new-name.pdf"));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("Unexpected status code: BadRequest", exception.InnerException.Message);
    }

    [Fact]
    public async Task RenameMaterialAsync_PassesExpectedStatusCodeExceptionFactories_ToResponseHandler()
    {
        HttpResponseHandlerConfig? capturedConfig = null;
        var materialRenameArg = new MaterialRenameArg
        {
            BearerToken = "token",
            OntapVolumeUuid = Guid.NewGuid(),
            CurrentFilePath = "path/current-name.pdf",
            NewFilePath = "path/new-name.pdf"
        };
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(materialRenameArg);
        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .Callback<HttpClient, HttpRequestMessage, HttpResponseHandlerConfig, HttpStatusCode[]>((_, _, config, _) => capturedConfig = config)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _client.RenameMaterialAsync("token", Guid.NewGuid(), "path/current-name.pdf", "path/new-name.pdf");

        Assert.NotNull(capturedConfig);

        var unauthorized = capturedConfig.StatusCodeExceptionFactories[HttpStatusCode.Unauthorized](
            new HttpResponseMessage { ReasonPhrase = "Unauthorized reason" });
        Assert.IsType<OntapUnauthorizedException>(unauthorized);
        Assert.Equal("Unauthorized reason", unauthorized.Message);

        var notFound = capturedConfig.StatusCodeExceptionFactories[HttpStatusCode.NotFound](
            new HttpResponseMessage { ReasonPhrase = "Not found reason" });
        Assert.IsType<OntapNotFoundException>(notFound);
        Assert.Equal("Not found reason", notFound.Message);

        var conflict = capturedConfig.StatusCodeExceptionFactories[HttpStatusCode.Conflict](
            new HttpResponseMessage { ReasonPhrase = "Conflict reason" });
        Assert.IsType<OntapConflictException>(conflict);
        Assert.Equal("Conflict reason", conflict.Message);
    }

    [Fact]
    public async Task RenameMaterialAsync_PassesDefaultExceptionFactory_ToResponseHandler()
    {
        HttpResponseHandlerConfig? capturedConfig = null;
        var materialRenameArg = new MaterialRenameArg
        {
            BearerToken = "token",
            OntapVolumeUuid = Guid.NewGuid(),
            CurrentFilePath = "path/current-name.pdf",
            NewFilePath = "path/new-name.pdf"
        };
        var request = new HttpRequestMessage(HttpMethod.Patch, OntapUrl);

        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(materialRenameArg);
        _ontapRequestFactoryMock
            .Setup(f => f.CreateRenameMaterialRequest(materialRenameArg))
            .Returns(request);
        _httpResponseHandlerMock
            .Setup(h => h.SendAsync(_httpClient, request, It.IsAny<HttpResponseHandlerConfig>(), It.IsAny<HttpStatusCode[]>()))
            .Callback<HttpClient, HttpRequestMessage, HttpResponseHandlerConfig, HttpStatusCode[]>((_, _, config, _) => capturedConfig = config)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _client.RenameMaterialAsync("token", Guid.NewGuid(), "path/current-name.pdf", "path/new-name.pdf");

        Assert.NotNull(capturedConfig);

        var innerException = new HttpRequestException("request failed");
        var exception = capturedConfig.DefaultExceptionFactory(HttpStatusCode.InternalServerError, innerException);

        var ontapException = Assert.IsType<OntapClientException>(exception);
        Assert.Equal(HttpStatusCode.InternalServerError, ontapException.StatusCode);
        Assert.Same(innerException, ontapException.InnerException);
    }
}