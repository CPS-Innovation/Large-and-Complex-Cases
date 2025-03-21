using CPS.ComplexCases.DDEI.Tactical.Models.Response;

namespace CPS.ComplexCases.DDEI.Tactical.Client;

public interface IDdeiClientTactical
{
  Task<AuthenticationResponse> AuthenticateAsync(string username, string password);
}