using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.DDEI.Services;

public class CaseNamingService : ICaseNamingService
{
    public Task<CaseNameDto> GenerateCaseName(CaseDto caseDto)
    {
        string operationName = !string.IsNullOrWhiteSpace(caseDto.OperationName)
            ? caseDto.OperationName
            : !string.IsNullOrWhiteSpace(caseDto.LeadDefendantSurname)
                ? caseDto.LeadDefendantSurname
                : "Unknown";

        var caseNameDto = new CaseNameDto
        {
            CaseName = $"{operationName}-{caseDto.Urn}",
            OperationName = operationName
        };

        return Task.FromResult(caseNameDto);
    }
}