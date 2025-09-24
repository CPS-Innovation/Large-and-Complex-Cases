namespace CPS.ComplexCases.API.Domain.Response;

public class InitResult
{
    public InitResultStatus Status { get; set; }
    public string? Message { get; set; }
    public string? RedirectUrl { get; set; }
    public bool ShouldSetCookie { get; set; }
    public string? Cc { get; set; }
    public string? Ct { get; set; }
}