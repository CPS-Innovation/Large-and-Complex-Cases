namespace CPS.ComplexCases.NetApp.Models.Args;

public class ListFoldersInBucketArg : BaseNetAppArg
{
    public string? OperationName { get; set; }
    public string? ContinuationToken { get; set; }
    public string? MaxKeys { get; set; }
    public string? Prefix { get; set; }
}
