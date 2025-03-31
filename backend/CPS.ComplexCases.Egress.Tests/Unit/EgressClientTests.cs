using System.Net;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;
using FluentAssertions;
using FluentAssertions.Execution;
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
        using (new AssertionScope())
        {
            workspace.Should().NotBeNull();
            workspace.Id.Should().Be(workspaceId);
            workspace.Name.Should().Be(workspaceName);
        }

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
        result.Data.Should().BeEmpty();
        VerifyRequestFactoryCalls(arg, workspaceId, token);
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
                It.Is<GetWorkSpacePermissionArg>(arg => arg.WorkspaceId == workspaceId),
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
                It.Is<GetWorkSpacePermissionArg>(arg => arg.WorkspaceId == workspaceId),
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