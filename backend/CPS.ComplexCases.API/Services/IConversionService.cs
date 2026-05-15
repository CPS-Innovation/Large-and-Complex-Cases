namespace CPS.ComplexCases.API.Services;

public interface IConversionService
{
    Task<bool> SaveDocumentToTemporaryBlobAsync(Stream documentStream, string fileName);
    Task<string?> ConvertToPdfAsync(string tmpFileName, bool firstPageOnly = true);
}
