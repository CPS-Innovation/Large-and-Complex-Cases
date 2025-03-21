

using Domain = CPS.ComplexCases.DDEI.Tactical.Models.Response;
using Dto = CPS.ComplexCases.DDEI.Tactical.Models.Dto;

namespace CPS.ComplexCases.DDEI.Tactical.Mappers;

public interface IAuthenticationResponseMapper
{
  Domain.AuthenticationResponse Map(Dto.AuthenticationResponse response);
}