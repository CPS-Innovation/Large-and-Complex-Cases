using CPS.ComplexCases.Common.Models.Domain.Dto;

namespace CPS.ComplexCases.Egress.Models.Dto;

public class ListTemplatesDto
{
    public required IEnumerable<ListTemplateDataDto> Data { get; set; }
    public required PaginationDto Pagination { get; set; }
}


public class ListTemplateDataDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
}