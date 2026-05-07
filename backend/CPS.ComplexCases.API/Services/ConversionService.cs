using System.Globalization;
using Aspose.Email.Mapi;
using Aspose.Pdf.Facades;
using Azure.Storage.Blobs;
using CPS.ComplexCases.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Services;

public class ConversionService(ILogger<ConversionService> logger, BlobServiceClient blobServiceClient) : IConversionService
{
    private readonly ILogger<ConversionService> _logger = logger;
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    public async Task<bool> SaveDocumentToTemporaryBlobAsync(Stream documentStream, string fileName)
    {
        var blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName");
        if (string.IsNullOrEmpty(blobContainerName))
        {
            _logger.LogError("BlobContainerName environment variable is not set.");
            return false;
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient($"tmp_{fileName}");

        try
        {
            using var memoryStream = new MemoryStream();
            await documentStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await blobClient.UploadAsync(memoryStream, overwrite: true);

            _logger.LogInformation("Document saved to temporary blob storage as [tmp_{FileName}]", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving document to temporary blob storage as [tmp_{FileName}]", fileName);
            return false;
        }
    }

    public async Task<string?> ConvertToPdfAsync(string tmpFileName, bool firstPageOnly = true)
    {
        var contentType = MimeTypeHelper.GetMimeType(tmpFileName);

        if (contentType == null)
        {
            _logger.LogWarning("Unable to determine MIME type for [{FileName}]", tmpFileName);
            return null;
        }

        try
        {
            return contentType switch
            {
                var t when IsRasterImage(t) => await ConvertWithLoggingAsync(() => ConvertRasterImageToPdfAsync(tmpFileName), "raster image"),
                "application/msword" => await ConvertWithLoggingAsync(() => ConvertDocToPdfAsync(tmpFileName, firstPageOnly), "DOC/DOT"),
                "application/octet-stream" => await ConvertWithLoggingAsync(() => ConvertHtmlToPdfAsync(tmpFileName), "HTML"),
                "application/pdf" => await ConvertWithLoggingAsync(() => ConvertPdfToPdfAsync(tmpFileName, firstPageOnly), "PDF"),
                "application/rtf" => await ConvertWithLoggingAsync(() => ConvertDocToPdfAsync(tmpFileName), "RTF"),
                "application/vnd.ms-excel" => await ConvertWithLoggingAsync(() => ConvertXlsToPdfAsync(tmpFileName, firstPageOnly), "XLS"),
                "application/vnd.ms-excel.sheet.macroEnabled.12" => await ConvertWithLoggingAsync(() => ConvertXlsToPdfAsync(tmpFileName, firstPageOnly), "XLSM"),
                "application/vnd.ms-outlook" => await ConvertWithLoggingAsync(() => ConvertMsgToPdfAsync(tmpFileName), "MSG"),
                "application/vnd.ms-powerpoint" => await ConvertWithLoggingAsync(() => ConvertPptToPdfAsync(tmpFileName, firstPageOnly), "PPT"),
                "application/vnd.ms-word.document.macroEnabled.12" => await ConvertWithLoggingAsync(() => ConvertDocToPdfAsync(tmpFileName, firstPageOnly), "DOCM"),
                "application/vnd.ms-word.template.macroEnabled.12" => await ConvertWithLoggingAsync(() => ConvertDocToPdfAsync(tmpFileName, firstPageOnly), "DOTM"),
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" => await ConvertWithLoggingAsync(() => ConvertPptToPdfAsync(tmpFileName, firstPageOnly), "PPTX"),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => await ConvertWithLoggingAsync(() => ConvertXlsToPdfAsync(tmpFileName, firstPageOnly), "XLSX"),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => await ConvertWithLoggingAsync(() => ConvertDocToPdfAsync(tmpFileName, firstPageOnly), "DOCX"),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.template" => await ConvertWithLoggingAsync(() => ConvertDocToPdfAsync(tmpFileName, firstPageOnly), "DOTX"),
                "application/xml" => await ConvertWithLoggingAsync(() => ConvertXmlToPdfAsync(tmpFileName), "XML"),
                "text/csv" => await ConvertWithLoggingAsync(() => ConvertXlsToPdfAsync(tmpFileName, firstPageOnly), "CSV"),
                "text/plain" => await ConvertWithLoggingAsync(() => ConvertTxtToPdfAsync(tmpFileName), "TXT"),
                "text/xml" => await ConvertWithLoggingAsync(() => ConvertXmlToPdfAsync(tmpFileName), "XML"),
                _ => throw new NotSupportedException($"Unsupported content type: {contentType}")
            };
        }
        catch (NotSupportedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting [{FileName}] to PDF", tmpFileName);
            return null;
        }
    }

    private async Task<string?> ConvertRasterImageToPdfAsync(string tmpFileName)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var imageStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(imageStream);
        imageStream.Position = 0;

        using var image = Aspose.Imaging.Image.Load(imageStream);
        using var pdfStream = new MemoryStream();
        image.Save(pdfStream, new Aspose.Imaging.ImageOptions.PdfOptions
        {
            PdfDocumentInfo = new Aspose.Imaging.FileFormats.Pdf.PdfDocumentInfo()
        });
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        _logger.LogInformation("Converted raster image to PDF and saved as [{PdfBlobName}]", pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertPdfToPdfAsync(string tmpFileName, bool firstPageOnly = true)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var originalStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(originalStream);
        originalStream.Position = 0;

        var document = new Aspose.Pdf.Document(originalStream);
        var pdfEditor = new PdfFileEditor();

        var pagesToExtract = firstPageOnly
            ? new[] { 1 }
            : Enumerable.Range(1, document.Pages.Count).ToArray();

        using var extractedStream = new MemoryStream();
        pdfEditor.Extract(originalStream, pagesToExtract, extractedStream);
        extractedStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(extractedStream, overwrite: true);

        var pageCount = firstPageOnly ? "1" : document.Pages.Count.ToString(CultureInfo.InvariantCulture);
        _logger.LogInformation("Extracted {PageCount} page(s) from PDF and saved as [{PdfBlobName}]", pageCount, pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertDocToPdfAsync(string tmpFileName, bool firstPageOnly = true)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var docStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(docStream);
        docStream.Position = 0;

        var document = new Aspose.Words.Document(docStream);

        if (document.PageCount == 0)
        {
            _logger.LogWarning("Document [{FileName}] contains no pages", tmpFileName);
            return null;
        }

        var convertAllPages = !firstPageOnly && document.PageCount > 1;
        var wordDocument = convertAllPages
            ? document.ExtractPages(0, document.PageCount)
            : document.ExtractPages(0, 1);

        using var pdfStream = new MemoryStream();
        wordDocument.Save(pdfStream, Aspose.Words.SaveFormat.Pdf);
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        var pageLabel = !convertAllPages ? "(first page)" : string.Empty;
        _logger.LogInformation("Converted DOC to PDF {PageLabel} and saved as [{PdfBlobName}]", pageLabel, pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertXlsToPdfAsync(string tmpFileName, bool firstPageOnly = true)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var xlsStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(xlsStream);
        xlsStream.Position = 0;

        var extension = Path.GetExtension(tmpFileName).TrimStart('.').ToUpperInvariant();
        var loadFormat = extension switch
        {
            "CSV" => Aspose.Cells.LoadFormat.Csv,
            "XLS" => Aspose.Cells.LoadFormat.Excel97To2003,
            "XLSX" => Aspose.Cells.LoadFormat.Xlsx,
            _ => Aspose.Cells.LoadFormat.Auto
        };
        var workbook = new Aspose.Cells.Workbook(xlsStream, new Aspose.Cells.LoadOptions(loadFormat));
        var convertAllSheets = !firstPageOnly && workbook.Worksheets.Count > 1;

        for (int i = 0; i < workbook.Worksheets.Count; i++)
        {
            workbook.Worksheets[i].IsVisible = convertAllSheets || i == 0;
        }

        using var pdfStream = new MemoryStream();
        workbook.Save(pdfStream, new Aspose.Cells.PdfSaveOptions());
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        var sheetLabel = !convertAllSheets ? "(first sheet)" : string.Empty;
        _logger.LogInformation("Converted XLS to PDF {SheetLabel} and saved as [{PdfBlobName}]", sheetLabel, pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertPptToPdfAsync(string tmpFileName, bool firstPageOnly = true)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var pptStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(pptStream);
        pptStream.Position = 0;

        using var presentation = new Aspose.Slides.Presentation(pptStream);
        using var slidePresentation = new Aspose.Slides.Presentation();

        var convertAllSlides = !firstPageOnly && presentation.Slides.Count > 1;

        if (presentation.Slides.Count > 0)
        {
            if (convertAllSlides)
            {
                foreach (var slide in presentation.Slides)
                    slidePresentation.Slides.AddClone(slide);
            }
            else
            {
                slidePresentation.Slides.AddClone(presentation.Slides[0]);
            }
        }

        using var pdfStream = new MemoryStream();
        slidePresentation.Save(pdfStream, Aspose.Slides.Export.SaveFormat.Pdf);
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        var slideLabel = !convertAllSlides ? "(first slide)" : string.Empty;
        _logger.LogInformation("Converted PPT to PDF {SlideLabel} and saved as [{PdfBlobName}]", slideLabel, pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertMsgToPdfAsync(string tmpFileName)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var msgStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(msgStream);
        msgStream.Position = 0;

        var mapiMessage = MapiMessage.Load(msgStream);

        using var pdfStream = new MemoryStream();
        var doc = new Aspose.Words.Document(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(mapiMessage.BodyHtml)));
        doc.Save(pdfStream, Aspose.Words.SaveFormat.Pdf);
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        _logger.LogInformation("Converted MSG to PDF and saved as [{PdfBlobName}]", pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertHtmlToPdfAsync(string tmpFileName)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var htmlStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(htmlStream);
        htmlStream.Position = 0;

        using var pdfStream = new MemoryStream();
        using var pdfDocument = new Aspose.Pdf.Document(htmlStream, new Aspose.Pdf.HtmlLoadOptions());
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        _logger.LogInformation("Converted HTML to PDF and saved as [{PdfBlobName}]", pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertTxtToPdfAsync(string tmpFileName)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var txtStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(txtStream);
        txtStream.Position = 0;

        string content;
        using (var reader = new StreamReader(txtStream))
            content = await reader.ReadToEndAsync();

        var pdfDocument = new Aspose.Pdf.Document();
        var page = pdfDocument.Pages.Add();
        page.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(content));

        using var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        _logger.LogInformation("Converted TXT to PDF and saved as [{PdfBlobName}]", pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<string?> ConvertXmlToPdfAsync(string tmpFileName)
    {
        var pdfBlobName = $"preview_{tmpFileName}.pdf";
        var containerClient = await GetBlobContainerClientAsync();
        var sourceBlob = containerClient.GetBlobClient($"tmp_{tmpFileName}");

        using var xmlStream = new MemoryStream();
        await sourceBlob.DownloadToAsync(xmlStream);
        xmlStream.Position = 0;

        string content;
        using (var reader = new StreamReader(xmlStream))
            content = await reader.ReadToEndAsync();

        var pdfDocument = new Aspose.Pdf.Document();
        var page = pdfDocument.Pages.Add();
        var fragment = new Aspose.Pdf.Text.TextFragment(content);
        fragment.TextState.FontSize = 12;
        page.Paragraphs.Add(fragment);

        using var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;

        var pdfBlob = containerClient.GetBlobClient(pdfBlobName);
        await pdfBlob.UploadAsync(pdfStream, overwrite: true);

        _logger.LogInformation("Converted XML to PDF and saved as [{PdfBlobName}]", pdfBlobName);
        return pdfBlob.Uri.ToString();
    }

    private async Task<BlobContainerClient> GetBlobContainerClientAsync()
    {
        var blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName")
            ?? throw new InvalidOperationException("BlobContainerName environment variable is not set.");

        var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }

    private async Task<string?> ConvertWithLoggingAsync(Func<Task<string?>> convertFunc, string conversionType)
    {
        try
        {
            return await convertFunc();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting {ConversionType} to PDF", conversionType);
            return null;
        }
    }

    private bool IsRasterImage(string contentType)
    {
        // List of common raster image MIME types
        var rasterMimeTypes = new HashSet<string>
        {
            "image/bmp",                // BMP
            "image/jpeg",               // JPEG / JPG
            "image/png",                // PNG
            "image/gif",                // GIF
            "image/tiff",               // TIFF
            "image/x-icon",             // ICO
            "image/webp",               // WEBP
            "image/vnd.microsoft.icon", // Another ICO MIME type
            "image/x-ms-bmp",           // Another BMP MIME type
            "image/x-pcx",              // PCX
            "image/x-pict",             // PICT
            "image/x-portable-bitmap",  // PBM
            "image/x-portable-pixmap",  // PPM
            "image/x-rgb",              // RGB
            "image/x-tga",              // TGA
            "image/x-xbitmap",          // XBM
            "image/x-xpixmap",          // XPM
        };

        // Return true if the content type is in the set of raster image types
        return rasterMimeTypes.Contains(contentType.ToLower());
    }
}
