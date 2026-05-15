using Amazon.S3.Model;
using Azure;
using Azure.Storage.Blobs;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AutoFixture;

namespace CPS.ComplexCases.API.Tests.Unit.Services;

public class DocumentServiceTests : IDisposable
{
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<IConversionService> _conversionServiceMock;
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly Mock<BlobContainerClient> _blobContainerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;
    private readonly DocumentService _service;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;

    private const string BlobContainerName = "test-container";

    public DocumentServiceTests()
    {
        _loggerMock = new Mock<ILogger<DocumentService>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _conversionServiceMock = new Mock<IConversionService>();
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _blobContainerClientMock = new Mock<BlobContainerClient>();
        _blobClientMock = new Mock<BlobClient>();
        _fixture = new Fixture();

        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();

        Environment.SetEnvironmentVariable("BlobContainerName", BlobContainerName);

        _blobServiceClientMock
            .Setup(s => s.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_blobContainerClientMock.Object);

        _blobContainerClientMock
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        _blobClientMock
            .Setup(c => c.DeleteIfExistsAsync(
                It.IsAny<Azure.Storage.Blobs.Models.DeleteSnapshotsOption>(),
                It.IsAny<Azure.Storage.Blobs.Models.BlobRequestConditions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(new GetObjectArg { BearerToken = "token", BucketName = "bucket", ObjectKey = "key" });

        _service = new DocumentService(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _conversionServiceMock.Object,
            _blobServiceClientMock.Object);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("BlobContainerName", null);
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_ReturnsNull_WhenNetAppThrowsFileNotFoundException()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ThrowsAsync(new FileNotFoundException("Object not found in bucket."));

        // Act
        var result = await _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName);

        // Assert
        Assert.Null(result);
        _conversionServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_DoesNotAttemptBlobCleanup_WhenNetAppThrowsFileNotFoundException()
    {
        // Arrange — exception is thrown before tmpFileName is set, so no blobs should be touched.
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ThrowsAsync(new FileNotFoundException("Object not found in bucket."));

        // Act
        await _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName);

        // Assert
        _blobContainerClientMock.Verify(
            c => c.GetBlobClient(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_ReturnsNull_WhenNetAppReturnsNullResponse()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync((GetObjectResponse?)null);

        // Act
        var result = await _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName);

        // Assert
        Assert.Null(result);
        _conversionServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_ReturnsNull_WhenNetAppResponseStreamIsNull()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = null! });

        // Act
        var result = await _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName);

        // Assert
        Assert.Null(result);
        _conversionServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_ThrowsInvalidOperationException_WhenSaveToTempBlobFails()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        _conversionServiceMock
            .Setup(s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName));
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_ThrowsInvalidOperationException_WhenConversionFails()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        _conversionServiceMock
            .Setup(s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _conversionServiceMock
            .Setup(s => s.ConvertToPdfAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((string?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName));
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_PropagatesNotSupportedException_WhenConversionThrows()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        _conversionServiceMock
            .Setup(s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _conversionServiceMock
            .Setup(s => s.ConvertToPdfAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new NotSupportedException("Unsupported content type: application/x-custom"));

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName));
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_ReturnsFileStreamResult_WhenConversionSucceeds()
    {
        // Arrange
        var pdfBlobUrl = "https://storage.example.com/container/preview_document.pdf.pdf";

        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        _conversionServiceMock
            .Setup(s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _conversionServiceMock
            .Setup(s => s.ConvertToPdfAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(pdfBlobUrl);

        _blobClientMock
            .Setup(c => c.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response>());

        // Act
        var result = await _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName);

        // Assert
        var fileStreamResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileStreamResult.ContentType);
        Assert.EndsWith("document.pdf.pdf", fileStreamResult.FileDownloadName);
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_DeletesTempBlobs_AfterSuccessfulConversion()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        _conversionServiceMock
            .Setup(s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _conversionServiceMock
            .Setup(s => s.ConvertToPdfAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync("https://example.com/preview.pdf");

        _blobClientMock
            .Setup(c => c.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response>());

        // Act
        await _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName);

        // Assert
        _blobContainerClientMock.Verify(c => c.GetBlobClient(It.Is<string>(s => s.EndsWith("_document.pdf"))), Times.AtLeastOnce);
        _blobContainerClientMock.Verify(c => c.GetBlobClient(It.Is<string>(s => s.EndsWith("_document.pdf.pdf"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_DeletesTempBlobs_EvenAfterConversionFailure()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        _conversionServiceMock
            .Setup(s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _conversionServiceMock
            .Setup(s => s.ConvertToPdfAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((string?)null);

        // Act
        try { await _service.GetMaterialPreviewAsync("/case/document.pdf", _testBearerToken, _testBucketName); }
        catch (InvalidOperationException) { }

        // Assert
        _blobContainerClientMock.Verify(c => c.GetBlobClient(It.Is<string>(s => s.EndsWith("_document.pdf"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetMaterialPreviewAsync_PassesCorrectFileNameToConversionService()
    {
        // Arrange
        _netAppClientMock
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArg>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        _conversionServiceMock
            .Setup(s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        try { await _service.GetMaterialPreviewAsync("/case/subdir/report.docx", _testBearerToken, _testBucketName); }
        catch (InvalidOperationException) { }

        // Assert
        _conversionServiceMock.Verify(
            s => s.SaveDocumentToTemporaryBlobAsync(It.IsAny<Stream>(), It.Is<string>(n => n.EndsWith("_report.docx"))),
            Times.Once);
    }
}
