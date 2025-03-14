using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;

namespace CPS.ComplexCases.DDEI.Mappers;

public class UserDetailsMapper : IUserDetailsMapper
{
  public IEnumerable<AreaDto> MapUserAreas(DdeiUserFilteredDataDto userDetails)
  {
    return userDetails.Areas.Select(area => new AreaDto
    {
      Id = area.Id,
      Description = area.Description
    });
  }
}