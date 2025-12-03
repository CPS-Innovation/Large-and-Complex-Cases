namespace CPS.ComplexCases.NetApp.Models.Args;

public abstract class BaseNetAppArg
{
    public required string BearerToken { get; set; }
    public required string BucketName { get; set; }
}