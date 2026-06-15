namespace CPS.ComplexCases.NetApp.Client;

public interface IOntapHttpClient
{
    Task<bool> RenameMaterialAsync(string bearerToken, Guid ontapVolumeUuid, string currentFolderPath, string newFolderPath);
}