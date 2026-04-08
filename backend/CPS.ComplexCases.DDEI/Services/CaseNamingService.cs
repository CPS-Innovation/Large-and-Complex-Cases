using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.DDEI.Services;

public class CaseNamingService : ICaseNamingService
{
    public Task<string> GenerateCaseName(CaseDto caseDto)
    {
        string caseName;
        if (!string.IsNullOrWhiteSpace(caseDto.OperationName))
        {
            caseName = $"{caseDto.OperationName}-{caseDto.Urn}";
        }
        else
        {
            caseName = $"{caseDto.LeadDefendantSurname}-{caseDto.Urn}";
        }
        return Task.FromResult(caseName);
    }
}