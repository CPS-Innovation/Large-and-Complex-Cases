using CPS.ComplexCases.NetApp.Enums;

namespace CPS.ComplexCases.NetApp.Models.Requests;

public class SearchNetAppFoldersDto
{
    public int CaseId { get; set; }
    public string? Query { get; set; } = string.Empty;
    public SearchModes Mode { get; set; } = SearchModes.Prefix;
    public int MaxResults { get; set; } = 1000;
}