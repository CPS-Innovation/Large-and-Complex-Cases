namespace CPS.ComplexCases.NetApp.Models.Args;

public class ListObjectsInBucketArg : BaseNetAppArg
{
    public string? ContinuationToken { get; set; }
    public string? Delimiter { get; set; }
    public string? MaxKeys { get; set; }
    public string? Prefix { get; set; }
    public bool IncludeDelimiter { get; set; } = false;
}
