namespace CPS.ComplexCases.NetApp.Models.Dto;

public class DataInfoDto
{
    public string? ContinuationToken { get; set; }
    public string? NextContinuationToken { get; set; }
    public int? MaxKeys { get; set; }
    public int KeyCount { get; set; }
}