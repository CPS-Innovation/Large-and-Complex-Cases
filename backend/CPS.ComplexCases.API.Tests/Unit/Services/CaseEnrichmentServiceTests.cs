using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.Egress.Models.Dto;
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
    Assert.Equal(cases.Count, result.Count());

    foreach (var caseResponse in result)
    {
      var expectedMetadata = metadata.First(m => m.CaseId == caseResponse.CaseId);
      Assert.Equal(expectedMetadata.EgressWorkspaceId, caseResponse.EgressWorkspaceId);
      Assert.Equal(expectedMetadata.NetappFolderPath, caseResponse.NetappFolderPath);
    }

    _caseMetadataServiceMock.Verify(
        s => s.GetCaseMetadataForCaseIdsAsync(It.Is<IEnumerable<int>>(ids =>
            ids.All(id => caseIds.Contains(id)) && ids.Count() == caseIds.Count)),
        Times.Once);
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
    Assert.Equal(cases.Count, result.Count());

    var firstCaseResponse = result.First(c => c.CaseId == cases.First().CaseId);
    Assert.Equal(metadata.First().EgressWorkspaceId, firstCaseResponse.EgressWorkspaceId);
    Assert.Equal(metadata.First().NetappFolderPath, firstCaseResponse.NetappFolderPath);

    foreach (var caseResponse in result.Where(c => c.CaseId != cases.First().CaseId))
    {
      Assert.Null(caseResponse.EgressWorkspaceId);
      Assert.Null(caseResponse.NetappFolderPath);
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
    Assert.Empty(result);

    _caseMetadataServiceMock.Verify(
        s => s.GetCaseMetadataForCaseIdsAsync(It.IsAny<IEnumerable<int>>()),
        Times.Never);
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
    Assert.Equal(cases.Count, result.Count());

    foreach (var caseResponse in result)
    {
      Assert.Null(caseResponse.EgressWorkspaceId);
      Assert.Null(caseResponse.NetappFolderPath);
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
    var caseResponse = result.Single();

    Assert.Equal(caseDto.CaseId, caseResponse.CaseId);
    Assert.Equal(caseDto.Urn, caseResponse.Urn);
    Assert.Equal(caseDto.OperationName, caseResponse.OperationName);
    Assert.Equal(caseDto.LeadDefendantName, caseResponse.LeadDefendantName);
    Assert.Equal(caseDto.RegistrationDate, caseResponse.RegistrationDate);
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
    Assert.Equal(workspaces.Data.Count(), result.Data.Count());

    foreach (var workspaceResponse in result.Data)
    {
      var expectedMetadata = metadata.First(m => m.EgressWorkspaceId == workspaceResponse.Id);
      Assert.Equal(expectedMetadata.CaseId, workspaceResponse.CaseId);
    }

    Assert.Equal(workspaces.Pagination.Count, result.Pagination.Count);
    Assert.Equal(workspaces.Pagination.Take, result.Pagination.Take);
    Assert.Equal(workspaces.Pagination.Skip, result.Pagination.Skip);
    Assert.Equal(workspaces.Pagination.TotalResults, result.Pagination.TotalResults);

    _caseMetadataServiceMock.Verify(
        s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.Is<IEnumerable<string>>(ids =>
            ids.All(id => workspaceIds.Contains(id)) && ids.Count() == workspaceIds.Count)),
        Times.Once);
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
    Assert.Equal(workspaces.Data.Count(), result.Data.Count());

    var firstWorkspaceResponse = result.Data.First(w => w.Id == workspaces.Data.First().Id);
    Assert.Equal(metadata.First().CaseId, firstWorkspaceResponse.CaseId);

    foreach (var workspaceResponse in result.Data.Where(w => w.Id != workspaces.Data.First().Id))
    {
      Assert.Null(workspaceResponse.CaseId);
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
    Assert.Empty(result.Data);

    _caseMetadataServiceMock.Verify(
        s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()),
        Times.Never);
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
    Assert.Equal(workspaces.Data.Count(), result.Data.Count());

    foreach (var workspaceResponse in result.Data)
    {
      Assert.Null(workspaceResponse.CaseId);
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
    var workspaceResponse = result.Data.Single();

    Assert.Equal(workspace.Id, workspaceResponse.Id);
    Assert.Equal(workspace.Name, workspaceResponse.Name);
    Assert.Equal(workspace.DateCreated, workspaceResponse.DateCreated);
    Assert.Null(workspaceResponse.CaseId);
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