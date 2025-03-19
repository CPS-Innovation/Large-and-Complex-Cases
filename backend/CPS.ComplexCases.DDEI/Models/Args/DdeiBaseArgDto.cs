namespace CPS.ComplexCases.DDEI.Models.Args;

public class DdeiBaseArgDto
{
  public required string CmsAuthValues { get; set; }
  public Guid CorrelationId { get; set; }
}