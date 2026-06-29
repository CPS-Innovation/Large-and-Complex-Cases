using Microsoft.AspNetCore.Mvc;

namespace CPS.ComplexCases.API.Services;

public interface IDocumentService
{
    Task<FileStreamResult?> GetMaterialPreviewAsync(string path, string bearerToken, string bucketName);
}
