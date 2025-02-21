using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.Egress.Integration;

public class EgressClientTests
{
    private readonly EgressClient _client;
    private readonly EgressArgFactory _egressArgFactory;
    public EgressClientTests()
    {

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var egressOptions = configuration.GetSection("EgressOptions").Get<EgressOptions>();

        if (string.IsNullOrEmpty(egressOptions?.Url))
        {
            throw new ArgumentNullException(nameof(egressOptions.Url), "Egress URL cannot be null or empty.");
        }

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(egressOptions.Url)
        };
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EgressClient>();
        var egressRequestFactory = new EgressRequestFactory();

        _client = new EgressClient(logger, new OptionsWrapper<EgressOptions>(egressOptions), httpClient, egressRequestFactory);
        _egressArgFactory = new EgressArgFactory();
    }

    [Fact]
    public async Task FindWorkspace_ShouldReturnMatchingAuthorisedWorkspaces()
    {
        // Arrange
        var arg = _egressArgFactory.CreateFindWorkspaceArg("test-workspace");

        // Act
        var result = await _client.FindWorkspace(arg, "integration@cps.gov.uk");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var workspace = result.First();
        Assert.Equal("test-workspace", workspace.Name);
        Assert.Equal("workspace-id", workspace.Id);
    }

    [Fact]
    public async Task FindWorkspace_ShouldNotReturnUnauthorisedWorkspaces()
    {
        // Arrange
        var arg = _egressArgFactory.CreateFindWorkspaceArg("test-workspace");

        // Act
        var result = await _client.FindWorkspace(arg, "notauthorised@cps.gov.uk");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCaseMaterial_ShouldReturnPaginatedCaseMaterial()
    {
        // Arrange
        var arg = _egressArgFactory.CreateGetCaseMaterialArg("workspace-id", 1, 10, null);

        // Act
        var result = await _client.GetCaseMaterial(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.PerPage);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.TotalResults);
        Assert.Equal(2, result.Data.Count());

        var material = result.Data.First();
        Assert.Equal("file-id", material.Id);
        Assert.Equal("file-name", material.FileName);
        Assert.Equal("file-path", material.Path);
        Assert.False(material.IsFolder);
        Assert.Equal(1, material.Version);
    }

    [Fact]
    public async Task GetCaseMaterial_ShouldReturnTraversedFolderMaterial()
    {
        // Arrange
        var arg = _egressArgFactory.CreateGetCaseMaterialArg("workspace-id", 1, 10, "folder-id");

        // Act
        var result = await _client.GetCaseMaterial(arg);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.PerPage);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(1, result.TotalResults);
        Assert.Single(result.Data);

        var material = result.Data.First();
        Assert.Equal("nested-file-id", material.Id);
        Assert.Equal("nested-file-name", material.FileName);
        Assert.Equal("folder-id/file-path", material.Path);
        Assert.False(material.IsFolder);
        Assert.Equal(1, material.Version);
    }
}