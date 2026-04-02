namespace CPS.ComplexCases.NetApp.Models.Args;

public class PutObjectTaggingArg : BaseNetAppArg
{
    public required string ObjectKey { get; set; }
    public required Dictionary<string, string> Tags { get; set; }
}
