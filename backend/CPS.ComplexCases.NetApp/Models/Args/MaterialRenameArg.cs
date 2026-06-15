namespace CPS.ComplexCases.NetApp.Models.Args;

public class MaterialRenameArg
{
    public required string BearerToken { get; set; }
    public required Guid OntapVolumeUuid { get; set; }
    public required string CurrentFolderPath { get; set; }
    public required string NewFolderPath { get; set; }
}