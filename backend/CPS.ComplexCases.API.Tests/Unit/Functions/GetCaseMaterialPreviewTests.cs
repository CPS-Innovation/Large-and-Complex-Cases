using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class GetCaseMaterialPreviewTests
{
    private readonly Mock<ILogger<GetCaseMaterialPreview>> _loggerMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<IDocumentService> _documentServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Fixture _fixture;
    private readonly GetCaseMaterialPreview _function;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;

    public GetCaseMaterialPreviewTests()
    {
        _loggerMock = new Mock<ILogger<GetCaseMaterialPreview>>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _documentServiceMock = new Mock<IDocumentService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _fixture = new Fixture();

        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
            .ReturnsAsync([
                new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = _testBucketName,
                    DisplayName = "Test Security Group"
                }
            ]);

        _function = new GetCaseMaterialPreview(
            _loggerMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _documentServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenPathQueryParamIsMissing()
    {
        // Arrange
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _fixture.Create<string>(), _testUsername, _testBearerToken);
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains(InputParameters.Path, badRequestResult.Value?.ToString());
        _documentServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenPathQueryParamIsWhitespace()
    {
        // Arrange
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _fixture.Create<string>(), _testUsername, _testBearerToken);
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(
            InputParameters.Path, "   ", _testCorrelationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _documentServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Run_ReturnsNotFound_WhenDocumentServiceReturnsNull()
    {
        // Arrange
        var path = "/case/documents/evidence.pdf";
        _documentServiceMock
            .Setup(s => s.GetMaterialPreviewAsync(path, _testBearerToken, _testBucketName))
            .ReturnsAsync((FileStreamResult?)null);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _fixture.Create<string>(), _testUsername, _testBearerToken);
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(
            InputParameters.Path, path, _testCorrelationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(path, notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task Run_ReturnsFileStreamResult_WhenDocumentFound()
    {
        // Arrange
        var path = "/case/documents/evidence.pdf";
        var pdfStream = new MemoryStream([1, 2, 3]);
        var fileStreamResult = new FileStreamResult(pdfStream, "application/pdf");

        _documentServiceMock
            .Setup(s => s.GetMaterialPreviewAsync(path, _testBearerToken, _testBucketName))
            .ReturnsAsync(fileStreamResult);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _fixture.Create<string>(), _testUsername, _testBearerToken);
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(
            InputParameters.Path, path, _testCorrelationId);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var streamResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", streamResult.ContentType);
    }

    [Fact]
    public async Task Run_CallsDocumentServiceWithCorrectParameters()
    {
        // Arrange
        var path = "/case/documents/evidence.pdf";
        _documentServiceMock
            .Setup(s => s.GetMaterialPreviewAsync(path, _testBearerToken, _testBucketName))
            .ReturnsAsync((FileStreamResult?)null);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _fixture.Create<string>(), _testUsername, _testBearerToken);
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(
            InputParameters.Path, path, _testCorrelationId);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _documentServiceMock.Verify(
            s => s.GetMaterialPreviewAsync(path, _testBearerToken, _testBucketName),
            Times.Once);
    }

    [Fact]
    public async Task Run_InitializesHandlerWithContextValues()
    {
        // Arrange
        var path = "/case/documents/evidence.pdf";
        _documentServiceMock
            .Setup(s => s.GetMaterialPreviewAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((FileStreamResult?)null);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _fixture.Create<string>(), _testUsername, _testBearerToken);
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(
            InputParameters.Path, path, _testCorrelationId);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _initializationHandlerMock.Verify(
            h => h.Initialize(_testUsername, _testCorrelationId, null),
            Times.Once);
    }

    [Fact]
    public async Task Run_PropagatesNotSupportedException_WhenDocumentServiceThrows()
    {
        // Arrange
        var path = "/case/documents/unknown.xyz";
        _documentServiceMock
            .Setup(s => s.GetMaterialPreviewAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new NotSupportedException("Unsupported file type"));

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _fixture.Create<string>(), _testUsername, _testBearerToken);
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(
            InputParameters.Path, path, _testCorrelationId);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _function.Run(httpRequest, functionContext));
    }
}
