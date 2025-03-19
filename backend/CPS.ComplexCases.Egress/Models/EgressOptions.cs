namespace CPS.ComplexCases.Egress.Models;

public class EgressOptions
{
  public const string ConfigKey = "Egress";
  public string? Username { get; set; }
  public string? Password { get; set; }
  public string? Url { get; set; }
}
