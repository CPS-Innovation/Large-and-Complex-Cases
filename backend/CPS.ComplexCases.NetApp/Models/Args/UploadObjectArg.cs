namespace CPS.ComplexCases.NetApp.Models.Args;

public class UploadObjectArg : BaseNetAppArg
{
    public required string ObjectKey { get; set; }
    public required Stream Stream { get; set; }
    public required long ContentLength { get; set; }
}