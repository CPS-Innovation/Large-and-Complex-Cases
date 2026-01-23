namespace CPS.ComplexCases.NetApp.Models.Args;

public class RegenerateUserKeysArg
{
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public Guid S3ServiceUuid { get; set; } = Guid.Empty;
    public string KeyTimeToLive { get; set; } = string.Empty;
}