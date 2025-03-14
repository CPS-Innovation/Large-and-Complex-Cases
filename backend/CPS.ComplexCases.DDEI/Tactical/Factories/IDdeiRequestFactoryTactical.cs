namespace CPS.ComplexCases.DDEI.Tactical.Factories;

public interface IDdeiRequestFactoryTactical
{
  HttpRequestMessage CreateAuthenticateRequest(string username, string password);
}