using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.DDEI.Services;

public interface ICaseNamingService
{
    Task<CaseNameDto> GenerateCaseName(CaseDto caseDto);
}