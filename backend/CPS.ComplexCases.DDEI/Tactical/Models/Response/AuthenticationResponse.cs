namespace CPS.ComplexCases.DDEI.Tactical.Models.Response;

public class AuthenticationResponse
{
    public required string Cookies { get; set; }
    public required string Token { get; set; }
}