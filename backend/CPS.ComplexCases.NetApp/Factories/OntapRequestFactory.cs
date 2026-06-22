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
}