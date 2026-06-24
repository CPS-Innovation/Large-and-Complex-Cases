namespace CPS.ComplexCases.NetApp.Models.Args;

public class GetCifsSessionUserArg
{
    public required string BearerToken { get; set; }
    public required string ClientIp { get; set; }
}