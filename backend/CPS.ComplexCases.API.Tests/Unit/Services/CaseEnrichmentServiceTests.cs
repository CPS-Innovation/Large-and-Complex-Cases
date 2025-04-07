using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Services;
using CPS.ComplexCases.DDEI.Models.Dto;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Services;

public class CaseEnrichmentServiceTests
{
  private readonly Fixture _fixture;
  private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
  private readonly Mock<ILogger<CaseEnrichmentService>> _loggerMock;
  private readonly CaseEnrichmentService _service;

  public CaseEnrichmentServiceTests()
  {
    _fixture = new Fixture();
    _fixture.Customize(new AutoMoqCustomization());

    _caseMetadataServiceMock = _fixture.Freeze<Mock<ICaseMetadataService>>();
    _loggerMock = _fixture.Freeze<Mock<ILogger<CaseEnrichmentService>>>();

    _service = new CaseEnrichmentService(
        _caseMetadataServiceMock.Object,
        _loggerMock.Object
    );
  }

  [Fact]
  public async Task EnrichCasesWithMetadataAsync_WhenCasesProvided_EnrichesWithMetadata()
  {
    // Arrange
    var cases = _fixture.CreateMany<CaseDto>(3).ToList();
    var caseIds = cases.Select(c => c.CaseId).ToList();

    var metadata = caseIds.Select(id => new CaseMetadata
    {
      CaseId = id,
      EgressWorkspaceId = _fixture.Create<string>(),
      NetappFolderPath = _fixture.Create<string>()
    }).ToList();

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForCaseIdsAsync(It.Is<IEnumerable<int>>(ids =>
            ids.All(id => caseIds.Contains(id)) && ids.Count() == caseIds.Count)))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichCasesWithMetadataAsync(cases);

    // Assert
    using (new AssertionScope())
    {
      result.Should().HaveCount(cases.Count);

      foreach (var caseResponse in result)
      {
        var expectedMetadata = metadata.First(m => m.CaseId == caseResponse.CaseId);
        caseResponse.EgressWorkspaceId.Should().Be(expectedMetadata.EgressWorkspaceId);
        caseResponse.NetappFolderPath.Should().Be(expectedMetadata.NetappFolderPath);
      }

      _caseMetadataServiceMock.Verify(
          s => s.GetCaseMetadataForCaseIdsAsync(It.Is<IEnumerable<int>>(ids =>
              ids.All(id => caseIds.Contains(id)) && ids.Count() == caseIds.Count)),
          Times.Once);
    }
  }

  [Fact]
  public async Task EnrichCasesWithMetadataAsync_WhenMetadataNotFoundForSomeCases_ReturnsEnrichedCasesWithAvailableMetadata()
  {
    // Arrange
    var cases = _fixture.CreateMany<CaseDto>(3).ToList();
    var caseIds = cases.Select(c => c.CaseId).ToList();

    // Only create metadata for the first case
    var metadata = new List<CaseMetadata>
        {
            new CaseMetadata
            {
                CaseId = cases.First().CaseId,
                EgressWorkspaceId = _fixture.Create<string>(),
                NetappFolderPath = _fixture.Create<string>()
            }
        };

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForCaseIdsAsync(It.IsAny<IEnumerable<int>>()))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichCasesWithMetadataAsync(cases);

    // Assert
    using (new AssertionScope())
    {
      result.Should().HaveCount(cases.Count);

      var firstCaseResponse = result.First(c => c.CaseId == cases.First().CaseId);
      firstCaseResponse.EgressWorkspaceId.Should().Be(metadata.First().EgressWorkspaceId);
      firstCaseResponse.NetappFolderPath.Should().Be(metadata.First().NetappFolderPath);

      foreach (var caseResponse in result.Where(c => c.CaseId != cases.First().CaseId))
      {
        caseResponse.EgressWorkspaceId.Should().BeNull();
        caseResponse.NetappFolderPath.Should().BeNull();
      }
    }
  }

  [Fact]
  public async Task EnrichCasesWithMetadataAsync_WhenNoCasesProvided_ReturnsEmptyCollection()
  {
    // Arrange
    var cases = Enumerable.Empty<CaseDto>();

    // Act
    var result = await _service.EnrichCasesWithMetadataAsync(cases);

    // Assert
    using (new AssertionScope())
    {
      result.Should().BeEmpty();

      _caseMetadataServiceMock.Verify(
          s => s.GetCaseMetadataForCaseIdsAsync(It.IsAny<IEnumerable<int>>()),
          Times.Never);
    }
  }

  [Fact]
  public async Task EnrichCasesWithMetadataAsync_WhenMetadataServiceThrowsException_LogsWarningAndReturnsUnmodifiedCases()
  {
    // Arrange
    var cases = _fixture.CreateMany<CaseDto>(3).ToList();
    var expectedException = new Exception("Metadata service error");

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForCaseIdsAsync(It.IsAny<IEnumerable<int>>()))
        .ThrowsAsync(expectedException);

    // Act
    var result = await _service.EnrichCasesWithMetadataAsync(cases);

    // Assert
    using (new AssertionScope())
    {
      result.Should().HaveCount(cases.Count);

      foreach (var caseResponse in result)
      {
        caseResponse.EgressWorkspaceId.Should().BeNull();
        caseResponse.NetappFolderPath.Should().BeNull();
      }

      _loggerMock.Verify(
          x => x.Log(
              LogLevel.Warning,
              It.IsAny<EventId>(),
              It.IsAny<It.IsAnyType>(),
              It.Is<Exception>(ex => ex == expectedException),
              It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
          Times.Once);
    }
  }

  [Fact]
  public async Task EnrichCasesWithMetadataAsync_VerifiesCorrectCaseMapping()
  {
    // Arrange
    var caseDto = _fixture.Create<CaseDto>();
    var cases = new[] { caseDto };

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForCaseIdsAsync(It.IsAny<IEnumerable<int>>()))
        .ReturnsAsync(Enumerable.Empty<CaseMetadata>());

    // Act
    var result = await _service.EnrichCasesWithMetadataAsync(cases);

    // Assert
    using (new AssertionScope())
    {
      var caseResponse = result.Single();

      caseResponse.CaseId.Should().Be(caseDto.CaseId);
      caseResponse.Urn.Should().Be(caseDto.Urn);
      caseResponse.OperationName.Should().Be(caseDto.OperationName);
      caseResponse.LeadDefendantName.Should().Be(caseDto.LeadDefendantName);
      caseResponse.RegistrationDate.Should().Be(caseDto.RegistrationDate);
    }
  }
}