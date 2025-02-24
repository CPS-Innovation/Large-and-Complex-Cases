namespace CPS.ComplexCases.API.Validators;

public class ValidateTokenResult
{
  public bool IsValid { get; set; }
  public string? Username { get; set; }
  public string? Token { get; set; }
}
