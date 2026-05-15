using CPS.ComplexCases.Common.Helpers;

namespace CPS.ComplexCases.Common.Tests.Unit;

public class MimeTypeHelperTests
{
    [Fact]
    public void GetMimeType_ReturnsNull_WhenFilePathIsNull()
    {
        var result = MimeTypeHelper.GetMimeType(null);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("document.doc", "application/msword")]
    [InlineData("spreadsheet.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("spreadsheet.xls", "application/vnd.ms-excel")]
    [InlineData("presentation.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData("presentation.ppt", "application/vnd.ms-powerpoint")]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("image.jpeg", "image/jpeg")]
    [InlineData("image.png", "image/png")]
    [InlineData("image.gif", "image/gif")]
    [InlineData("image.tiff", "image/tiff")]
    [InlineData("image.bmp", "image/bmp")]
    [InlineData("data.csv", "text/csv")]
    [InlineData("notes.txt", "text/plain")]
    [InlineData("email.msg", "application/vnd.ms-outlook")]
    [InlineData("data.xml", "text/xml")]
    [InlineData("document.rtf", "application/rtf")]
    public void GetMimeType_ReturnsCorrectMimeType_ForKnownExtension(string fileName, string expectedMimeType)
    {
        var result = MimeTypeHelper.GetMimeType(fileName);

        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData("DOCUMENT.PDF", "application/pdf")]
    [InlineData("Report.DOCX", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void GetMimeType_IsCaseInsensitive_ForExtension(string fileName, string expectedMimeType)
    {
        var result = MimeTypeHelper.GetMimeType(fileName);

        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData("/full/path/to/document.pdf", "application/pdf")]
    [InlineData("subdir/report.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void GetMimeType_WorksWithFilePaths_NotJustFileNames(string filePath, string expectedMimeType)
    {
        var result = MimeTypeHelper.GetMimeType(filePath);

        Assert.Equal(expectedMimeType, result);
    }

    [Fact]
    public void GetMimeType_ReturnsApplicationOctetStream_ForUnknownExtension()
    {
        var result = MimeTypeHelper.GetMimeType("file.unknownextension123");

        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetMimeType_ReturnsApplicationOctetStream_ForEmptyString()
    {
        var result = MimeTypeHelper.GetMimeType(string.Empty);

        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetExtensionFromMimeType_ReturnsNull_WhenMimeTypeIsNull()
    {
        var result = MimeTypeHelper.GetExtensionFromMimeType(null);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("application/pdf", ".pdf")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx")]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("text/plain", ".txt")]
    public void GetExtensionFromMimeType_ReturnsExtension_ForKnownMimeType(string mimeType, string expectedExtension)
    {
        var result = MimeTypeHelper.GetExtensionFromMimeType(mimeType);

        Assert.Equal(expectedExtension, result);
    }

    [Fact]
    public void GetExtensionFromMimeType_ReturnsNull_ForUnknownMimeType()
    {
        var result = MimeTypeHelper.GetExtensionFromMimeType("application/x-completely-unknown-type-xyz");

        Assert.Null(result);
    }
}
