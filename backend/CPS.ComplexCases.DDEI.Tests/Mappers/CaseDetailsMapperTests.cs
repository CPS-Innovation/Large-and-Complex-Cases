using AutoFixture;
using CPS.ComplexCases.DDEI.Mappers;
using CPS.ComplexCases.DDEI.Models.Response;

namespace CPS.ComplexCases.DDEI.Tests.Mappers;

public class CaseDetailsMapperTests
{
  private readonly Fixture _fixture;
  private readonly CaseDetailsMapper _mapper;

  public CaseDetailsMapperTests()
  {
    _fixture = new Fixture();
    _mapper = new CaseDetailsMapper();
  }

  [Fact]
  public void MapCaseDetails_ValidDdeiCaseDetailsDto_ReturnsCorrectCaseDto()
  {
    // Arrange
    var summary = new DdeiCaseSummaryDto
    {
      Id = _fixture.Create<int>(),
      Urn = _fixture.Create<string>(),
      LeadDefendantFirstNames = _fixture.Create<string>(),
      LeadDefendantSurname = _fixture.Create<string>(),
      Operation = _fixture.Create<string>(),
      RegistrationDate = _fixture.Create<string>()
    };


    // Act
    var result = _mapper.MapCaseDetails(summary);

    // Assert
    Assert.Equal(summary.Id, result.CaseId);
    Assert.Equal(summary.Urn, result.Urn);
    Assert.Equal($"{summary.LeadDefendantFirstNames} {summary.LeadDefendantSurname}", result.LeadDefendantName);
    Assert.Equal(summary.Operation, result.OperationName);
    Assert.Equal(summary.RegistrationDate, result.RegistrationDate);
  }
}