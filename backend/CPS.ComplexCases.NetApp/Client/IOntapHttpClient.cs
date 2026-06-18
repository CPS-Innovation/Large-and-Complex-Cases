using Microsoft.AspNetCore.Mvc;

namespace CPS.ComplexCases.NetApp.Client;

public interface IOntapHttpClient
{
    Task<IActionResult> RenameMaterialAsync(string bearerToken, Guid ontapVolumeUuid, string currentFolderPath, string newFolderPath);
}