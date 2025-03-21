using CPS.ComplexCases.DDEI.Tactical.Factories;

namespace CPS.ComplexCases.DDEI.Factories;

public class DdeiRequestFactoryTactical : IDdeiRequestFactoryTactical
{
  public HttpRequestMessage CreateAuthenticateRequest(string username, string password)
  {
    return new HttpRequestMessage(HttpMethod.Post, $"api/authenticate")
    {
      Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new ("username", username),
                    new ("password", password)
                })
    };
  }
}