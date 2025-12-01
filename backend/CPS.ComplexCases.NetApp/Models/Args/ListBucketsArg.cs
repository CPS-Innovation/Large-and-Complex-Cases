namespace CPS.ComplexCases.NetApp.Models.Args;

public class ListBucketsArg : BaseNetAppArg
{
    public string? ContinuationToken { get; set; }
    public int? MaxBuckets { get; set; }
    public string? Prefix { get; set; }
}
