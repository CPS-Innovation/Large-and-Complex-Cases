using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Enums;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Results;
using CPS.ComplexCases.Common.Services;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class DisconnectEgressConnectionTests
    {
        private readonly Mock<ILogger<DisconnectEgressConnection>> _loggerMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly DisconnectEgressConnection _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;

        public DisconnectEgressConnectionTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<DisconnectEgressConnection>>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();

            _function = new DisconnectEgressConnection(
                _loggerMock.Object,
                _caseMetadataServiceMock.Object,
                _activityLogServiceMock.Object,
                _initializationHandlerMock.Object);
        }

        [Fact]
        public async Task Run_WhenCaseIdIsMissing_ReturnsBadRequest()
        {
            // Arrange
            var request = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Run_WhenCaseIdIsNotAnInteger_ReturnsBadRequest()
        {
            // Arrange
            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, "not-an-int", _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Run_WhenNoConnectionExists_ReturnsNotFound()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.NoCaseMetadataFound });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(caseId.ToString(), notFound.Value?.ToString());
        }

        [Fact]
        public async Task Run_WhenTransferIsActive_ReturnsConflict()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.TransferIsActive });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains(caseId.ToString(), conflict.Value?.ToString());
        }

        [Fact]
        public async Task Run_WhenEgressConnectionIsNull_ReturnsBadRequest()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.EgressConnectionIsNull });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains(caseId.ToString(), badRequest.Value?.ToString());
        }

        [Fact]
        public async Task Run_WhenSuccessful_ReturnsOk()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Run_WhenSuccessful_CreatesActivityLog()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.DisconnectionFromEgress,
                ActivityLog.Enums.ResourceType.StorageConnection,
                caseId,
                clearedPath,
                clearedPath,
                _testUsername,
                null),
                Times.Once);
        }

        [Fact]
        public async Task Run_WhenSuccessful_InitializesHandler()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _initializationHandlerMock.Verify(x => x.Initialize(_testUsername, _testCorrelationId, caseId), Times.Once);
        }

        [Fact]
        public async Task Run_WhenNoConnectionExists_DoesNotCreateActivityLog()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.NoCaseMetadataFound });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null),
                Times.Never);
        }

        [Fact]
        public async Task Run_WhenTransferIsActive_DoesNotCreateActivityLog()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.TransferIsActive });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null),
                Times.Never);
        }

        [Fact]
        public async Task Run_WhenEgressConnectionIsNull_DoesNotCreateActivityLog()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.EgressConnectionIsNull });

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null),
                Times.Never);
        }

        [Fact]
        public async Task Run_WhenActivityLogThrows_StillReturnsOk()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var clearedPath = _fixture.Create<string>();
            _caseMetadataServiceMock
                .Setup(x => x.ClearEgressConnectionAsync(caseId))
                .ReturnsAsync(new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = clearedPath });

            _activityLogServiceMock
                .Setup(x => x.CreateActivityLogAsync(
                    It.IsAny<ActivityLog.Enums.ActionType>(),
                    It.IsAny<ActivityLog.Enums.ResourceType>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    null))
                .ThrowsAsync(new Exception("Activity log unavailable"));

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameter(InputParameters.CaseId, caseId.ToString(), _testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert — disconnection succeeded; logging failure must not surface as an error
            Assert.IsType<OkResult>(result);
        }
    }
}
