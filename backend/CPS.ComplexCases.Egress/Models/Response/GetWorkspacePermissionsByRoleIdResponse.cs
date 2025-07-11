using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;

class GetWorkspacePermissionsByRoleIdResponse
{
    [JsonPropertyName("role_id")]
    public string? RoleId { get; set; }
    [JsonPropertyName("file_permissions")]
    public IEnumerable<string>? FilePermissions { get; set; }
}