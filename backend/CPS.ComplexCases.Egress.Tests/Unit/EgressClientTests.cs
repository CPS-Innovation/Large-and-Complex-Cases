using System.Net;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.Egress.Tests.Unit;

public class EgressClientTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<EgressClient>> _loggerMock;
    private readonly Mock<IOptions<EgressOptions>> _optionsMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IEgressRequestFactory> _requestFactoryMock;
    private readonly EgressClient _client;
    private const string TestUrl = "https://example.com";

    public EgressClientTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<EgressClient>>>();
        _optionsMock = _fixture.Freeze<Mock<IOptions<EgressOptions>>>();
        _requestFactoryMock = _fixture.Freeze<Mock<IEgressRequestFactory>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();


        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(TestUrl)
        };
        var egressOptions = new EgressOptions
        {
            Url = TestUrl,
            Username = _fixture.Create<string>(),
            Password = _fixture.Create<string>()
        };
        _optionsMock.Setup(o => o.Value).Returns(egressOptions);

        _client = new EgressClient(
            _loggerMock.Object,
            _optionsMock.Object,
            _httpClient,
            _requestFactoryMock.Object
        );
    }


    [Fact]
    public async Task FindWorkspace_WhenWorkspaceExists_AndUserHasPermission_ReturnsWorkspace()
    {
        // Arrange
        var workspaceName = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var token = _fixture.Create<string>();
        var email = _fixture.Create<string>();
        var findWorkspaceArg = new ListEgressWorkspacesArg { Name = workspaceName };

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var workspaceResponse = new ListWorkspacesResponse
        {
            Data = [
                new ListWorkspacesResponseData
                {
                    Id = workspaceId,
                    Name = workspaceName,
                }
            ],
            DataInfo = _fixture.Create<DataInfoResponse>()
        };
        var permissionsResponse = new GetWorkspacePermissionsResponse
        {
            Data = [
                new GetWorkspacePersmissionsResponseData
                {
                    Email = email,
                    RoleId = _fixture.Create<string>()
                }
            ],
            DataInfo = _fixture.Create<DataInfoResponse>()
        };

        SetupRequestFactory(workspaceId, token, findWorkspaceArg);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("workspace", workspaceResponse),
            ("permissions", permissionsResponse)
        );

        // Act
        var result = await _client.ListWorkspacesAsync(findWorkspaceArg, email);

        // Assert
        var workspace = Assert.Single(result.Data);
        Assert.NotNull(workspace);
        Assert.Equal(workspaceId, workspace.Id);
        Assert.Equal(workspaceName, workspace.Name);

        VerifyRequestFactoryCalls(findWorkspaceArg, workspaceId, token);
    }

    [Fact]
    public async Task FindWorkspace_WhenWorkspaceExists_AndUserLacksPermission_ReturnsEmptyList()
    {
        // Arrange
        var workspaceName = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var token = _fixture.Create<string>();
        var email = _fixture.Create<string>();
        var arg = new ListEgressWorkspacesArg { Name = workspaceName };

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var workspaceResponse = new ListWorkspacesResponse
        {
            Data = [
                new ListWorkspacesResponseData
                {
                    Id = workspaceId,
                    Name = workspaceName,
                }
            ],
            DataInfo = _fixture.Create<DataInfoResponse>()
        };
        var permissionsResponse = new GetWorkspacePermissionsResponse
        {
            Data = [
                new GetWorkspacePersmissionsResponseData
                {
                    Email = _fixture.Create<string>(),
                    RoleId = _fixture.Create<string>()
                }
            ],
            DataInfo = _fixture.Create<DataInfoResponse>()
        };

        SetupRequestFactory(workspaceId, token, arg);
        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("workspace", workspaceResponse),
            ("permissions", permissionsResponse)
        );

        // Act
        var result = await _client.ListWorkspacesAsync(arg, email);

        // Assert
        Assert.Empty(result.Data);
        VerifyRequestFactoryCalls(arg, workspaceId, token);
    }

    [Fact]
    public async Task CreateWorkspaceAsync_ReturnsWorkspaceResponse()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var templateId = _fixture.Create<string>();
        var workspaceId = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var arg = new CreateEgressWorkspaceArg
        {
            Name = name,
            Description = description,
            TemplateId = templateId
        };

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var workspaceResponse = new CreateWorkspaceResponse
        {
            Id = workspaceId,
            Name = name,
            Description = description
        };

        _requestFactoryMock
            .Setup(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/auth"));

        _requestFactoryMock
            .Setup(f => f.CreateWorkspaceRequest(arg, token))
            .Returns(new HttpRequestMessage(HttpMethod.Post, $"{TestUrl}/api/v1/workspaces"));

        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("workspace", workspaceResponse)
        );

        // Act
        var result = await _client.CreateWorkspaceAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workspaceId, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);

        _requestFactoryMock.Verify(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _requestFactoryMock.Verify(f => f.CreateWorkspaceRequest(arg, token), Times.Once);
    }

    [Fact]
    public async Task GrantWorkspacePermission_CompletesSuccessfully()
    {
        // Arrange
        var workspaceId = _fixture.Create<string>();
        var email = _fixture.Create<string>();
        var roleId = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var arg = new GrantWorkspacePermissionArg
        {
            WorkspaceId = workspaceId,
            Username = email,
            RoleId = roleId
        };

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };

        _requestFactoryMock
            .Setup(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/auth"));

        _requestFactoryMock
            .Setup(f => f.GrantWorkspacePermissionRequest(arg, token))
            .Returns(new HttpRequestMessage(HttpMethod.Post, $"{TestUrl}/api/v1/workspaces/{workspaceId}/permissions"));

        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("permission", new { })
        );

        // Act
        await _client.GrantWorkspacePermission(arg);

        // Assert
        _requestFactoryMock.Verify(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _requestFactoryMock.Verify(f => f.GrantWorkspacePermissionRequest(arg, token), Times.Once);
    }

    [Fact]
    public async Task ListWorkspaceRolesAsync_ReturnsRoles()
    {
        // Arrange
        var workspaceId = _fixture.Create<string>();
        var roleId1 = _fixture.Create<string>();
        var roleId2 = _fixture.Create<string>();
        var roleName1 = "Administrator";
        var roleName2 = "Viewer";
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var rolesResponse = new ListWorkspaceRolesResponse
        {
            Data = new[]
            {
                new WorkspaceRole
                {
                    RoleId = roleId1,
                    RoleName = roleName1
                },
                new WorkspaceRole
                {
                    RoleId = roleId2,
                    RoleName = roleName2
                }
            },
            DataInfo = _fixture.Create<DataInfoResponse>()
        };

        _requestFactoryMock
            .Setup(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/auth"));

        _requestFactoryMock
            .Setup(f => f.ListWorkspaceRolesRequest(
                It.Is<ListWorkspaceRolesArg>(a => a.WorkspaceId == workspaceId && a.Take == 100 && a.Skip == 0),
                token))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/roles"));

        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("roles", rolesResponse)
        );

        // Act
        var result = await _client.ListWorkspaceRolesAsync(workspaceId);

        // Assert
        var roles = result.ToArray();
        Assert.Equal(2, roles.Length);
        Assert.Contains(roles, r => r.RoleId == roleId1 && r.RoleName == roleName1);
        Assert.Contains(roles, r => r.RoleId == roleId2 && r.RoleName == roleName2);

        _requestFactoryMock.Verify(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _requestFactoryMock.Verify(f => f.ListWorkspaceRolesRequest(
            It.Is<ListWorkspaceRolesArg>(a => a.WorkspaceId == workspaceId && a.Take == 100 && a.Skip == 0),
            token), Times.Once);
    }

    [Fact]
    public async Task ListWorkspaceRolesAsync_ReturnsEmptyList_WhenNoRoles()
    {
        // Arrange
        var workspaceId = _fixture.Create<string>();
        var token = _fixture.Create<string>();

        var tokenResponse = new GetWorkspaceTokenResponse { Token = token };
        var rolesResponse = new ListWorkspaceRolesResponse
        {
            Data = Array.Empty<WorkspaceRole>(),
            DataInfo = _fixture.Create<DataInfoResponse>()
        };

        _requestFactoryMock
            .Setup(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/auth"));

        _requestFactoryMock
            .Setup(f => f.ListWorkspaceRolesRequest(
                It.Is<ListWorkspaceRolesArg>(a => a.WorkspaceId == workspaceId),
                token))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/roles"));

        SetupHttpMockResponses(
            ("token", tokenResponse),
            ("roles", rolesResponse)
        );

        // Act
        var result = await _client.ListWorkspaceRolesAsync(workspaceId);

        // Assert
        Assert.Empty(result);

        _requestFactoryMock.Verify(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _requestFactoryMock.Verify(f => f.ListWorkspaceRolesRequest(
            It.Is<ListWorkspaceRolesArg>(a => a.WorkspaceId == workspaceId),
            token), Times.Once);
    }

    private void SetupRequestFactory(string workspaceId, string token, ListEgressWorkspacesArg arg)
    {
        _requestFactoryMock
            .Setup(f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/auth"));

        _requestFactoryMock
            .Setup(f => f.ListWorkspacesRequest(arg, token))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces"));

        _requestFactoryMock
            .Setup(f => f.GetWorkspacePermissionsRequest(
                It.Is<GetWorkspacePermissionArg>(arg => arg.WorkspaceId == workspaceId),
                token))
            .Returns(new HttpRequestMessage(HttpMethod.Get, $"{TestUrl}/api/v1/workspaces/{workspaceId}/users"));
    }

    private void VerifyRequestFactoryCalls(ListEgressWorkspacesArg arg, string workspaceId, string token)
    {
        _requestFactoryMock.Verify(
            f => f.GetWorkspaceTokenRequest(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
        _requestFactoryMock.Verify(
            f => f.ListWorkspacesRequest(arg, token),
            Times.Once);
        _requestFactoryMock.Verify(
            f => f.GetWorkspacePermissionsRequest(
                It.Is<GetWorkspacePermissionArg>(arg => arg.WorkspaceId == workspaceId),
                token),
            Times.Once);
    }

    private void SetupHttpMockResponses(params (string type, object response)[] responses)
    {
        var sequence = _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        foreach (var (_, response) in responses)
        {
            var content = JsonSerializer.Serialize(response);
            sequence = sequence.ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            });
        }
    }
}