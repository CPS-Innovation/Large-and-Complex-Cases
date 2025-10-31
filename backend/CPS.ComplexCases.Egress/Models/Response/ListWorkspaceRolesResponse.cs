using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class ListWorkspaceRolesResponse
{
    [JsonPropertyName("data")]
    public required IEnumerable<WorkspaceRole> Data { get; set; }
    [JsonPropertyName("data_info")]
    public required DataInfoResponse DataInfo { get; set; }
}

public class WorkspaceRole
{
    [JsonPropertyName("role_id")]
    public required string RoleId { get; set; }
    [JsonPropertyName("role_name")]
    public required string RoleName { get; set; }
}