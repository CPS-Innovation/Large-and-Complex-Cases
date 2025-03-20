namespace CPS.ComplexCases.DDEI;

public class DDEIOptions
{
    public const string FunctionKey = "X-Functions-Key";
    public const string DevtunnelUrlFragment = "devtunnels.ms";
    public const string DevtunnelTokenKey = "X-Tunnel-Authorization";
    public required string DevtunnelToken { get; set; }
    public required string BaseUrl { get; set; }
    public required string AccessKey { get; set; }
    public required string MockBaseUrl { get; set; }
}