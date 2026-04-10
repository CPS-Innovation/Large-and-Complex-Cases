using CPS.ComplexCases.NetApp.Enums;

namespace CPS.ComplexCases.NetApp.Models.Args;

public class SearchArg : BaseNetAppArg
{
    public required string OperationName { get; set; }
    public string? Query { get; set; }
    public int MaxResults { get; set; }
    public SearchModes Mode { get; set; } = SearchModes.Prefix;
}