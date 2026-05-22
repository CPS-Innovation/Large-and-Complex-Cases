using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Services;

namespace CPS.ComplexCases.DDEI.Tests.Services;

public class CaseNamingServiceTests
{
    private const string Urn = "12AB3456";
    private readonly CaseNamingService _service = new();

    [Fact]
    public async Task GenerateCaseName_WhenOperationNameIsPresent_UsesOperationName()
    {
        var caseDto = new CaseDto
        {
            Urn = Urn,
            OperationName = "OperationAlpha",
            LeadDefendantSurname = "Smith"
        };

        var result = await _service.GenerateCaseName(caseDto);

        Assert.Equal("OperationAlpha", result.OperationName);
        Assert.Equal("OperationAlpha-12AB3456", result.CaseName);
    }

    [Fact]
    public async Task GenerateCaseName_WhenOperationNameIsNullAndSurnameIsPresent_UsesSurname()
    {
        var caseDto = new CaseDto
        {
            Urn = Urn,
            OperationName = null,
            LeadDefendantSurname = "Smith"
        };

        var result = await _service.GenerateCaseName(caseDto);

        Assert.Equal("Smith", result.OperationName);
        Assert.Equal("Smith-12AB3456", result.CaseName);
    }

    [Fact]
    public async Task GenerateCaseName_WhenOperationNameIsWhitespaceAndSurnameIsPresent_UsesSurname()
    {
        var caseDto = new CaseDto
        {
            Urn = Urn,
            OperationName = "   ",
            LeadDefendantSurname = "Jones"
        };

        var result = await _service.GenerateCaseName(caseDto);

        Assert.Equal("Jones", result.OperationName);
        Assert.Equal("Jones-12AB3456", result.CaseName);
    }

    [Fact]
    public async Task GenerateCaseName_WhenBothOperationNameAndSurnameAreNull_UsesUnknown()
    {
        var caseDto = new CaseDto
        {
            Urn = Urn,
            OperationName = null,
            LeadDefendantSurname = null
        };

        var result = await _service.GenerateCaseName(caseDto);

        Assert.Equal("Unknown", result.OperationName);
        Assert.Equal("Unknown-12AB3456", result.CaseName);
    }

    [Fact]
    public async Task GenerateCaseName_WhenBothOperationNameAndSurnameAreWhitespace_UsesUnknown()
    {
        var caseDto = new CaseDto
        {
            Urn = Urn,
            OperationName = "   ",
            LeadDefendantSurname = "   "
        };

        var result = await _service.GenerateCaseName(caseDto);

        Assert.Equal("Unknown", result.OperationName);
        Assert.Equal("Unknown-12AB3456", result.CaseName);
    }

    [Fact]
    public async Task GenerateCaseName_CaseNameIncludesUrn()
    {
        var caseDto = new CaseDto
        {
            Urn = "99ZZ1234",
            OperationName = "OperationBeta"
        };

        var result = await _service.GenerateCaseName(caseDto);

        Assert.EndsWith("-99ZZ1234", result.CaseName);
    }

    [Fact]
    public async Task GenerateCaseName_OperationNameTakesPrecedenceOverSurname()
    {
        var caseDto = new CaseDto
        {
            Urn = Urn,
            OperationName = "OperationGamma",
            LeadDefendantSurname = "Brown"
        };

        var result = await _service.GenerateCaseName(caseDto);

        Assert.Equal("OperationGamma", result.OperationName);
        Assert.DoesNotContain("Brown", result.CaseName);
    }
}
