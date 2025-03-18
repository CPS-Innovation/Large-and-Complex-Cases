using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.DDEI.Client;

public interface IDdeiClient
{
  Task<IEnumerable<CaseDto>> ListCasesByUrnAsync(DdeiUrnArgDto arg);
  Task<IEnumerable<CaseDto>> ListCasesByOperationNameAsync(DdeiOperationNameArgDto arg);
  Task<IEnumerable<CaseDto>> ListCasesByDefendantNameAsync(DdeiDefendantNameArgDto arg);
  Task<AreasDto> GetAreasAsync(DdeiBaseArgDto arg);
}