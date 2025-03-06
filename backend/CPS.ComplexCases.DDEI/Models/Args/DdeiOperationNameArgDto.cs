namespace CPS.ComplexCases.DDEI.Models.Args;

public class DdeiOperationNameArgDto : DdeiBaseArgDto
{
  public required string OperationName { get; set; }
  public required string CmsAreaCode { get; set; }
}