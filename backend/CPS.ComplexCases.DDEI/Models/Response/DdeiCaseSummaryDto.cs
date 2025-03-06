namespace CPS.ComplexCases.DDEI.Models.Response;

public class DdeiCaseSummaryDto
{
  public required string Urn { get; set; }
  public int Id { get; set; }
  public string? LeadDefendantFirstNames { get; set; }
  public string? LeadDefendantSurname { get; set; }
}