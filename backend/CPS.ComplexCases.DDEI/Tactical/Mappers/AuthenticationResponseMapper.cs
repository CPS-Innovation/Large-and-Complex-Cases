

using Domain = CPS.ComplexCases.DDEI.Tactical.Models.Response;
using Dto = CPS.ComplexCases.DDEI.Tactical.Models.Dto;

namespace CPS.ComplexCases.DDEI.Tactical.Mappers;

public class AuthenticationResponseMapper : IAuthenticationResponseMapper
{
  public Domain.AuthenticationResponse Map(Dto.AuthenticationResponse response)
  {
    return new Domain.AuthenticationResponse
    {
      Cookies = response.Cookies,
      Token = response.Token
    };
  }
}