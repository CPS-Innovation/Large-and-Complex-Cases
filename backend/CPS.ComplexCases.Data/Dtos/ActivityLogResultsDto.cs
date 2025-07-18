using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Data.Dtos;

public class ActivityLogResultsDto
{
    public IEnumerable<ActivityLog> Logs { get; set; } = [];
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}