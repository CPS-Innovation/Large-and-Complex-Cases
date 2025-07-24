using System.Text.Json;
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
            .ReturnsAsync(activityLog)
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
            CreateActivityLogWithProperties("1", "Case"),
            CreateActivityLogWithProperties("2", "Document")
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

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithValidFilesAndErrors_ReturnsCorrectCsv()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""files"": [
                {""path"": ""C:\\Documents\\file1.txt""},
                {""path"": ""C:\\Documents\\file2.pdf""}
            ],
            ""errors"": [
                {""path"": ""C:\\Documents\\error1.doc""},
                {""path"": ""C:\\Documents\\error2.xlsx""}
            ]
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Verify CSV contains headers
        Assert.Contains("Path", result);
        Assert.Contains("FileName", result);
        Assert.Contains("Status", result);

        // Verify successful files are included
        Assert.Contains("C:\\Documents\\file1.txt", result);
        Assert.Contains("file1.txt", result);
        Assert.Contains("C:\\Documents\\file2.pdf", result);
        Assert.Contains("file2.pdf", result);

        // Verify error files are included
        Assert.Contains("C:\\Documents\\error1.doc", result);
        Assert.Contains("error1.doc", result);
        Assert.Contains("C:\\Documents\\error2.xlsx", result);
        Assert.Contains("error2.xlsx", result);

        // Verify status values
        Assert.Contains("Success", result);
        Assert.Contains("Fail", result);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithOnlyFiles_ReturnsCorrectCsv()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""files"": [
                {""path"": ""C:\\Documents\\success1.txt""},
                {""path"": ""C:\\Documents\\success2.pdf""}
            ]
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("success1.txt", result);
        Assert.Contains("success2.pdf", result);
        Assert.Contains("Success", result);
        Assert.DoesNotContain("Fail", result);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithOnlyErrors_ReturnsCorrectCsv()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""errors"": [
                {""path"": ""C:\\Documents\\error1.doc""},
                {""path"": ""C:\\Documents\\error2.xlsx""}
            ]
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("error1.doc", result);
        Assert.Contains("error2.xlsx", result);
        Assert.Contains("Fail", result);
        Assert.DoesNotContain("Success", result);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithEmptyFilesAndErrorsArrays_ReturnsEmptyString()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""files"": [],
            ""errors"": []
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.Equal(string.Empty, result);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("No file records to export")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithNullDetails_ReturnsEmptyString()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var activityLog = CreateActivityLogWithDetails(activityLogId, null);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.Equal(string.Empty, result);

        // Verify warning is logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Activity log details are null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithInvalidJsonStructure_ReturnsEmptyString()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"""this is not an object""");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.Equal(string.Empty, result);

        // Verify warning is logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Details is not a valid JSON object")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithMissingFilesAndErrorsProperties_ReturnsEmptyString()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""SomeOtherProperty"": ""value"",
            ""AnotherProperty"": 123
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.Equal(string.Empty, result);

        // Verify no file records logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("No file records to export")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithNullPathInFiles_HandlesGracefully()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""files"": [
                {""path"": null},
                {""path"": ""C:\\Documents\\valid.txt""},
                {}
            ]
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("valid.txt", result);
        Assert.Contains("Success", result);
        // Should handle null paths gracefully
        Assert.Contains(",,Success", result); // Empty path and filename with Success status
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithNonArrayFilesProperty_ReturnsEmptyString()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""files"": ""not an array"",
            ""errors"": ""also not an array""
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.Equal(string.Empty, result);

        // Verify no file records logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("No file records to export")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GenerateFileDetailsCsvAsync_WithFilePathsWithoutExtension_ExtractsFileNameCorrectly()
    {
        // Arrange
        var activityLogId = Guid.NewGuid();
        var jsonDetails = JsonDocument.Parse(@"{
            ""files"": [
                {""path"": ""C:\\Documents\\README""},
                {""path"": ""C:\\Documents\\folder\\script""}
            ]
        }");

        var activityLog = CreateActivityLogWithDetails(activityLogId, jsonDetails);

        // Act
        var result = _service.GenerateFileDetailsCsvAsync(activityLog);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("README", result);
        Assert.Contains("script", result);
        Assert.Contains("Success", result);
    }

    private Data.Entities.ActivityLog CreateActivityLogWithProperties(string resourceId, string resourceType)
    {
        var activityLog = new Data.Entities.ActivityLog();
        activityLog.ResourceId = resourceId;
        activityLog.ResourceType = resourceType;
        return activityLog;
    }

    private Data.Entities.ActivityLog CreateActivityLogWithDetails(Guid id, JsonDocument? details)
    {
        var activityLog = new Data.Entities.ActivityLog();
        activityLog.Details = details;

        return activityLog;
    }
}