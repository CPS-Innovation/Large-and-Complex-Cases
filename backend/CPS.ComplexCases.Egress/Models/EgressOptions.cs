namespace CPS.ComplexCases.Egress.Models;

public class EgressOptions
{
  public const string ConfigKey = "Egress";
  public required string Username { get; set; }
  public required string Password { get; set; }
  public required string Url { get; set; }
}
