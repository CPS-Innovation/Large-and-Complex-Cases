using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Services;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.Egress.Models.Dto;
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

  [Fact]
  public async Task EnrichEgressWorkspacesWithMetadataAsync_WhenWorkspacesProvided_EnrichesWithMetadata()
  {
    // Arrange
    var workspaces = CreateSampleWorkspaces(3);
    var workspaceIds = workspaces.Data.Select(w => w.Id).ToList();

    var metadata = workspaceIds.Select(id => new CaseMetadata
    {
      CaseId = _fixture.Create<int>(),
      EgressWorkspaceId = id,
      NetappFolderPath = _fixture.Create<string>()
    }).ToList();

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.Is<IEnumerable<string>>(ids =>
            ids.All(id => workspaceIds.Contains(id)) && ids.Count() == workspaceIds.Count)))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichEgressWorkspacesWithMetadataAsync(workspaces);

    // Assert
    using (new AssertionScope())
    {
      result.Data.Should().HaveCount(workspaces.Data.Count());

      foreach (var workspaceResponse in result.Data)
      {
        var expectedMetadata = metadata.First(m => m.EgressWorkspaceId == workspaceResponse.Id);
        workspaceResponse.CaseId.Should().Be(expectedMetadata.CaseId);
      }

      result.Pagination.Count.Should().Be(workspaces.Pagination.Count);
      result.Pagination.Take.Should().Be(workspaces.Pagination.Take);
      result.Pagination.Skip.Should().Be(workspaces.Pagination.Skip);
      result.Pagination.TotalResults.Should().Be(workspaces.Pagination.TotalResults);

      _caseMetadataServiceMock.Verify(
          s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.Is<IEnumerable<string>>(ids =>
              ids.All(id => workspaceIds.Contains(id)) && ids.Count() == workspaceIds.Count)),
          Times.Once);
    }
  }

  [Fact]
  public async Task EnrichEgressWorkspacesWithMetadataAsync_WhenMetadataNotFoundForSomeWorkspaces_ReturnsEnrichedWorkspacesWithAvailableMetadata()
  {
    // Arrange
    var workspaces = CreateSampleWorkspaces(3);
    var workspaceIds = workspaces.Data.Select(w => w.Id).ToList();

    // Only create metadata for the first workspace
    var metadata = new List<CaseMetadata>
        {
            new CaseMetadata
            {
                CaseId = _fixture.Create<int>(),
                EgressWorkspaceId = workspaces.Data.First().Id,
                NetappFolderPath = _fixture.Create<string>()
            }
        };

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichEgressWorkspacesWithMetadataAsync(workspaces);

    // Assert
    using (new AssertionScope())
    {
      result.Data.Should().HaveCount(workspaces.Data.Count());

      var firstWorkspaceResponse = result.Data.First(w => w.Id == workspaces.Data.First().Id);
      firstWorkspaceResponse.CaseId.Should().Be(metadata.First().CaseId);

      foreach (var workspaceResponse in result.Data.Where(w => w.Id != workspaces.Data.First().Id))
      {
        workspaceResponse.CaseId.Should().BeNull();
      }
    }
  }

  [Fact]
  public async Task EnrichEgressWorkspacesWithMetadataAsync_WhenNoWorkspacesProvided_ReturnsEmptyCollection()
  {
    // Arrange
    var workspaces = CreateEmptyWorkspaces();

    // Act
    var result = await _service.EnrichEgressWorkspacesWithMetadataAsync(workspaces);

    // Assert
    using (new AssertionScope())
    {
      result.Data.Should().BeEmpty();

      _caseMetadataServiceMock.Verify(
          s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()),
          Times.Never);
    }
  }

  [Fact]
  public async Task EnrichEgressWorkspacesWithMetadataAsync_WhenMetadataServiceThrowsException_LogsWarningAndReturnsUnmodifiedWorkspaces()
  {
    // Arrange
    var workspaces = CreateSampleWorkspaces(3);
    var expectedException = new Exception("Metadata service error");

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()))
        .ThrowsAsync(expectedException);

    // Act
    var result = await _service.EnrichEgressWorkspacesWithMetadataAsync(workspaces);

    // Assert
    using (new AssertionScope())
    {
      result.Data.Should().HaveCount(workspaces.Data.Count());

      foreach (var workspaceResponse in result.Data)
      {
        workspaceResponse.CaseId.Should().BeNull();
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
  public async Task EnrichEgressWorkspacesWithMetadataAsync_VerifiesCorrectWorkspaceMapping()
  {
    // Arrange
    var workspaces = CreateSampleWorkspaces(1);
    var workspace = workspaces.Data.First();

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(Enumerable.Empty<CaseMetadata>());

    // Act
    var result = await _service.EnrichEgressWorkspacesWithMetadataAsync(workspaces);

    // Assert
    using (new AssertionScope())
    {
      var workspaceResponse = result.Data.Single();

      workspaceResponse.Id.Should().Be(workspace.Id);
      workspaceResponse.Name.Should().Be(workspace.Name);
      workspaceResponse.DateCreated.Should().Be(workspace.DateCreated);
      workspaceResponse.CaseId.Should().BeNull();
    }
  }
  private ListWorkspacesDto CreateSampleWorkspaces(int count)
  {
    var workspaceData = _fixture.CreateMany<ListWorkspaceDataDto>(count).ToList();

    return new ListWorkspacesDto
    {
      Data = workspaceData,
      Pagination = new PaginationDto
      {
        Count = count,
        Take = count,
        Skip = 0,
        TotalResults = count
      }
    };
  }

  private ListWorkspacesDto CreateEmptyWorkspaces()
  {
    return new ListWorkspacesDto
    {
      Data = new List<ListWorkspaceDataDto>(),
      Pagination = new PaginationDto
      {
        Count = 0,
        Take = 0,
        Skip = 0,
        TotalResults = 0
      }
    };
  }
}