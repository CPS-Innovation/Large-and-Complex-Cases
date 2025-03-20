using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Tactical.Factories;

namespace CPS.ComplexCases.DDEI.Factories;

public class DdeiRequestFactoryTactical(IMockSwitch mockSwitch) : IDdeiRequestFactoryTactical
{
  private readonly IMockSwitch _mockSwitch = mockSwitch;

  public HttpRequestMessage CreateAuthenticateRequest(string username, string password)
  {
    return new HttpRequestMessage(HttpMethod.Post, _mockSwitch.SwitchPathIfMockUser(username, $"api/authenticate"))
    {
      Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new ("username", username),
                    new ("password", password)
                })
    };
  }
}