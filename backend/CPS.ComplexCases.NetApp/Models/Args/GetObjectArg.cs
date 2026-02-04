namespace CPS.ComplexCases.NetApp.Models.Args;

public class GetObjectArg : BaseNetAppArg
{
    public required string ObjectKey { get; set; }
    public string? ETag { get; set; }
}
