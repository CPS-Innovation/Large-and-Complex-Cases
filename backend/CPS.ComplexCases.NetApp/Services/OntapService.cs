using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Services;

public class OntapService(IOntapHttpClient ontapHttpClient, IOntapArgFactory ontapArgFactory) : IOntapService
{
    private readonly IOntapHttpClient _ontapHttpClient = ontapHttpClient;
    private readonly IOntapArgFactory _ontapArgFactory = ontapArgFactory;

    public async Task<GetFileLockResult> GetFileLockAsync(GetFileLockArg arg)
    {
        var fileLockResults = await _ontapHttpClient.GetFileLockAsync(arg);

        if (fileLockResults.StatusCode == System.Net.HttpStatusCode.OK)
        {
            if (fileLockResults.Records is null || !fileLockResults.Records.Any())
            {
                return new GetFileLockResult(false, null, null, "No records found in the response.");
            }

            var records = fileLockResults.Records;
            var clientIp = records.First().ClientAddress;

            if (string.IsNullOrEmpty(clientIp))
            {
                return new GetFileLockResult(false, null, null, "Client IP is missing in the response.");
            }

            var userResult = await _ontapHttpClient.GetCifsSessionUserAsync(_ontapArgFactory.CreateGetCifsSessionUserArg(arg.BearerToken, clientIp));

            if (userResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new GetFileLockResult(false, null, null, $"Failed to retrieve CIFS session user. Status code: {userResult.StatusCode}");
            }

            var types = records
                .Where(r => r.LockType != null)
                .Select(r => r.LockType!.ToString())
                .Distinct()
                .ToArray();

            return new GetFileLockResult(true, userResult.Records?.FirstOrDefault()?.User, types, "File lock retrieved successfully.");
        }
        else if (fileLockResults.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new GetFileLockResult(false, null, null, "File not found.");
        }
        else
        {
            return new GetFileLockResult(false, null, null, $"Unexpected status code: {fileLockResults.StatusCode}");
        }
    }
}