using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Handlers;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Enums;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Results;
using CPS.ComplexCases.Common.Services;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Handlers
{
    public class DisconnectConnectionHandlerTests
    {
        private readonly Mock<ILogger<DisconnectConnectionHandler>> _loggerMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly DisconnectConnectionHandler _handler;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;

        public DisconnectConnectionHandlerTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<DisconnectConnectionHandler>>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();

            _handler = new DisconnectConnectionHandler(
                _loggerMock.Object,
                _initializationHandlerMock.Object,
                _caseMetadataServiceMock.Object,
                _activityLogServiceMock.Object);
        }

        [Fact]
        public async Task RunAsync_WhenCaseIdIsMissing_ReturnsBadRequest()
        {
            // Arrange
            var request = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RunAsync_WhenCaseIdIsNotAnInteger_ReturnsBadRequest()
        {
            // Arrange
            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, "not-an-int", _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RunAsync_WhenNoConnectionExists_ReturnsNotFound()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.NoCaseMetadataFound });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(caseId.ToString(), notFound.Value?.ToString());
        }

        [Fact]
        public async Task RunAsync_WhenTransferIsActive_ReturnsConflict()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.TransferIsActive });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains(caseId.ToString(), conflict.Value?.ToString());
        }

        [Fact]
        public async Task RunAsync_WhenSuccessful_ReturnsOk()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath, Key = key });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task RunAsync_WhenSuccessful_InitializesHandler()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath, Key = key });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            _initializationHandlerMock.Verify(x => x.Initialize(_testUsername, _testCorrelationId, caseId), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenNoConnectionExists_DoesNotCreateActivityLog()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.NoCaseMetadataFound });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActionType>(),
                It.IsAny<ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null),
                Times.Never);
        }

        [Fact]
        public async Task RunAsync_WhenTransferIsActive_DoesNotCreateActivityLog()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.TransferIsActive });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActionType>(),
                It.IsAny<ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null),
                Times.Never);
        }

        [Fact]
        public async Task RunAsync_WhenSuccessful_CallsClearConnectionOnService()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath, Key = key });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            _caseMetadataServiceMock.Verify(x => x.ClearNetAppFolderPathAsync(caseId), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenActivityLogThrows_StillReturnsOk()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath, Key = key });

            _activityLogServiceMock
                .Setup(x => x.CreateActivityLogAsync(
                    It.IsAny<ActionType>(),
                    It.IsAny<ResourceType>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    null))
                .ThrowsAsync(new Exception("Activity log unavailable"));

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task RunAsync_WhenActivityLogThrows_LogsTheError()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var activityLogException = new Exception("Activity log unavailable");

            _caseMetadataServiceMock
                .Setup(x => x.ClearNetAppFolderPathAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath, Key = key });

            _activityLogServiceMock
                .Setup(x => x.CreateActivityLogAsync(
                    It.IsAny<ActionType>(),
                    It.IsAny<ResourceType>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    null))
                .ThrowsAsync(activityLogException);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _handler.RunAsync(request, functionContext, StorageConnectionType.NetApp);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(caseId.ToString())),
                    activityLogException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(StorageConnectionType.NetApp, CaseMetadataState.NetAppFolderPathIsNull)]
        [InlineData(StorageConnectionType.Egress, CaseMetadataState.EgressConnectionIsNull)]
        public async Task RunAsync_WhenConnectionIsAlreadyNull_ReturnsBadRequest(
            StorageConnectionType connectionType, CaseMetadataState missingState)
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            SetupClearConnection(connectionType, caseId, new ClearFolderPathResult { State = missingState });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _handler.RunAsync(request, functionContext, connectionType);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains(caseId.ToString(), badRequest.Value?.ToString());
        }

        [Theory]
        [InlineData(StorageConnectionType.NetApp, CaseMetadataState.NetAppFolderPathIsNull)]
        [InlineData(StorageConnectionType.Egress, CaseMetadataState.EgressConnectionIsNull)]
        public async Task RunAsync_WhenConnectionIsAlreadyNull_DoesNotCreateActivityLog(
            StorageConnectionType connectionType, CaseMetadataState missingState)
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            SetupClearConnection(connectionType, caseId, new ClearFolderPathResult { State = missingState });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _handler.RunAsync(request, functionContext, connectionType);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActionType>(),
                It.IsAny<ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null),
                Times.Never);
        }

        [Theory]
        [InlineData(StorageConnectionType.NetApp, ActionType.DisconnectionFromNetApp)]
        [InlineData(StorageConnectionType.Egress, ActionType.DisconnectionFromEgress)]
        public async Task RunAsync_WhenSuccessful_CreatesActivityLogWithCorrectActionType(
            StorageConnectionType connectionType, ActionType expectedAction)
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            SetupClearConnection(connectionType, caseId, new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath, Key = key });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _handler.RunAsync(request, functionContext, connectionType);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                expectedAction,
                ResourceType.StorageConnection,
                caseId,
                key,
                clearedPath,
                _testUsername,
                null),
                Times.Once);
        }

        private void SetupClearConnection(StorageConnectionType connectionType, int caseId, ClearFolderPathResult result)
        {
            if (connectionType == StorageConnectionType.NetApp)
                _caseMetadataServiceMock.Setup(x => x.ClearNetAppFolderPathAsync(caseId)).ReturnsAsync(result);
            else
                _caseMetadataServiceMock.Setup(x => x.ClearEgressConnectionAsync(caseId)).ReturnsAsync(result);
        }
    }
}
