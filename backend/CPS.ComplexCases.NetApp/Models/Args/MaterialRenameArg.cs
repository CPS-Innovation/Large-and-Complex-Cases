namespace CPS.ComplexCases.NetApp.Models.Args;

public class MaterialRenameArg
{
    public required string BearerToken { get; set; }
    public required Guid OntapVolumeUuid { get; set; }
    public required string CurrentFilePath { get; set; }
    public required string NewFilePath { get; set; }
}