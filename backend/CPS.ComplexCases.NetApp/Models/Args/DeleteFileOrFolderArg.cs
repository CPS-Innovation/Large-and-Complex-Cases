namespace CPS.ComplexCases.NetApp.Models.Args;

public class DeleteFileOrFolderArg : BaseNetAppArg
{
    public required string OperationName { get; set; }
    public required string Path { get; set; }
    public bool IsFolder { get; set; }
}