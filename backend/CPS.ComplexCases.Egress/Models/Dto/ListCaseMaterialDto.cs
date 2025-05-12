namespace CPS.ComplexCases.Egress.Models.Dto;

public class ListCaseMaterialDto
{
  public required IEnumerable<ListCaseMaterialDataDto> Data { get; set; }
  public required PaginationDto Pagination { get; set; }
}
public class ListCaseMaterialDataDto
{
  public required string Id { get; set; }
  public required string Name { get; set; }
  public required string Path { get; set; }
  public DateTime? DateUpdated { get; set; }
  public bool IsFolder { get; set; }
  public int Version { get; set; }
  public long? Filesize { get; set; }
}
