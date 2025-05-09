using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.WireMock.Mappings;
using CPS.ComplexCases.WireMock.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.Tests.Integration;

public class EgressClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly EgressClient _client;
    private readonly EgressArgFactory _egressArgFactory;
    public EgressClientTests()
    {
        _server = WireMockServer
            .Start()
            .LoadMappings(
                new CaseDocumentMapping(),
                new CaseMaterialMapping(),
                new FindWorkspaceMapping(),
                new WorkspacePermissionsMapping(),
                new WorkspaceTokenMapping()
            );

        var egressOptions = new EgressOptions
        {
            Url = _server.Urls[0],
            Username = "username",
            Password = "password"
        };

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(egressOptions.Url)
        };
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EgressClient>();

        _client = new EgressClient(logger, new OptionsWrapper<EgressOptions>(egressOptions), httpClient, new EgressRequestFactory());
        _egressArgFactory = new EgressArgFactory();
    }

    public void Dispose()
    {
        _server.Stop();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FindWorkspace_ShouldReturnMatchingAuthorisedWorkspaces()
    {
        // Arrange
        var arg = _egressArgFactory.CreateListEgressWorkspacesArg("test-workspace", 0, 100);

        // Act
        var result = await _client.ListWorkspacesAsync(arg, "integration@cps.gov.uk");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);

        var workspace = result.Data.First();
        Assert.Equal("test-workspace", workspace.Name);
        Assert.Equal("workspace-id", workspace.Id);
    }

    [Fact]
    public async Task FindWorkspace_ShouldNotReturnUnauthorisedWorkspaces()
    {
        // Arrange
        var arg = _egressArgFactory.CreateListEgressWorkspacesArg("test-workspace", 0, 100);

        // Act
        var result = await _client.ListWorkspacesAsync(arg, "notauthorised@cps.gov.uk");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.Count);
        Assert.Equal(0, result.Pagination.Skip);
        Assert.Equal(100, result.Pagination.Take);
        Assert.Equal(1, result.Pagination.TotalResults);
    }

    [Fact]
    public async Task ListCaseMaterialAsync_ShouldReturnPaginatedCaseMaterial()
    {
        // Arrange
        var arg = _egressArgFactory.CreateListWorkspaceMaterialArg("workspace-id", 0, 10, null);

        // Act
        var result = await _client.ListCaseMaterialAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Pagination.Take);
        Assert.Equal(0, result.Pagination.Skip);
        Assert.Equal(2, result.Pagination.TotalResults);
        Assert.Equal(2, result.Data.Count());

        var material = result.Data.First();
        Assert.Equal("file-id", material.Id);
        Assert.Equal("file-name", material.FileName);
        Assert.Equal("file-path", material.Path);
        Assert.False(material.IsFolder);
        Assert.Equal(1, material.Version);
    }

    [Fact]
    public async Task ListCaseMaterialAsync_ShouldReturnTraversedFolderMaterial()
    {
        // Arrange
        var arg = _egressArgFactory.CreateListWorkspaceMaterialArg("workspace-id", 0, 10, "folder-id");

        // Act
        var result = await _client.ListCaseMaterialAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Pagination.Take);
        Assert.Equal(0, result.Pagination.Skip);
        Assert.Equal(1, result.Pagination.TotalResults);
        Assert.Single(result.Data);

        var material = result.Data.First();
        Assert.Equal("nested-file-id", material.Id);
        Assert.Equal("nested-file-name", material.FileName);
        Assert.Equal("folder-id/file-path", material.Path);
        Assert.False(material.IsFolder);
        Assert.Equal(1, material.Version);
    }
}