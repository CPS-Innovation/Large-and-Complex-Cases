using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.Data.Tests.Unit.Services;

public class CaseMetadataServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ICaseMetadataRepository> _repositoryMock;
    private readonly Mock<ILogger<CaseMetadataService>> _loggerMock;
    private readonly CaseMetadataService _service;

    public CaseMetadataServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _repositoryMock = _fixture.Freeze<Mock<ICaseMetadataRepository>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<CaseMetadataService>>>();

        _service = new CaseMetadataService(
            _repositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateEgressConnectionAsync_WhenMetadataExists_UpdatesExistingRecord()
    {
        // Arrange
        var caseId = _fixture.Create<int>();
        var egressWorkspaceId = _fixture.Create<string>();
        var existingMetadata = new CaseMetadata
        {
            CaseId = caseId,
            EgressWorkspaceId = _fixture.Create<string>()
        };

        var createDto = new CreateEgressConnectionDto
        {
            CaseId = caseId,
            EgressWorkspaceId = egressWorkspaceId
        };

        _repositoryMock
            .Setup(r => r.GetByCaseIdAsync(caseId))
            .ReturnsAsync(existingMetadata);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CaseMetadata>()))
            .ReturnsAsync(existingMetadata);

        // Act
        await _service.CreateEgressConnectionAsync(createDto);

        // Assert
        Assert.Equal(egressWorkspaceId, existingMetadata.EgressWorkspaceId);

        _repositoryMock.Verify(
            r => r.GetByCaseIdAsync(caseId),
            Times.Once);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.Is<CaseMetadata>(m =>
                m.CaseId == caseId &&
                m.EgressWorkspaceId == egressWorkspaceId)),
            Times.Once);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<CaseMetadata>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateEgressConnectionAsync_WhenMetadataDoesNotExist_CreatesNewRecord()
    {
        // Arrange
        var caseId = _fixture.Create<int>();
        var egressWorkspaceId = _fixture.Create<string>();
        var newMetadata = new CaseMetadata
        {
            CaseId = caseId,
            EgressWorkspaceId = egressWorkspaceId
        };

        var createDto = new CreateEgressConnectionDto
        {
            CaseId = caseId,
            EgressWorkspaceId = egressWorkspaceId
        };

        _repositoryMock
            .Setup(r => r.GetByCaseIdAsync(caseId))
            .ReturnsAsync((CaseMetadata?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<CaseMetadata>()))
            .ReturnsAsync(newMetadata);

        // Act
        await _service.CreateEgressConnectionAsync(createDto);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByCaseIdAsync(caseId),
            Times.Once);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<CaseMetadata>()),
            Times.Never);

        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<CaseMetadata>(m =>
                m.CaseId == caseId &&
                m.EgressWorkspaceId == egressWorkspaceId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateEgressConnectionAsync_WhenRepositoryThrowsException_LogsAndRethrows()
    {
        // Arrange
        var caseId = _fixture.Create<int>();
        var egressWorkspaceId = _fixture.Create<string>();
        var expectedException = new Exception("Repository error");

        var createDto = new CreateEgressConnectionDto
        {
            CaseId = caseId,
            EgressWorkspaceId = egressWorkspaceId
        };

        _repositoryMock
            .Setup(r => r.GetByCaseIdAsync(caseId))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.CreateEgressConnectionAsync(createDto));

        Assert.Same(expectedException, exception);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCaseMetadataForCaseIdsAsync_WhenCaseIdsProvided_ReturnsMatchingRecords()
    {
        // Arrange
        var caseIds = _fixture.CreateMany<int>(3).ToList();
        var expectedMetadata = _fixture.CreateMany<CaseMetadata>(3).ToList();

        for (int i = 0; i < 3; i++)
        {
            expectedMetadata[i].CaseId = caseIds[i];
        }

        _repositoryMock
            .Setup(r => r.GetByCaseIdsAsync(It.Is<IEnumerable<int>>(ids =>
                ids.Count() == caseIds.Count &&
                ids.All(id => caseIds.Contains(id)))))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _service.GetCaseMetadataForCaseIdsAsync(caseIds);

        // Assert
        Assert.Equal(expectedMetadata, result);

        _repositoryMock.Verify(
            r => r.GetByCaseIdsAsync(It.Is<IEnumerable<int>>(ids =>
                ids.Count() == caseIds.Count &&
                ids.All(id => caseIds.Contains(id)))),
            Times.Once);
    }

    [Fact]
    public async Task GetCaseMetadataForCaseIdsAsync_WhenRepositoryThrowsException_LogsAndRethrows()
    {
        // Arrange
        var caseIds = _fixture.CreateMany<int>(3).ToList();
        var expectedException = new Exception("Repository error");

        _repositoryMock
            .Setup(r => r.GetByCaseIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.GetCaseMetadataForCaseIdsAsync(caseIds));

        Assert.Same(expectedException, exception);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCaseMetadataForEgressWorkspaceIdsAsync_WhenWorkspaceIdsProvided_ReturnsMatchingRecords()
    {
        // Arrange
        var workspaceIds = _fixture.CreateMany<string>(3).ToList();
        var expectedMetadata = _fixture.CreateMany<CaseMetadata>(3).ToList();

        for (int i = 0; i < 3; i++)
        {
            expectedMetadata[i].EgressWorkspaceId = workspaceIds[i];
        }

        _repositoryMock
            .Setup(r => r.GetByEgressWorkspaceIdsAsync(It.Is<IEnumerable<string>>(ids =>
                ids.Count() == workspaceIds.Count &&
                ids.All(id => workspaceIds.Contains(id)))))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _service.GetCaseMetadataForEgressWorkspaceIdsAsync(workspaceIds);

        // Assert
        Assert.Equal(expectedMetadata, result);

        _repositoryMock.Verify(
            r => r.GetByEgressWorkspaceIdsAsync(It.Is<IEnumerable<string>>(ids =>
                ids.Count() == workspaceIds.Count &&
                ids.All(id => workspaceIds.Contains(id)))),
            Times.Once);
    }

    [Fact]
    public async Task GetCaseMetadataForEgressWorkspaceIdsAsync_WhenRepositoryThrowsException_LogsAndRethrows()
    {
        // Arrange
        var workspaceIds = _fixture.CreateMany<string>(3).ToList();
        var expectedException = new Exception("Repository error");

        _repositoryMock
            .Setup(r => r.GetByEgressWorkspaceIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.GetCaseMetadataForEgressWorkspaceIdsAsync(workspaceIds));

        Assert.Same(expectedException, exception);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCaseMetadataForCaseIdAsync_WhenCaseIdProvided_ReturnsMatchingRecord()
    {
        // Arrange
        var caseId = _fixture.Create<int>();
        var expectedMetadata = _fixture.Create<CaseMetadata>();
        expectedMetadata.CaseId = caseId;

        _repositoryMock
            .Setup(r => r.GetByCaseIdAsync(caseId))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _service.GetCaseMetadataForCaseIdAsync(caseId);

        // Assert
        Assert.Equal(expectedMetadata, result);

        _repositoryMock.Verify(
            r => r.GetByCaseIdAsync(caseId),
            Times.Once);
    }

    [Fact]
    public async Task GetCaseMetadataForCaseIdAsync_WhenRepositoryThrowsException_LogsAndRethrows()
    {
        // Arrange
        var caseId = _fixture.Create<int>();
        var expectedException = new Exception("Repository error");

        _repositoryMock
            .Setup(r => r.GetByCaseIdAsync(caseId))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.GetCaseMetadataForCaseIdAsync(caseId));

        Assert.Same(expectedException, exception);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ClearActiveTransferIdAsync_WhenMetadataExists_ClearsActiveTransferIdAndReturnsTrue()
    {
        // Arrange
        var transferId = _fixture.Create<Guid>();
        var existingMetadata = new CaseMetadata
        {
            CaseId = _fixture.Create<int>(),
            ActiveTransferId = transferId
        };

        _repositoryMock
            .Setup(r => r.GetByActiveTransferIdAsync(transferId))
            .ReturnsAsync(existingMetadata);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CaseMetadata>()))
            .ReturnsAsync(existingMetadata);

        // Act
        var result = await _service.ClearActiveTransferIdAsync(transferId);

        // Assert
        Assert.True(result);
        Assert.Null(existingMetadata.ActiveTransferId);

        _repositoryMock.Verify(
            r => r.GetByActiveTransferIdAsync(transferId),
            Times.Once);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.Is<CaseMetadata>(m =>
                m.CaseId == existingMetadata.CaseId &&
                m.ActiveTransferId == null)),
            Times.Once);
    }

    [Fact]
    public async Task ClearActiveTransferIdAsync_WhenMetadataDoesNotExist_LogsWarningAndReturnsFalse()
    {
        // Arrange
        var transferId = _fixture.Create<Guid>();

        _repositoryMock
            .Setup(r => r.GetByActiveTransferIdAsync(transferId))
            .ReturnsAsync((CaseMetadata?)null);

        // Act
        var result = await _service.ClearActiveTransferIdAsync(transferId);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            r => r.GetByActiveTransferIdAsync(transferId),
            Times.Once);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<CaseMetadata>()),
            Times.Never);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"No metadata found for transfer {transferId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ClearActiveTransferIdAsync_WhenRepositoryThrowsException_LogsAndRethrows()
    {
        // Arrange
        var transferId = _fixture.Create<Guid>();
        var expectedException = new Exception("Repository error");

        _repositoryMock
            .Setup(r => r.GetByActiveTransferIdAsync(transferId))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.ClearActiveTransferIdAsync(transferId));

        Assert.Same(expectedException, exception);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ClearActiveTransferIdAsync_WhenUpdateThrowsException_LogsAndRethrows()
    {
        // Arrange
        var transferId = _fixture.Create<Guid>();
        var existingMetadata = new CaseMetadata
        {
            CaseId = _fixture.Create<int>(),
            ActiveTransferId = transferId
        };
        var expectedException = new Exception("Update error");

        _repositoryMock
            .Setup(r => r.GetByActiveTransferIdAsync(transferId))
            .ReturnsAsync(existingMetadata);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CaseMetadata>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.ClearActiveTransferIdAsync(transferId));

        Assert.Same(expectedException, exception);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}