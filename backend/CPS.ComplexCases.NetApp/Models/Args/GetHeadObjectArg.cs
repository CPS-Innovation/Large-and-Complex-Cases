namespace CPS.ComplexCases.NetApp.Models.Args;

public class GetHeadObjectArg : BaseNetAppArg
{
    public required string ObjectKey { get; set; }
}