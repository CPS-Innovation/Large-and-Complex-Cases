namespace CPS.ComplexCases.Data.Dtos;

public class ActivityLogFilterDto
{
    public int? CaseId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Username { get; set; }
    public string? ActionType { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 100;
}

