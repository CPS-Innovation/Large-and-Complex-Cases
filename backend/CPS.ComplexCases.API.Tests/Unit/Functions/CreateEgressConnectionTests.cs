using AutoFixture;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class CreateEgressConnectionTests
    {
        private readonly Mock<ILogger<CreateEgressConnection>> _loggerMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<IEgressClient> _egressClientMock;
        private readonly Mock<IEgressArgFactory> _egressArgFactoryMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IRequestValidator> _requestValidatorMock;
        private readonly CreateEgressConnection _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;

        public CreateEgressConnectionTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<CreateEgressConnection>>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _egressClientMock = new Mock<IEgressClient>();
            _egressArgFactoryMock = new Mock<IEgressArgFactory>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _requestValidatorMock = new Mock<IRequestValidator>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();

            _function = new CreateEgressConnection(
                _caseMetadataServiceMock.Object,
                _egressClientMock.Object,
                _egressArgFactoryMock.Object,
                _loggerMock.Object,
                _activityLogServiceMock.Object,
                _requestValidatorMock.Object);
        }

        [Fact]
        public async Task Run_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var validationErrors = _fixture.CreateMany<string>(2).ToList();
            var egressConnectionRequest = _fixture.Create<CreateEgressConnectionDto>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateEgressConnectionDto>
                {
                    IsValid = false,
                    ValidationErrors = validationErrors,
                    Value = egressConnectionRequest
                });

            var request = HttpRequestStubHelper.CreateHttpRequestFor(egressConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
            Assert.Equal(validationErrors, errors);
        }

        [Fact]
        public async Task Run_UserDoesNotHaveEgressPermission_ReturnsUnauthorized()
        {
            // Arrange
            var egressConnectionRequest = _fixture.Create<CreateEgressConnectionDto>();
            var egressArg = _fixture.Create<GetWorkspacePermissionArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateEgressConnectionDto>
                {
                    IsValid = true,
                    Value = egressConnectionRequest
                });

            _egressArgFactoryMock
                .Setup(x => x.CreateGetWorkspacePermissionArg(egressConnectionRequest.EgressWorkspaceId, _testUsername))
                .Returns(egressArg);

            _egressClientMock
                .Setup(x => x.GetWorkspacePermission(egressArg))
                .ReturnsAsync(false);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(egressConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Run_ValidRequestWithPermission_CreatesConnectionAndReturnsOk()
        {
            // Arrange
            var egressConnectionRequest = _fixture.Create<CreateEgressConnectionDto>();
            var egressArg = _fixture.Create<GetWorkspacePermissionArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateEgressConnectionDto>
                {
                    IsValid = true,
                    Value = egressConnectionRequest
                });

            _egressArgFactoryMock
                .Setup(x => x.CreateGetWorkspacePermissionArg(egressConnectionRequest.EgressWorkspaceId, _testUsername))
                .Returns(egressArg);

            _egressClientMock
                .Setup(x => x.GetWorkspacePermission(egressArg))
                .ReturnsAsync(true);

            _caseMetadataServiceMock
                .Setup(x => x.CreateEgressConnectionAsync(egressConnectionRequest))
                .Returns(Task.CompletedTask);

            _activityLogServiceMock
                .Setup(x => x.CreateActivityLogAsync(
                    ActivityLog.Enums.ActionType.ConnectionToEgress,
                    ActivityLog.Enums.ResourceType.StorageConnection,
                    egressConnectionRequest.CaseId,
                    egressConnectionRequest.EgressWorkspaceId,
                    egressConnectionRequest.EgressWorkspaceId,
                    _testUsername,
                    null))
                .Returns(Task.CompletedTask);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(egressConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<OkResult>(result);

            _caseMetadataServiceMock.Verify(x => x.CreateEgressConnectionAsync(egressConnectionRequest), Times.Once);
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.ConnectionToEgress,
                ActivityLog.Enums.ResourceType.StorageConnection,
                egressConnectionRequest.CaseId,
                egressConnectionRequest.EgressWorkspaceId,
                egressConnectionRequest.EgressWorkspaceId,
                _testUsername,
                null), Times.Once);
        }

        [Fact]
        public async Task Run_EgressArgFactoryCalledWithCorrectParameters()
        {
            // Arrange
            var egressConnectionRequest = _fixture.Create<CreateEgressConnectionDto>();
            var egressArg = _fixture.Create<GetWorkspacePermissionArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateEgressConnectionDto>
                {
                    IsValid = true,
                    Value = egressConnectionRequest
                });

            _egressArgFactoryMock
                .Setup(x => x.CreateGetWorkspacePermissionArg(egressConnectionRequest.EgressWorkspaceId, _testUsername))
                .Returns(egressArg);

            _egressClientMock
                .Setup(x => x.GetWorkspacePermission(egressArg))
                .ReturnsAsync(false);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(egressConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _egressArgFactoryMock.Verify(x => x.CreateGetWorkspacePermissionArg(
                egressConnectionRequest.EgressWorkspaceId,
                _testUsername), Times.Once);
        }

        [Fact]
        public async Task Run_EgressClientCalledWithCorrectArg()
        {
            // Arrange
            var egressConnectionRequest = _fixture.Create<CreateEgressConnectionDto>();
            var egressArg = _fixture.Create<GetWorkspacePermissionArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateEgressConnectionDto>
                {
                    IsValid = true,
                    Value = egressConnectionRequest
                });

            _egressArgFactoryMock
                .Setup(x => x.CreateGetWorkspacePermissionArg(egressConnectionRequest.EgressWorkspaceId, _testUsername))
                .Returns(egressArg);

            _egressClientMock
                .Setup(x => x.GetWorkspacePermission(egressArg))
                .ReturnsAsync(false);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(egressConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _egressClientMock.Verify(x => x.GetWorkspacePermission(egressArg), Times.Once);
        }

        [Fact]
        public async Task Run_OnlyCallsCaseMetadataAndActivityLogWhenUserHasPermission()
        {
            // Arrange
            var egressConnectionRequest = _fixture.Create<CreateEgressConnectionDto>();
            var egressArg = _fixture.Create<GetWorkspacePermissionArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateEgressConnectionDto>
                {
                    IsValid = true,
                    Value = egressConnectionRequest
                });

            _egressArgFactoryMock
                .Setup(x => x.CreateGetWorkspacePermissionArg(egressConnectionRequest.EgressWorkspaceId, _testUsername))
                .Returns(egressArg);

            _egressClientMock
                .Setup(x => x.GetWorkspacePermission(egressArg))
                .ReturnsAsync(false);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(egressConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _caseMetadataServiceMock.Verify(x => x.CreateEgressConnectionAsync(It.IsAny<CreateEgressConnectionDto>()), Times.Never);
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<System.Text.Json.JsonDocument>()), Times.Never);
        }
    }
}