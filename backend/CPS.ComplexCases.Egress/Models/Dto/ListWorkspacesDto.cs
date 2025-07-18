using CPS.ComplexCases.Common.Models.Domain.Dto;

namespace CPS.ComplexCases.Egress.Models.Dto;

public class ListWorkspacesDto
{
  public required IEnumerable<ListWorkspaceDataDto> Data { get; set; }
  public required PaginationDto Pagination { get; set; }
}


public class ListWorkspaceDataDto
{
  public required string Id { get; set; }
  public required string Name { get; set; }
  public string? DateCreated { get; set; }
}