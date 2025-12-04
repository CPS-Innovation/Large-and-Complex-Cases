using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class ListEgressMaterialsTests
    {
        private readonly Mock<ILogger<ListEgressMaterials>> _loggerMock;
        private readonly Mock<IEgressClient> _egressClientMock;
        private readonly Mock<IEgressArgFactory> _egressArgFactoryMock;
        private readonly Fixture _fixture;
        private readonly ListEgressMaterials _function;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;

        public ListEgressMaterialsTests()
        {
            _loggerMock = new Mock<ILogger<ListEgressMaterials>>();
            _egressClientMock = new Mock<IEgressClient>();
            _egressArgFactoryMock = new Mock<IEgressArgFactory>();
            _fixture = new Fixture();
            _function = new ListEgressMaterials(_loggerMock.Object, _egressClientMock.Object, _egressArgFactoryMock.Object);
            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithExpectedResponse_WhenUserHasPermission()
        {
            // Arrange
            var workspaceId = _fixture.Create<string>();
            var username = _fixture.Create<string>();

            var folderId = _fixture.Create<string>();
            var skip = _fixture.Create<int>();
            var take = _fixture.Create<int>();

            var listMaterialsResponse = _fixture.Create<ListCaseMaterialDto>();

            var listMaterialsArg = _fixture.Create<ListWorkspaceMaterialArg>();
            var permissionsArg = _fixture.Create<GetWorkspacePermissionArg>();

            _egressArgFactoryMock
                .Setup(f => f.CreateListWorkspaceMaterialArg(workspaceId, skip, take, folderId, null))
                .Returns(listMaterialsArg);

            _egressArgFactoryMock
                .Setup(f => f.CreateGetWorkspacePermissionArg(workspaceId, username))
                .Returns(permissionsArg);

            _egressClientMock
                .Setup(c => c.GetWorkspacePermission(permissionsArg))
                .ReturnsAsync(true);

            _egressClientMock
                .Setup(c => c.ListCaseMaterialAsync(listMaterialsArg))
                .ReturnsAsync(listMaterialsResponse);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, username, _testBearerToken);
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.FolderId] = folderId,
                [InputParameters.Skip] = skip.ToString(),
                [InputParameters.Take] = take.ToString()
            };
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams, _testCorrelationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext, workspaceId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(listMaterialsResponse, okResult.Value);

            _egressArgFactoryMock.Verify(f => f.CreateListWorkspaceMaterialArg(workspaceId, skip, take, folderId, null), Times.Once);
            _egressArgFactoryMock.Verify(f => f.CreateGetWorkspacePermissionArg(workspaceId, username), Times.Once);
            _egressClientMock.Verify(c => c.GetWorkspacePermission(permissionsArg), Times.Once);
            _egressClientMock.Verify(c => c.ListCaseMaterialAsync(listMaterialsArg), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorized_WhenUserHasNoPermission()
        {
            // Arrange
            var workspaceId = _fixture.Create<string>();
            var correlationId = _fixture.Create<Guid>();
            var username = _fixture.Create<string>();

            var permissionsArg = _fixture.Create<GetWorkspacePermissionArg>();

            _egressArgFactoryMock
                .Setup(f => f.CreateGetWorkspacePermissionArg(workspaceId, username))
                .Returns(permissionsArg);

            _egressClientMock
                .Setup(c => c.GetWorkspacePermission(permissionsArg))
                .ReturnsAsync(false);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, username, _testBearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), _testCorrelationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext, workspaceId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);

            _egressArgFactoryMock.Verify(f => f.CreateGetWorkspacePermissionArg(workspaceId, username), Times.Once);
            _egressClientMock.Verify(c => c.GetWorkspacePermission(permissionsArg), Times.Once);
            _egressClientMock.Verify(c => c.ListCaseMaterialAsync(It.IsAny<ListWorkspaceMaterialArg>()), Times.Never);
        }

        [Fact]
        public async Task Run_ReturnsNotFound_WhenGetWorkspacePermissionThrowsNotFound()
        {
            // Arrange
            var workspaceId = _fixture.Create<string>();
            var correlationId = _fixture.Create<Guid>();
            var username = _fixture.Create<string>();

            var permissionsArg = _fixture.Create<GetWorkspacePermissionArg>();

            _egressArgFactoryMock
                .Setup(f => f.CreateGetWorkspacePermissionArg(workspaceId, username))
                .Returns(permissionsArg);

            _egressClientMock
                .Setup(c => c.GetWorkspacePermission(permissionsArg))
                .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _testCmsAuthValues, username, _testBearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext, workspaceId);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            _egressArgFactoryMock.Verify(f => f.CreateGetWorkspacePermissionArg(workspaceId, username), Times.Once);
            _egressClientMock.Verify(c => c.GetWorkspacePermission(permissionsArg), Times.Once);
            _egressClientMock.Verify(c => c.ListCaseMaterialAsync(It.IsAny<ListWorkspaceMaterialArg>()), Times.Never);
        }
    }
}
