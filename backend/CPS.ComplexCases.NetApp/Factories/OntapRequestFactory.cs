using System.Text;
using System.Text.Json;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;

namespace CPS.ComplexCases.NetApp.Factories;

public class OntapRequestFactory : IOntapRequestFactory
{

    public HttpRequestMessage CreateRenameMaterialRequest(MaterialRenameArg arg)
    {
        var encodedCurrentPath = Uri.EscapeDataString(arg.CurrentFilePath.RemoveTrailingSlash());
        var url = $"/api/storage/volumes/{arg.OntapVolumeUuid}/files/{encodedCurrentPath}";

        var payload = new MaterialRenameDto
        {
            Path = arg.NewFilePath.RemoveTrailingSlash()
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", arg.BearerToken);

        return request;
    }

    public HttpRequestMessage CreateGetFileLockRequest(GetFileLockArg arg)
    {
        var filePath = "/FlexGroup-Vol04/" + arg.FilePath;
        var encodedFilePath = Uri.EscapeDataString(filePath.RemoveTrailingSlash());
        var returnTimeout = 15; // The number of seconds to allow the call to execute before returning. When iterating over a collection, the default is 15 seconds. ONTAP returns earlier if either max records or the end of the collection is reached. Default value: 15, Max value: 120, Min value: 0
        var fields = "client_address,type"; // The fields to return in the response.
        var url = $"/api/protocols/locks/?volume.uuid={arg.VolumeUuid}&return_timeout={returnTimeout}&fields={fields}&path={encodedFilePath}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", arg.BearerToken);

        return request;
    }

    public HttpRequestMessage CreateGetCifsSessionUserRequest(GetCifsSessionUserArg arg)
    {
        var fields = "user"; // The fields to return in the response.
        var url = $"/api/protocols/cifs/sessions/?client_ip={arg.ClientIp}&fields={fields}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", arg.BearerToken);

        return request;
    }
}