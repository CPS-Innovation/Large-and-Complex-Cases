namespace CPS.ComplexCases.API.Validators;

public class ValidatableRequest<T>
{
  public required T Value { get; set; }

  public bool IsValid { get; set; }
  public List<string> ValidationErrors { get; set; } = new List<string>();
}
