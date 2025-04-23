namespace CPS.ComplexCases.API.Domain.Response;

public class NetAppPaginationResponse
{
    public string? NextContinuationToken { get; set; }
    public int? MaxKeys { get; set; }
}