namespace CPS.ComplexCases.Egress.Models.Dto;

public class GetCaseMaterialDto
{
  public required IEnumerable<GetCaseMaterialDataDto> Data { get; set; }
  public required PaginationDto Pagination { get; set; }
}
public class GetCaseMaterialDataDto
{
  public required string Id { get; set; }
  public required string FileName { get; set; }
  public required string Path { get; set; }
  public DateTime? DateUpdated { get; set; }
  public bool IsFolder { get; set; }
  public int Version { get; set; }
}
