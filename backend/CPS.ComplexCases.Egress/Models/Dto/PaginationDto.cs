namespace CPS.ComplexCases.Egress.Models.Dto;

public class PaginationDto
{
  public int Count { get; set; }
  public int Take { get; set; }
  public int Skip { get; set; }
  public int TotalResults { get; set; }
}
