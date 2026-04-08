using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.DDEI.Services;

public interface ICaseNamingService
{
    Task<string> GenerateCaseName(CaseDto caseDto);
}