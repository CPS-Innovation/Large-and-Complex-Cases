using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Data.Dtos;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.ActivityLog.Tests.Unit.Services;

public class ActivityLogServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IActivityLogRepository> _repositoryMock;
    private readonly Mock<ILogger<ActivityLogService>> _loggerMock;
    private readonly ActivityLogService _service;

    private readonly int _testCaseId;
    private readonly string _testResourceId;
    private readonly string _testResourceName;
    private readonly string _testUserName;

    public ActivityLogServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _repositoryMock = _fixture.Freeze<Mock<IActivityLogRepository>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<ActivityLogService>>>();

        _testCaseId = _fixture.Create<int>();
        _testResourceId = _fixture.Create<string>();
        _testResourceName = _fixture.Create<string>();
        _testUserName = _fixture.Create<string>();

        _service = new ActivityLogService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddActivityLogAsync_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var activityLog = new Data.Entities.ActivityLog(
            Guid.NewGuid(),
            ActionType.TransferInitiated.GetAlternateValue(),
            ResourceType.FileTransfer.ToString(),
            _testResourceId,
            _testUserName);
        _repositoryMock
            .Setup(r => r.AddAsync(activityLog))
            .Returns(Task.FromResult(activityLog))
            .Verifiable();

        // Act
        await _service.CreateActivityLogAsync(ActionType.TransferInitiated, ResourceType.FileTransfer, _testCaseId, _testResourceId, _testResourceName, _testUserName);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Data.Entities.ActivityLog>(a =>
            a.ActionType == activityLog.ActionType &&
            a.ResourceType == activityLog.ResourceType &&
            a.ResourceId == activityLog.ResourceId &&
            a.UserName == activityLog.UserName)), Times.Once);
    }

    [Fact]
    public async Task GetActivityLogByIdAsync_ReturnsAuditLog()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new Data.Entities.ActivityLog(
            id,
            ActionType.TransferInitiated.GetAlternateValue(),
            ResourceType.FileTransfer.ToString(),
            _testResourceId,
            _testUserName);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetActivityLogByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.ActionType, result.ActionType);
        Assert.Equal(expected.ResourceType, result.ResourceType);
        Assert.Equal(expected.ResourceId, result.ResourceId);
        Assert.Equal(expected.UserName, result.UserName);
    }

    [Fact]
    public async Task GetActivityLogByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Data.Entities.ActivityLog?)null);

        // Act
        var result = await _service.GetActivityLogByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActivityLogsByResourceIdAsync_ReturnsAuditLogs()
    {
        // Arrange
        var resourceId = _fixture.Create<string>();
        var expected = new List<Data.Entities.ActivityLog>
        {
            new(Guid.NewGuid(), ActionType.TransferInitiated.GetAlternateValue(), ResourceType.FileTransfer.ToString(), resourceId, _testUserName)
        };
        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync(resourceId))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetActivityLogsByResourceIdAsync(resourceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Count, result.Count());
        var expectedList = expected.ToList();
        var resultList = result.ToList();
        for (int i = 0; i < expectedList.Count; i++)
        {
            Assert.Equal(expectedList[i].Id, resultList[i].Id);
            Assert.Equal(expectedList[i].ActionType, resultList[i].ActionType);
            Assert.Equal(expectedList[i].ResourceType, resultList[i].ResourceType);
            Assert.Equal(expectedList[i].ResourceId, resultList[i].ResourceId);
            Assert.Equal(expectedList[i].UserName, resultList[i].UserName);
        }
    }

    [Fact]
    public async Task GetActivitytLogsByResourceIdAsync_NonExistentResourceId_ReturnsEmptyList()
    {
        // Arrange
        var resourceId = _fixture.Create<string>();
        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync(resourceId))
            .ReturnsAsync(new List<Data.Entities.ActivityLog>());

        // Act
        var result = await _service.GetActivityLogsByResourceIdAsync(resourceId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateActivityLogAsync_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var activityLog = new Data.Entities.ActivityLog(
            id,
            ActionType.TransferCompleted.GetAlternateValue(),
            ResourceType.FileTransfer.ToString(),
            _testResourceId,
            _testUserName);
        _repositoryMock
            .Setup(r => r.UpdateAsync(activityLog))
            .Returns(Task.FromResult(activityLog))
            .Verifiable();

        // Act
        await _service.UpdateActivityLogAsync(activityLog);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Data.Entities.ActivityLog>(a =>
            a.Id == activityLog.Id &&
            a.ActionType == activityLog.ActionType &&
            a.ResourceType == activityLog.ResourceType &&
            a.ResourceId == activityLog.ResourceId &&
            a.UserName == activityLog.UserName)), Times.Once);
    }

    [Fact]
    public async Task GetActivityLogsAsync_ShouldReturnLogs_WhenRepositoryReturnsLogs()
    {
        // Arrange
        var filter = new ActivityLogFilterDto();
        var logs = new List<Data.Entities.ActivityLog>
        {
            new Data.Entities.ActivityLog { ResourceId = "1", ResourceType = "Case" },
            new Data.Entities.ActivityLog { ResourceId = "2", ResourceType = "Document" }
        };
        var activityLogResultsDto = new ActivityLogResultsDto
        {
            Logs = logs,
            TotalCount = logs.Count,
            Skip = 0,
            Take = logs.Count
        };

        _repositoryMock
            .Setup(r => r.GetByFilterAsync(filter))
            .ReturnsAsync(activityLogResultsDto);

        // Act
        var result = await _service.GetActivityLogsAsync(filter);

        // Assert
        Assert.NotNull(result);
        var resultList = result.Data.ToList();
        Assert.Equal("1", resultList[0].ResourceId);
        Assert.Equal("2", resultList[1].ResourceId);
        _repositoryMock.Verify(r => r.GetByFilterAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetActivityLogsAsync_ShouldThrow_WhenRepositoryThrows()
    {
        // Arrange
        var filter = new ActivityLogFilterDto();
        _repositoryMock
            .Setup(r => r.GetByFilterAsync(filter))
            .ThrowsAsync(new Exception("Repository error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetActivityLogsAsync(filter));
        _repositoryMock.Verify(r => r.GetByFilterAsync(filter), Times.Once);
    }
}
