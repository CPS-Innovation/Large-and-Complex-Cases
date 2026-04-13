namespace CPS.ComplexCases.NetApp.Models.Args;

public class AbortMultipartUploadArg : BaseNetAppArg
{
    public required string ObjectKey { get; set; }
    public required string UploadId { get; set; }
}
