using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CPS.ComplexCases.API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Services;

public class ConversionServiceTests : IDisposable
{
    private readonly Mock<ILogger<ConversionService>> _loggerMock;
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly Mock<BlobContainerClient> _blobContainerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;
    private readonly ConversionService _service;

    private const string BlobContainerName = "test-container";

    public ConversionServiceTests()
    {
        _loggerMock = new Mock<ILogger<ConversionService>>();
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _blobContainerClientMock = new Mock<BlobContainerClient>();
        _blobClientMock = new Mock<BlobClient>();

        Environment.SetEnvironmentVariable("BlobContainerName", BlobContainerName);

        _blobServiceClientMock
            .Setup(s => s.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_blobContainerClientMock.Object);

        _blobContainerClientMock
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        _blobContainerClientMock
            .Setup(c => c.CreateIfNotExistsAsync(
                It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobContainerEncryptionScopeOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());

        _service = new ConversionService(_loggerMock.Object, _blobServiceClientMock.Object);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("BlobContainerName", null);
    }


    [Fact]
    public async Task SaveDocumentToTemporaryBlobAsync_ReturnsFalse_WhenBlobContainerNameNotSet()
    {
        Environment.SetEnvironmentVariable("BlobContainerName", null);

        var result = await _service.SaveDocumentToTemporaryBlobAsync(new MemoryStream([1, 2, 3]), "document.pdf");

        Assert.False(result);
        _blobServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveDocumentToTemporaryBlobAsync_ReturnsTrue_WhenUploadSucceeds()
    {
        _blobClientMock
            .Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        var result = await _service.SaveDocumentToTemporaryBlobAsync(new MemoryStream([1, 2, 3]), "document.pdf");

        Assert.True(result);
    }

    [Fact]
    public async Task SaveDocumentToTemporaryBlobAsync_UploadsBlobWithTmpPrefix()
    {
        _blobClientMock
            .Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        await _service.SaveDocumentToTemporaryBlobAsync(new MemoryStream([1, 2, 3]), "report.docx");

        _blobContainerClientMock.Verify(c => c.GetBlobClient("tmp_report.docx"), Times.Once);
    }

    [Fact]
    public async Task SaveDocumentToTemporaryBlobAsync_ReturnsFalse_WhenUploadThrows()
    {
        _blobClientMock
            .Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Simulated upload failure"));

        var result = await _service.SaveDocumentToTemporaryBlobAsync(new MemoryStream([1, 2, 3]), "document.pdf");

        Assert.False(result);
    }

    [Fact]
    public async Task SaveDocumentToTemporaryBlobAsync_CallsCreateIfNotExistsAsync_BeforeUploading()
    {
        var callOrder = new List<string>();

        _blobContainerClientMock
            .Setup(c => c.CreateIfNotExistsAsync(
                It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobContainerEncryptionScopeOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("create"))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());

        _blobClientMock
            .Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, bool, CancellationToken>((_, _, _) => callOrder.Add("upload"))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        await _service.SaveDocumentToTemporaryBlobAsync(new MemoryStream([1, 2, 3]), "document.pdf");

        Assert.Equal(["create", "upload"], callOrder);
    }

    [Fact]
    public async Task SaveDocumentToTemporaryBlobAsync_UsesCorrectContainerName()
    {
        _blobClientMock
            .Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        await _service.SaveDocumentToTemporaryBlobAsync(new MemoryStream([1, 2, 3]), "document.pdf");

        _blobServiceClientMock.Verify(s => s.GetBlobContainerClient(BlobContainerName), Times.Once);
    }

    [Fact]
    public async Task ConvertToPdfAsync_ReturnsNull_WhenFileNameIsNull()
    {
        var result = await _service.ConvertToPdfAsync(null!);

        Assert.Null(result);
    }

    [Fact]
    public async Task ConvertToPdfAsync_ThrowsNotSupportedException_ForUnsupportedMimeType()
    {
        // .mp4 resolves to video/mp4, which is not handled by the conversion switch
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _service.ConvertToPdfAsync("video.mp4"));
    }

    [Fact]
    public async Task ConvertToPdfAsync_ReturnsNull_WhenBlobContainerNameNotSet()
    {
        // InvalidOperationException from missing env var is caught by ConvertWithLoggingAsync
        Environment.SetEnvironmentVariable("BlobContainerName", null);

        var result = await _service.ConvertToPdfAsync("document.docx");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("document.docx")]
    [InlineData("document.doc")]
    [InlineData("document.dotx")]
    [InlineData("document.dotm")]
    [InlineData("document.docm")]
    public async Task ConvertToPdfAsync_ReturnsNull_WhenConversionEncountersAnError(string fileName)
    {
        // DownloadToAsync writes nothing, leaving an empty stream that Aspose cannot parse.
        // The resulting exception is caught by ConvertWithLoggingAsync and surfaced as null.
        _blobClientMock
            .Setup(c => c.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response>());

        var result = await _service.ConvertToPdfAsync(fileName);

        Assert.Null(result);
    }
}
