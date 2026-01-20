namespace CPS.ComplexCases.NetApp.Models.Args;

public class CreateFolderArg : BaseNetAppArg
{
    public required string FolderKey { get; set; }
}