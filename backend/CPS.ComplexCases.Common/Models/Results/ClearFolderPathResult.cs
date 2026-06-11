using CPS.ComplexCases.Common.Enums;

namespace CPS.ComplexCases.Common.Models.Results;

public class ClearFolderPathResult
{
    public CaseMetadataState State { get; set; }
    public string? ClearedPath { get; set; }
    public string? Key { get; set; }
}