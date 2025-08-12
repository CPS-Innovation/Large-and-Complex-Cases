using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.NetApp.Models.Dto;
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
  public async Task EnrichCasesWithMetadataAsync_LogsInformationWhenEnrichingCases()
  {
    // Arrange
    var cases = _fixture.CreateMany<CaseDto>(3).ToList();

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForCaseIdsAsync(It.IsAny<IEnumerable<int>>()))
        .ReturnsAsync(new List<CaseMetadata>());

    // Act
    await _service.EnrichCasesWithMetadataAsync(cases);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enriching 3 cases with metadata")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
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

  [Fact]
  public async Task EnrichEgressWorkspacesWithMetadataAsync_WhenMetadataHasNullEgressWorkspaceId_FiltersCorrectly()
  {
    // Arrange
    var workspaces = CreateSampleWorkspaces(2);
    var workspaceIds = workspaces.Data.Select(w => w.Id).ToList();

    var metadata = new List<CaseMetadata>
    {
        new CaseMetadata { CaseId = 1, EgressWorkspaceId = workspaceIds.First(), NetappFolderPath = "path1" },
        new CaseMetadata { CaseId = 2, EgressWorkspaceId = null, NetappFolderPath = "path2" }, // Should be filtered out
        new CaseMetadata { CaseId = 3, EgressWorkspaceId = workspaceIds.Last(), NetappFolderPath = "path3" }
    };

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichEgressWorkspacesWithMetadataAsync(workspaces);

    // Assert
    var firstWorkspace = result.Data.First(w => w.Id == workspaceIds.First());
    var lastWorkspace = result.Data.First(w => w.Id == workspaceIds.Last());

    Assert.Equal(1, firstWorkspace.CaseId);
    Assert.Equal(3, lastWorkspace.CaseId);
  }

  [Fact]
  public async Task EnrichEgressWorkspacesWithMetadataAsync_LogsInformationWhenEnrichingWorkspaces()
  {
    // Arrange
    var workspaces = CreateSampleWorkspaces(2);

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(new List<CaseMetadata>());

    // Act
    await _service.EnrichEgressWorkspacesWithMetadataAsync(workspaces);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enriching 2 workspaces with metadata")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_WhenFoldersProvided_EnrichesWithMetadata()
  {
    // Arrange
    var folders = CreateSampleNetAppFolders(3);
    var folderPaths = folders.Data.FolderData
        .Where(d => d.Path != null)
        .Select(d => $"{folders.Data.BucketName}:{d.Path}")
        .ToList();

    var metadata = folderPaths.Select(path => new CaseMetadata
    {
      CaseId = _fixture.Create<int>(),
      EgressWorkspaceId = _fixture.Create<string>(),
      NetappFolderPath = path
    }).ToList();

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.Is<IEnumerable<string>>(paths =>
            paths.All(p => folderPaths.Contains(p)) && paths.Count() == folderPaths.Count)))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    Assert.Equal(folderPaths.Count, result.Data.Folders.Count());

    foreach (var folderResponse in result.Data.Folders)
    {
      var expectedPath = $"{folders.Data.BucketName}:{folderResponse.FolderPath}";
      var expectedMetadata = metadata.First(m => m.NetappFolderPath == expectedPath);
      Assert.Equal(expectedMetadata.CaseId, folderResponse.CaseId);
    }

    Assert.Equal(folders.Data.RootPath, result.Data.RootPath);
    Assert.Empty(result.Data.Files);

    _caseMetadataServiceMock.Verify(
        s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.Is<IEnumerable<string>>(paths =>
            paths.All(p => folderPaths.Contains(p)) && paths.Count() == folderPaths.Count)),
        Times.Once);
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_WhenMetadataNotFoundForSomeFolders_ReturnsEnrichedFoldersWithAvailableMetadata()
  {
    // Arrange
    var folders = CreateSampleNetAppFolders(3);
    var folderPaths = folders.Data.FolderData
        .Where(d => d.Path != null)
        .Select(d => $"{folders.Data.BucketName}:{d.Path}")
        .ToList();

    var metadata = new List<CaseMetadata>
    {
        new CaseMetadata
        {
            CaseId = _fixture.Create<int>(),
            EgressWorkspaceId = _fixture.Create<string>(),
            NetappFolderPath = folderPaths.First()
        }
    };

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    Assert.Equal(folderPaths.Count, result.Data.Folders.Count());

    var firstFolderResponse = result.Data.Folders.First();
    Assert.Equal(metadata.First().CaseId, firstFolderResponse.CaseId);

    foreach (var folderResponse in result.Data.Folders.Skip(1))
    {
      Assert.Null(folderResponse.CaseId);
    }
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_WhenNoFoldersProvided_ReturnsEmptyCollection()
  {
    // Arrange
    var folders = CreateEmptyNetAppFolders();

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    Assert.Empty(result.Data.Folders);
    Assert.Empty(result.Data.Files);

    _caseMetadataServiceMock.Verify(
        s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()),
        Times.Never);
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_WhenMetadataServiceThrowsException_LogsWarningAndReturnsUnmodifiedFolders()
  {
    // Arrange
    var folders = CreateSampleNetAppFolders(3);
    var expectedException = new Exception("NetApp metadata service error");

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
        .ThrowsAsync(expectedException);

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    Assert.Equal(folders.Data.FolderData.Count(), result.Data.Folders.Count());

    foreach (var folderResponse in result.Data.Folders)
    {
      Assert.Null(folderResponse.CaseId);
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
  public async Task EnrichNetAppFoldersWithMetadataAsync_WhenFoldersHaveNullPaths_HandlesGracefully()
  {
    // Arrange
    var folderData = new List<ListNetAppFolderDataDto>
    {
        new ListNetAppFolderDataDto { Path = null },
        new ListNetAppFolderDataDto { Path = "valid-path" },
        new ListNetAppFolderDataDto { Path = null }
    };

    var folders = new ListNetAppObjectsDto
    {
      Data = new ListNetAppDataDto
      {
        BucketName = "test-bucket",
        FolderData = folderData,
        RootPath = "/root",
        FileData = new List<ListNetAppFileDataDto>()
      },
      Pagination = new NetApp.Models.Dto.PaginationDto
      {
        MaxKeys = 10,
        NextContinuationToken = null
      }
    };

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(new List<CaseMetadata>());

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    Assert.Single(result.Data.Folders);
    Assert.Equal("valid-path", result.Data.Folders.First().FolderPath);
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_VerifiesPaginationMapping()
  {
    // Arrange
    var folders = CreateSampleNetAppFolders(1);

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(new List<CaseMetadata>());

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    Assert.Equal(folders.Pagination.MaxKeys, result.Pagination.MaxKeys);
    Assert.Equal(folders.Pagination.NextContinuationToken, result.Pagination.NextContinuationToken);
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_WhenMetadataHasNullNetappFolderPath_FiltersCorrectly()
  {
    // Arrange
    var folders = CreateSampleNetAppFolders(2);
    var folderPaths = folders.Data.FolderData
        .Where(d => d.Path != null)
        .Select(d => $"{folders.Data.BucketName}:{d.Path}")
        .ToList();

    var metadata = new List<CaseMetadata>
    {
        new CaseMetadata { CaseId = 1, EgressWorkspaceId = "ws1", NetappFolderPath = folderPaths.First() },
        new CaseMetadata { CaseId = 2, EgressWorkspaceId = "ws2", NetappFolderPath = null }, // Should be filtered out
        new CaseMetadata { CaseId = 3, EgressWorkspaceId = "ws3", NetappFolderPath = folderPaths.Last() }
    };

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(metadata);

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    var enrichedFolders = result.Data.Folders.Where(f => f.CaseId.HasValue).ToList();
    Assert.Equal(2, enrichedFolders.Count);
    Assert.Contains(enrichedFolders, f => f.CaseId == 1);
    Assert.Contains(enrichedFolders, f => f.CaseId == 3);
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_LogsInformationWhenEnrichingFolders()
  {
    // Arrange
    var folders = CreateSampleNetAppFolders(4);

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(new List<CaseMetadata>());

    // Act
    await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enriching 4 workspaces with metadata")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  public async Task EnrichNetAppFoldersWithMetadataAsync_VerifiesFolderPathExtraction()
  {
    // Arrange
    var folders = CreateSampleNetAppFolders(1);
    var expectedPath = folders.Data.FolderData.First().Path;

    _caseMetadataServiceMock
        .Setup(s => s.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(new List<CaseMetadata>());

    // Act
    var result = await _service.EnrichNetAppFoldersWithMetadataAsync(folders);

    // Assert
    var folderResponse = result.Data.Folders.Single();
    Assert.Equal(expectedPath, folderResponse.FolderPath);
  }

  private ListWorkspacesDto CreateSampleWorkspaces(int count)
  {
    var workspaceData = _fixture.CreateMany<ListWorkspaceDataDto>(count).ToList();

    return new ListWorkspacesDto
    {
      Data = workspaceData,
      Pagination = new Common.Models.Domain.Dto.PaginationDto
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
      Pagination = new Common.Models.Domain.Dto.PaginationDto
      {
        Count = 0,
        Take = 0,
        Skip = 0,
        TotalResults = 0
      }
    };
  }

  private ListNetAppObjectsDto CreateSampleNetAppFolders(int count)
  {
    var folderData = Enumerable.Range(1, count)
        .Select(i => new ListNetAppFolderDataDto { Path = $"folder-{i}" })
        .ToList();

    return new ListNetAppObjectsDto
    {
      Data = new ListNetAppDataDto
      {
        BucketName = "test-bucket",
        FolderData = folderData,
        RootPath = "/root/path",
        FileData = new List<ListNetAppFileDataDto>()
      },
      Pagination = new NetApp.Models.Dto.PaginationDto
      {
        MaxKeys = count,
        NextContinuationToken = count > 10 ? "next-token" : null
      }
    };
  }

  private ListNetAppObjectsDto CreateEmptyNetAppFolders()
  {
    return new ListNetAppObjectsDto
    {
      Data = new ListNetAppDataDto
      {
        BucketName = "test-bucket",
        FolderData = new List<ListNetAppFolderDataDto>(),
        RootPath = "/root/path",
        FileData = new List<ListNetAppFileDataDto>()
      },
      Pagination = new NetApp.Models.Dto.PaginationDto
      {
        MaxKeys = 0,
        NextContinuationToken = null
      }
    };
  }
}