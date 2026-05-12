using Azure.Storage.Blobs;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Streams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Services;

public class DocumentService(
    ILogger<DocumentService> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IConversionService conversionService,
    BlobServiceClient blobServiceClient) : IDocumentService
{
    private readonly ILogger<DocumentService> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IConversionService _conversionService = conversionService;
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    public async Task<FileStreamResult?> GetMaterialPreviewAsync(string path, string bearerToken, string bucketName)
    {
        var fileName = Path.GetFileName(path);
        string? tmpFileName = null;

        try
        {
            _logger.LogInformation("Downloading document from NetApp at path [{Path}]", path);

            var arg = _netAppArgFactory.CreateGetObjectArg(bearerToken, bucketName, path);
            var netAppResponse = await _netAppClient.GetObjectAsync(arg);

            if (netAppResponse?.ResponseStream == null)
            {
                _logger.LogWarning("No document found in NetApp at path [{Path}]", path);
                return null;
            }

            var contentType = MimeTypeHelper.GetMimeType(fileName);
            if (contentType == null)
            {
                throw new NotSupportedException($"Unsupported file type for [{fileName}]");
            }

            tmpFileName = fileName;

            using var responseStream = new HashValidationIgnoringStream(netAppResponse.ResponseStream);
            var saved = await _conversionService.SaveDocumentToTemporaryBlobAsync(responseStream, tmpFileName);
            if (!saved)
            {
                throw new InvalidOperationException($"Failed to save document [{tmpFileName}] to temporary blob storage");
            }

            var pdfBlobUrl = await _conversionService.ConvertToPdfAsync(tmpFileName);
            if (pdfBlobUrl == null)
            {
                throw new InvalidOperationException($"Failed to convert [{tmpFileName}] to PDF");
            }

            var blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName")
                ?? throw new InvalidOperationException("BlobContainerName environment variable is not set.");

            var pdfStream = new MemoryStream();
            var pdfBlobClient = _blobServiceClient
                .GetBlobContainerClient(blobContainerName)
                .GetBlobClient($"preview_{tmpFileName}.pdf");

            await pdfBlobClient.DownloadToAsync(pdfStream);
            pdfStream.Position = 0;

            _logger.LogInformation("Successfully generated preview PDF for [{Path}]", path);

            return new FileStreamResult(pdfStream, "application/pdf")
            {
                FileDownloadName = $"{tmpFileName}.pdf"
            };
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("No document found in NetApp at path [{Path}]", path);
            return null;
        }
        catch (NotSupportedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating material preview for [{Path}]", path);
            throw;
        }
        finally
        {
            if (tmpFileName != null)
            {
                await DeleteTemporaryBlobsAsync(tmpFileName);
            }
        }
    }

    private async Task DeleteTemporaryBlobsAsync(string tmpFileName)
    {
        var blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName");
        if (string.IsNullOrEmpty(blobContainerName))
            return;

        var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);

        await DeleteBlobIfExistsAsync(containerClient, $"tmp_{tmpFileName}");
        await DeleteBlobIfExistsAsync(containerClient, $"preview_{tmpFileName}.pdf");
    }

    private async Task DeleteBlobIfExistsAsync(BlobContainerClient containerClient, string blobName)
    {
        try
        {
            await containerClient.GetBlobClient(blobName).DeleteIfExistsAsync();
            _logger.LogInformation("Deleted temporary blob [{BlobName}]", blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting temporary blob [{BlobName}]", blobName);
        }
    }
}
