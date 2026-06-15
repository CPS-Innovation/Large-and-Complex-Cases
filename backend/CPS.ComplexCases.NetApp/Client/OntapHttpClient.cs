using System.Net;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.NetApp.Client;

public class OntapHttpClient(
    HttpClient httpClient,
    IOntapArgFactory ontapArgFactory,
    IOntapRequestFactory ontapRequestFactory) : IOntapHttpClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IOntapArgFactory _ontapArgFactory = ontapArgFactory;
    private readonly IOntapRequestFactory _ontapRequestFactory = ontapRequestFactory;

    public async Task<bool> RenameMaterialAsync(string bearerToken, Guid ontapVolumeUuid, string currentFolderPath, string newFolderPath)
    {
        var request = _ontapRequestFactory.CreateRenameMaterialRequest(_ontapArgFactory.CreateMaterialRenameArg(bearerToken, ontapVolumeUuid, currentFolderPath, newFolderPath));
        var response = await CallOntap(request);
        return response.IsSuccessStatusCode;
    }

    private async Task<HttpResponseMessage> CallOntap(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        var response = await _httpClient.SendAsync(request);
        try
        {
            if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
            {
                return response;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new NetAppUnauthorizedException(response.ReasonPhrase ?? "Unauthorized access to ONTAP.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NetAppNotFoundException(response.ReasonPhrase ?? "User not found.");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new NetAppConflictException(response.ReasonPhrase ?? "Conflict occurred while accessing ONTAP API.");
            }

            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(content);
        }
        catch (HttpRequestException exception)
        {
            throw new NetAppClientException(response.StatusCode, exception);
        }
    }
}