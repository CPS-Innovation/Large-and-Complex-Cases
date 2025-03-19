namespace CPS.ComplexCases.DDEI.Models.Args;

public class DdeiDefendantNameArgDto : DdeiBaseArgDto
{
  public string? FirstName { get; set; }
  public required string LastName { get; set; }
  public required string CmsAreaCode { get; set; }
}