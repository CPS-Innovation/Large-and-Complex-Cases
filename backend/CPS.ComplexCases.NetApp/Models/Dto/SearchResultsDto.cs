namespace CPS.ComplexCases.NetApp.Models.Dto;

public class SearchResultsDto
{
    public required IEnumerable<SearchResultItemDto> Data { get; set; }
    public bool Truncated { get; set; }
    public int TotalScanned { get; set; }
}

public class SearchResultItemDto
{
    public required string Key { get; set; }
    public required string Type { get; set; }
    public long? Size { get; set; }
    public DateTime? LastModified { get; set; }
}