using System.Text;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class DownloadActivityLogTests
    {
        private readonly Mock<ILogger<DownloadActivityLog>> _loggerMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly DownloadActivityLog _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;

        public DownloadActivityLogTests()
        {
            _fixture = new Fixture();

            _fixture.Customize<Data.Entities.ActivityLog>(composer =>
                composer.Without(x => x.Details));

            _loggerMock = new Mock<ILogger<DownloadActivityLog>>();
            _activityLogServiceMock = new Mock<IActivityLogService>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();

            _function = new DownloadActivityLog(
                _loggerMock.Object,
                _activityLogServiceMock.Object);
        }

        [Fact]
        public async Task Run_ActivityLogNotFound_ReturnsNotFound()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync((Data.Entities.ActivityLog?)null);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext, activityId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Activity log not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Run_EmptyCsvContent_ReturnsBadRequest()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();
            var activityLog = _fixture.Create<Data.Entities.ActivityLog>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync(activityLog);

            _activityLogServiceMock
                .Setup(x => x.GenerateFileDetailsCsvAsync(activityLog))
                .Returns(string.Empty);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext, activityId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file paths found in activity log details", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_NullCsvContent_ReturnsBadRequest()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();
            var activityLog = _fixture.Create<Data.Entities.ActivityLog>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync(activityLog);

            _activityLogServiceMock
                .Setup(x => x.GenerateFileDetailsCsvAsync(activityLog))
                .Returns((string?)null!);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext, activityId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file paths found in activity log details", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_ValidActivityLogWithCsvContent_ReturnsFileContentResult()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();
            var activityLog = _fixture.Create<Data.Entities.ActivityLog>();
            var csvContent = "Header1,Header2\nValue1,Value2\nValue3,Value4";

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync(activityLog);

            _activityLogServiceMock
                .Setup(x => x.GenerateFileDetailsCsvAsync(activityLog))
                .Returns(csvContent);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext, activityId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal(Encoding.UTF8.GetBytes(csvContent), fileResult.FileContents);
            Assert.Contains($"activity-log-{activityId}-files-", fileResult.FileDownloadName);
            Assert.EndsWith(".csv", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task Run_ActivityLogServiceCalledWithCorrectActivityId()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();
            var activityLog = _fixture.Create<Data.Entities.ActivityLog>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync(activityLog);

            _activityLogServiceMock
                .Setup(x => x.GenerateFileDetailsCsvAsync(activityLog))
                .Returns(string.Empty);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext, activityId);

            // Assert
            _activityLogServiceMock.Verify(x => x.GetActivityLogByIdAsync(activityId), Times.Once);
        }

        [Fact]
        public async Task Run_GenerateFileDetailsCsvCalledWithCorrectActivityLog()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();
            var activityLog = _fixture.Create<Data.Entities.ActivityLog>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync(activityLog);

            _activityLogServiceMock
                .Setup(x => x.GenerateFileDetailsCsvAsync(activityLog))
                .Returns(string.Empty);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext, activityId);

            // Assert
            _activityLogServiceMock.Verify(x => x.GenerateFileDetailsCsvAsync(activityLog), Times.Once);
        }

        [Fact]
        public async Task Run_FileNameContainsTimestamp()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();
            var activityLog = _fixture.Create<Data.Entities.ActivityLog>();
            var csvContent = "test,content";

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync(activityLog);

            _activityLogServiceMock
                .Setup(x => x.GenerateFileDetailsCsvAsync(activityLog))
                .Returns(csvContent);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext, activityId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);

            // Verify filename format: activity-log-{activityId}-files-{timestamp}.csv
            var expectedPrefix = $"activity-log-{activityId}-files-";
            var expectedSuffix = ".csv";

            Assert.StartsWith(expectedPrefix, fileResult.FileDownloadName);
            Assert.EndsWith(expectedSuffix, fileResult.FileDownloadName);

            // Extract timestamp part and verify it's in correct format (yyyyMMdd-HHmmss)
            var timestampPart = fileResult.FileDownloadName
                .Substring(expectedPrefix.Length, fileResult.FileDownloadName.Length - expectedPrefix.Length - expectedSuffix.Length);

            Assert.Matches(@"^\d{8}-\d{6}$", timestampPart);
        }

        [Fact]
        public async Task Run_DoesNotCallGenerateFileDetailsCsvWhenActivityLogNotFound()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync((Data.Entities.ActivityLog?)null);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext, activityId);

            // Assert
            _activityLogServiceMock.Verify(x => x.GenerateFileDetailsCsvAsync(It.IsAny<Data.Entities.ActivityLog>()), Times.Never);
        }

        [Fact]
        public async Task Run_LogsInformationWhenFunctionTriggered()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync((Data.Entities.ActivityLog?)null);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext, activityId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"DownloadActivityLog function triggered for activityId: {activityId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Run_LogsWarningWhenActivityLogNotFound()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync((Data.Entities.ActivityLog?)null);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext, activityId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Activity log not found for ID: {activityId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Run_LogsInformationWhenNoFilePathsFound()
        {
            // Arrange
            var activityId = _fixture.Create<Guid>();
            var activityLog = _fixture.Create<Data.Entities.ActivityLog>();

            _activityLogServiceMock
                .Setup(x => x.GetActivityLogByIdAsync(activityId))
                .ReturnsAsync(activityLog);

            _activityLogServiceMock
                .Setup(x => x.GenerateFileDetailsCsvAsync(activityLog))
                .Returns(string.Empty);

            var request = HttpRequestStubHelper.CreateHttpRequest();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext, activityId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"No file paths found in activity log details for ID: {activityId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}