namespace CPS.ComplexCases.NetApp.Models.Args;

public class InitiateMultipartUploadArg : BaseNetAppArg
{
    public required string ObjectKey { get; set; }
}
