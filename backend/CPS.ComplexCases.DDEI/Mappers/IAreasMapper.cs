using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;

namespace CPS.ComplexCases.DDEI.Mappers;

public interface IAreasMapper
{
  AreasDto MapAreas(DdeiUserFilteredDataDto userFilteredData, DdeiUserDataDto userData, IEnumerable<DdeiUnitDto> allAreas);
}