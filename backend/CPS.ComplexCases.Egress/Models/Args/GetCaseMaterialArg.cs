
namespace CPS.ComplexCases.Egress.Models.Args;

public class GetCaseMaterialArg : PaginationArg
{
  public required string CaseId { get; set; }
  public string? FolderId { get; set; }
}
