using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.Egress.Models.Response;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class CreateEgressWorkspaceTests
    {
        private readonly Mock<ILogger<CreateEgressWorkspace>> _loggerMock;
        private readonly Mock<IEgressClient> _egressClientMock;
        private readonly Mock<IEgressArgFactory> _egressArgFactoryMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IRequestValidator> _requestValidatorMock;
        private readonly Fixture _fixture;
        private readonly CreateEgressWorkspace _function;
        private readonly Mock<IDdeiClient> _ddeiClientMock;
        private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
        private readonly Guid _correlationId;
        private readonly string _username;
        private readonly string _cmsAuthValues;
        private readonly string _bearerToken;
        private readonly int _caseId;
        private readonly string _description;
        private readonly string _templateId;
        private readonly string _workspaceId;
        private readonly string _administratorRoleId;
        private readonly string _operationName;
        private readonly string _urn;
        private readonly string _leadDefendantName;

        public CreateEgressWorkspaceTests()
        {
            _loggerMock = new Mock<ILogger<CreateEgressWorkspace>>();
            _egressClientMock = new Mock<IEgressClient>();
            _egressArgFactoryMock = new Mock<IEgressArgFactory>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _requestValidatorMock = new Mock<IRequestValidator>();
            _ddeiClientMock = new Mock<IDdeiClient>();
            _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();

            _fixture = new Fixture();
            _function = new CreateEgressWorkspace(
                _caseMetadataServiceMock.Object,
                _egressClientMock.Object,
                _egressArgFactoryMock.Object,
                _ddeiClientMock.Object,
                _ddeiArgFactoryMock.Object,
                _loggerMock.Object,
                _activityLogServiceMock.Object,
                _requestValidatorMock.Object);

            _correlationId = _fixture.Create<Guid>();
            _cmsAuthValues = _fixture.Create<string>();
            _username = _fixture.Create<string>();
            _bearerToken = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();
            _description = _fixture.Create<string>();
            _templateId = _fixture.Create<string>();
            _workspaceId = _fixture.Create<string>();
            _administratorRoleId = _fixture.Create<string>();
            _operationName = _fixture.Create<string>();
            _urn = _fixture.Create<string>();
            _leadDefendantName = _fixture.Create<string>();
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithWorkspace_WhenRequestIsValid_AndOperationNameExists()
        {
            // Arrange
            var createRequest = new CreateEgressWorkspaceRequest
            {
                CaseId = _caseId,
                Description = _description,
                TemplateId = _templateId
            };

            var validationResult = new ValidatableRequest<CreateEgressWorkspaceRequest>
            {
                IsValid = true,
                Value = createRequest
            };

            var caseResponse = new CaseDto
            {
                OperationName = _operationName,
                Urn = _urn,
                LeadDefendantSurname = _leadDefendantName
            };

            var expectedWorkspaceName = $"{_operationName}-{_urn}";

            var workspace = new CreateWorkspaceResponse
            {
                Id = _workspaceId,
                Name = expectedWorkspaceName,
                Description = _description
            };

            var roles = new List<ListWorkspaceRoleDto>
            {
                new ListWorkspaceRoleDto { RoleId = _administratorRoleId, RoleName = "Administrator" }
            };

            var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
            var createWorkspaceArg = _fixture.Create<CreateEgressWorkspaceArg>();
            var grantPermissionArg = _fixture.Create<GrantWorkspacePermissionArg>();

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<CreateEgressWorkspaceRequest, CreateEgressWorkspaceRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(validationResult);

            _ddeiArgFactoryMock
                .Setup(f => f.CreateCaseArg(_cmsAuthValues, _correlationId, _caseId))
                .Returns(cmsArg);

            _ddeiClientMock
                .Setup(c => c.GetCaseAsync(cmsArg))
                .ReturnsAsync(caseResponse);

            _egressArgFactoryMock
                .Setup(f => f.CreateEgressWorkspaceArg(expectedWorkspaceName, _description, _templateId))
                .Returns(createWorkspaceArg);

            _egressClientMock
                .Setup(c => c.CreateWorkspaceAsync(createWorkspaceArg))
                .ReturnsAsync(workspace);

            _egressClientMock
                .Setup(c => c.ListWorkspaceRolesAsync(_workspaceId))
                .ReturnsAsync(roles);

            _egressArgFactoryMock
                .Setup(f => f.CreateGrantWorkspacePermissionArg(_workspaceId, _username, _administratorRoleId))
                .Returns(grantPermissionArg);

            _egressClientMock
                .Setup(c => c.GrantWorkspacePermission(grantPermissionArg))
                .Returns(Task.CompletedTask);

            _caseMetadataServiceMock
                .Setup(s => s.CreateEgressConnectionAsync(It.Is<CreateEgressConnectionDto>(
                    dto => dto.CaseId == _caseId && dto.EgressWorkspaceId == _workspaceId)))
                .Returns(Task.CompletedTask);

            _activityLogServiceMock
                .Setup(s => s.CreateActivityLogAsync(
                    ActionType.ConnectionToEgress,
                    ResourceType.StorageConnection,
                    _caseId,
                    _workspaceId,
                    _workspaceId,
                    _username, null))
                .Returns(Task.CompletedTask);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), _correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(workspace, okResult.Value);

            _requestValidatorMock.Verify(v => v.GetJsonBody<CreateEgressWorkspaceRequest, CreateEgressWorkspaceRequestValidator>(httpRequest), Times.Once);
            _ddeiArgFactoryMock.Verify(f => f.CreateCaseArg(It.IsAny<string>(), _correlationId, _caseId), Times.Once);
            _ddeiClientMock.Verify(c => c.GetCaseAsync(cmsArg), Times.Once);
            _egressArgFactoryMock.Verify(f => f.CreateEgressWorkspaceArg(expectedWorkspaceName, _description, _templateId), Times.Once);
            _egressClientMock.Verify(c => c.CreateWorkspaceAsync(createWorkspaceArg), Times.Once);
            _egressClientMock.Verify(c => c.ListWorkspaceRolesAsync(_workspaceId), Times.Once);
            _egressArgFactoryMock.Verify(f => f.CreateGrantWorkspacePermissionArg(_workspaceId, _username, _administratorRoleId), Times.Once);
            _egressClientMock.Verify(c => c.GrantWorkspacePermission(grantPermissionArg), Times.Once);
            _caseMetadataServiceMock.Verify(s => s.CreateEgressConnectionAsync(It.Is<CreateEgressConnectionDto>(
                dto => dto.CaseId == _caseId && dto.EgressWorkspaceId == _workspaceId)), Times.Once);
            _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
                ActionType.ConnectionToEgress,
                ResourceType.StorageConnection,
                _caseId,
                _workspaceId,
                _workspaceId,
                _username,
                null), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithWorkspace_WhenRequestIsValid_AndOperationNameIsEmpty()
        {
            // Arrange
            var createRequest = new CreateEgressWorkspaceRequest
            {
                CaseId = _caseId,
                Description = _description,
                TemplateId = _templateId
            };

            var validationResult = new ValidatableRequest<CreateEgressWorkspaceRequest>
            {
                IsValid = true,
                Value = createRequest
            };

            var caseResponse = new CaseDto
            {
                OperationName = string.Empty,
                Urn = _urn,
                LeadDefendantSurname = _leadDefendantName
            };

            var expectedWorkspaceName = $"{_leadDefendantName}-{_urn}";

            var workspace = new CreateWorkspaceResponse
            {
                Id = _workspaceId,
                Name = expectedWorkspaceName,
                Description = _description
            };

            var roles = new List<ListWorkspaceRoleDto>
            {
                new ListWorkspaceRoleDto { RoleId = _administratorRoleId, RoleName = "Administrator" }
            };

            var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
            var createWorkspaceArg = _fixture.Create<CreateEgressWorkspaceArg>();
            var grantPermissionArg = _fixture.Create<GrantWorkspacePermissionArg>();

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<CreateEgressWorkspaceRequest, CreateEgressWorkspaceRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(validationResult);

            _ddeiArgFactoryMock
                .Setup(f => f.CreateCaseArg(_cmsAuthValues, _correlationId, _caseId))
                .Returns(cmsArg);

            _ddeiClientMock
                .Setup(c => c.GetCaseAsync(cmsArg))
                .ReturnsAsync(caseResponse);

            _egressArgFactoryMock
                .Setup(f => f.CreateEgressWorkspaceArg(expectedWorkspaceName, _description, _templateId))
                .Returns(createWorkspaceArg);

            _egressClientMock
                .Setup(c => c.CreateWorkspaceAsync(createWorkspaceArg))
                .ReturnsAsync(workspace);

            _egressClientMock
                .Setup(c => c.ListWorkspaceRolesAsync(_workspaceId))
                .ReturnsAsync(roles);

            _egressArgFactoryMock
                .Setup(f => f.CreateGrantWorkspacePermissionArg(_workspaceId, _username, _administratorRoleId))
                .Returns(grantPermissionArg);

            _egressClientMock
                .Setup(c => c.GrantWorkspacePermission(grantPermissionArg))
                .Returns(Task.CompletedTask);

            _caseMetadataServiceMock
                .Setup(s => s.CreateEgressConnectionAsync(It.Is<CreateEgressConnectionDto>(
                    dto => dto.CaseId == _caseId && dto.EgressWorkspaceId == _workspaceId)))
                .Returns(Task.CompletedTask);

            _activityLogServiceMock
                .Setup(s => s.CreateActivityLogAsync(
                    ActionType.ConnectionToEgress,
                    ResourceType.StorageConnection,
                    _caseId,
                    _workspaceId,
                    _workspaceId,
                    _username, null))
                .Returns(Task.CompletedTask);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), _correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(workspace, okResult.Value);

            _egressArgFactoryMock.Verify(f => f.CreateEgressWorkspaceArg(expectedWorkspaceName, _description, _templateId), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsBadRequest_WhenRequestIsInvalid()
        {
            // Arrange
            var validationErrors = _fixture.Create<List<string>>();

            var validationResult = new ValidatableRequest<CreateEgressWorkspaceRequest>
            {
                IsValid = false,
                ValidationErrors = validationErrors,
                Value = null!
            };

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<CreateEgressWorkspaceRequest, CreateEgressWorkspaceRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(validationResult);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _username, _bearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), _correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(validationErrors, badRequestResult.Value);

            _requestValidatorMock.Verify(v => v.GetJsonBody<CreateEgressWorkspaceRequest, CreateEgressWorkspaceRequestValidator>(httpRequest), Times.Once);
            _ddeiClientMock.Verify(c => c.GetCaseAsync(It.IsAny<DdeiCaseIdArgDto>()), Times.Never);
            _egressClientMock.Verify(c => c.CreateWorkspaceAsync(It.IsAny<CreateEgressWorkspaceArg>()), Times.Never);
        }

        [Fact]
        public async Task Run_ThrowsException_WhenAdministratorRoleNotFound()
        {
            // Arrange
            var createRequest = new CreateEgressWorkspaceRequest
            {
                CaseId = _caseId,
                Description = _description,
                TemplateId = _templateId
            };

            var validationResult = new ValidatableRequest<CreateEgressWorkspaceRequest>
            {
                IsValid = true,
                Value = createRequest
            };

            var caseResponse = new CaseDto
            {
                OperationName = _operationName,
                Urn = _urn,
                LeadDefendantSurname = _leadDefendantName
            };

            var expectedWorkspaceName = $"{_operationName}-{_urn}";

            var workspace = new CreateWorkspaceResponse
            {
                Id = _workspaceId,
                Name = expectedWorkspaceName,
                Description = _description
            };

            var roles = new List<ListWorkspaceRoleDto>
            {
                new ListWorkspaceRoleDto { RoleId = _fixture.Create<string>(), RoleName = "OtherRole" }
            };

            var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
            var createWorkspaceArg = _fixture.Create<CreateEgressWorkspaceArg>();

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<CreateEgressWorkspaceRequest, CreateEgressWorkspaceRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(validationResult);

            _ddeiArgFactoryMock
                .Setup(f => f.CreateCaseArg(_cmsAuthValues, _correlationId, _caseId))
                .Returns(cmsArg);

            _ddeiClientMock
                .Setup(c => c.GetCaseAsync(cmsArg))
                .ReturnsAsync(caseResponse);

            _egressArgFactoryMock
                .Setup(f => f.CreateEgressWorkspaceArg(expectedWorkspaceName, _description, _templateId))
                .Returns(createWorkspaceArg);

            _egressClientMock
                .Setup(c => c.CreateWorkspaceAsync(createWorkspaceArg))
                .ReturnsAsync(workspace);

            _egressClientMock
                .Setup(c => c.ListWorkspaceRolesAsync(_workspaceId))
                .ReturnsAsync(roles);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), _correlationId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _function.Run(httpRequest, functionContext));
            Assert.Equal("Administrator role not found for workspace", exception.Message);

            _ddeiClientMock.Verify(c => c.GetCaseAsync(cmsArg), Times.Once);
            _egressClientMock.Verify(c => c.CreateWorkspaceAsync(createWorkspaceArg), Times.Once);
            _egressClientMock.Verify(c => c.ListWorkspaceRolesAsync(_workspaceId), Times.Once);
            _egressClientMock.Verify(c => c.GrantWorkspacePermission(It.IsAny<GrantWorkspacePermissionArg>()), Times.Never);
            _caseMetadataServiceMock.Verify(s => s.CreateEgressConnectionAsync(It.IsAny<CreateEgressConnectionDto>()), Times.Never);
            _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
                It.IsAny<ActionType>(),
                It.IsAny<ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null), Times.Never);
        }
    }
}