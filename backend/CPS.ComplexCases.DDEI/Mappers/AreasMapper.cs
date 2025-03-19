using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;

namespace CPS.ComplexCases.DDEI.Mappers;

public class AreasMapper : IAreasMapper
{
  public AreasDto MapAreas(DdeiUserFilteredDataDto userFilteredData, DdeiUserDataDto userData, IEnumerable<DdeiUnitDto> allUnits)
  {
    var userAreas = userFilteredData.Areas.Select(a => new AreaDto
    {
      Id = a.Id,
      Description = a.Description
    }).ToList();

    var allAreas = allUnits
        .Select(u => new AreaDto
        {
          Id = u.AreaId,
          Description = u.AreaDescription
        })
        .GroupBy(a => a.Id)
        .Select(g => g.First())
        .ToList();

    var homeUnitId = userData.HomeUnit.UnitId;
    var homeUnit = allUnits.FirstOrDefault(u => u.Id == homeUnitId);

    AreaDto? homeArea = null;
    if (homeUnit != null)
    {
      homeArea = new AreaDto
      {
        Id = homeUnit.AreaId,
        Description = homeUnit.AreaDescription
      };
    }

    return new AreasDto
    {
      UserAreas = userAreas,
      AllAreas = allAreas,
      HomeArea = homeArea
    };
  }
}