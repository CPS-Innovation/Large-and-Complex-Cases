using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.ActivityLog.Attributes;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Data.Repositories;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.ActivityLog.Tests.Unit.Services;

public class ActivityLogServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IActivityLogRepository> _repositoryMock;
    private readonly Mock<ILogger<ActivityLogService>> _loggerMock;
    private readonly ActivityLogService _service;
    private const string TestResourceId = "TestResourceId";
    private const string TestResourceName = "TestResourceName";
    private const string TestUserName = "TestUserName";

    public ActivityLogServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _repositoryMock = _fixture.Freeze<Mock<IActivityLogRepository>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<ActivityLogService>>>();

        _service = new ActivityLogService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddAuditLogAsync_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var activityLog = new Data.Entities.ActivityLog(
            Guid.NewGuid(),
            ActionType.TransferInitiated.GetAlternateValue(),
            ResourceType.FileTransfer.ToString(),
            TestResourceId,
            TestUserName);
        _repositoryMock
            .Setup(r => r.AddAsync(activityLog))
            .Returns(Task.FromResult(activityLog))
            .Verifiable();

        // Act
        await _service.CreateActivityLogAsync(ActionType.TransferInitiated, ResourceType.FileTransfer, TestResourceId, TestResourceName, TestUserName);

        // Assert
        using (new AssertionScope())
        {
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Data.Entities.ActivityLog>(a =>
                a.ActionType == activityLog.ActionType &&
                a.ResourceType == activityLog.ResourceType &&
                a.ResourceId == activityLog.ResourceId &&
                a.UserName == activityLog.UserName)), Times.Once);
        }
    }

    [Fact]
    public async Task GetAuditLogByIdAsync_ReturnsAuditLog()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new Data.Entities.ActivityLog(
            id,
            ActionType.TransferInitiated.GetAlternateValue(),
            ResourceType.FileTransfer.ToString(),
            TestResourceId,
            TestUserName);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetActivityLogByIdAsync(id);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public async Task GetAuditLogByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Data.Entities.ActivityLog?)null);

        // Act
        var result = await _service.GetActivityLogByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAuditLogsByResourceIdAsync_ReturnsAuditLogs()
    {
        // Arrange
        var resourceId = "TestResourceId";
        var expected = new List<Data.Entities.ActivityLog>
        {
            new(Guid.NewGuid(), ActionType.TransferInitiated.GetAlternateValue(), ResourceType.FileTransfer.ToString(), resourceId, TestUserName)
        };
        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync(resourceId))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetActivityLogsByResourceIdAsync(resourceId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count().Should().Be(expected.Count);
            result.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public async Task GetAuditLogsByResourceIdAsync_NonExistentResourceId_ReturnsEmptyList()
    {
        // Arrange
        var resourceId = "TestResourceId";
        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync(resourceId))
            .ReturnsAsync(new List<Data.Entities.ActivityLog>());

        // Act
        var result = await _service.GetActivityLogsByResourceIdAsync(resourceId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAuditLogAsync_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var activityLog = new Data.Entities.ActivityLog(
            id,
            ActionType.TransferCompleted.GetAlternateValue(),
            ResourceType.FileTransfer.ToString(),
            TestResourceId,
            TestUserName);
        _repositoryMock
            .Setup(r => r.UpdateAsync(activityLog))
            .Returns(Task.FromResult(activityLog))
            .Verifiable();

        // Act
        await _service.UpdateActivityLogAsync(activityLog);

        // Assert
        using (new AssertionScope())
        {
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Data.Entities.ActivityLog>(a =>
                a.Id == activityLog.Id &&
                a.ActionType == activityLog.ActionType &&
                a.ResourceType == activityLog.ResourceType &&
                a.ResourceId == activityLog.ResourceId &&
                a.UserName == activityLog.UserName)), Times.Once);
        }
    }
}
