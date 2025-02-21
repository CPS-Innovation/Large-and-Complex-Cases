namespace CPS.ComplexCases.Egress.Models.Dto;

public class PaginationDto
{
  public int CurrentPage { get; set; }
  public int PerPage { get; set; }
  public int TotalPages { get; set; }
  public int TotalResults { get; set; }
}
