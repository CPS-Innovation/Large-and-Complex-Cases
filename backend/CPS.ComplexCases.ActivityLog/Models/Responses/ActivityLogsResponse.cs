using CPS.ComplexCases.Common.Models.Domain.Dto;

namespace CPS.ComplexCases.ActivityLog.Models.Responses;

public class ActivityLogsResponse
{
    public IEnumerable<Data.Entities.ActivityLog> Data { get; set; } = [];
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}