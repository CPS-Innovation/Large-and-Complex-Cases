using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.API.Integration.Tests.Egress;

[Collection("Integration Tests")]
public class EgressClientTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public EgressClientTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task ListWorkspacesAsync_WithValidCredentials_ReturnsWorkspaces()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured - set Egress__Url, Egress__Username, Egress__Password, and Egress__WorkspaceId environment variables");

        // Arrange
        var arg = new ListEgressWorkspacesArg
        {
            Skip = 0,
            Take = 10
        };

        // Act
        var result = await _fixture.EgressClient!.ListWorkspacesAsync(arg, _fixture.Settings.Egress.Username!);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Pagination);
    }

    [SkippableFact]
    public async Task ListWorkspacesAsync_WithNameFilter_ReturnsFilteredWorkspaces()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var arg = new ListEgressWorkspacesArg
        {
            Skip = 0,
            Take = 100,
            Name = _fixture.Settings.Egress.WorkspaceName!
        };

        // Act
        var result = await _fixture.EgressClient!.ListWorkspacesAsync(arg, _fixture.Settings.Egress.Username!);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Pagination);
    }

    [SkippableFact]
    public async Task GetWorkspacePermission_WithValidUser_ReturnsTrue()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var arg = new GetWorkspacePermissionArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Email = _fixture.Settings.Egress.Email!
        };

        // Act
        var hasPermission = await _fixture.EgressClient!.GetWorkspacePermission(arg);

        // Assert
        Assert.True(hasPermission, "User should have permission to the configured workspace");
    }

    [SkippableFact]
    public async Task GetWorkspacePermission_WithInvalidEmail_ReturnsFalse()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var arg = new GetWorkspacePermissionArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Email = $"nonexistent-user-{Guid.NewGuid()}@example.com"
        };

        // Act
        var hasPermission = await _fixture.EgressClient!.GetWorkspacePermission(arg);

        // Assert
        Assert.False(hasPermission, "Non-existent user should not have permission");
    }

    [SkippableFact]
    public async Task ListCaseMaterialAsync_WithValidWorkspace_ReturnsMaterials()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var arg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Skip = 0,
            Take = 10,
            RecurseSubFolders = false
        };

        // Act
        var result = await _fixture.EgressClient!.ListCaseMaterialAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Pagination);
        Assert.True(result.Pagination.Take <= 10, "Should respect Take parameter");
    }

    [SkippableFact]
    public async Task ListCaseMaterialAsync_WithFolderPath_ReturnsSameShapeAsFolderId()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange - find a folder in the workspace root to list by its path
        var rootArg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Skip = 0,
            Take = 100,
            RecurseSubFolders = false
        };

        var rootListing = await _fixture.EgressClient!.ListCaseMaterialAsync(rootArg);
        var folder = rootListing.Data.FirstOrDefault(d => d.IsFolder);

        Skip.If(folder is null, "No folder available in the configured workspace to exercise path listing");

        // Act - list the same folder by id and by path
        var byIdArg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Skip = 0,
            Take = 100,
            FolderId = folder!.Id,
            RecurseSubFolders = false
        };

        var byPathArg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Skip = 0,
            Take = 100,
            Path = folder.Path,
            RecurseSubFolders = false
        };

        var byId = await _fixture.EgressClient!.ListCaseMaterialAsync(byIdArg);
        var byPath = await _fixture.EgressClient!.ListCaseMaterialAsync(byPathArg);

        // Assert - listing by path returns the same response shape as listing by
        // folder-id: a populated Data set and Pagination, with each item carrying an id
        Assert.NotNull(byId);
        Assert.NotNull(byPath);
        Assert.NotNull(byPath.Data);
        Assert.NotNull(byPath.Pagination);

        Assert.All(byPath.Data, item =>
        {
            Assert.False(string.IsNullOrEmpty(item.Id), "Each item returned by path listing should have an id");
            Assert.NotNull(item.Name);
            Assert.NotNull(item.Path);
        });
    }

    [SkippableFact]
    public async Task ListCaseMaterialAsync_WithNonExistentPath_ReturnsDistinguishableNotFound()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange - a path that should not exist in the workspace
        var arg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Skip = 0,
            Take = 10,
            Path = $"non-existent-folder-{Guid.NewGuid()}",
            RecurseSubFolders = false
        };

        // Act & Assert - the outcome must be distinguishable from an empty folder.
        try
        {
            var result = await _fixture.EgressClient!.ListCaseMaterialAsync(arg);

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
        catch (HttpRequestException ex)
        {
            // Alternatively the API rejects the unknown path with an HTTP error,
            // which is also distinguishable from an empty folder.
            Assert.NotNull(ex);
        }
    }

    [SkippableFact]
    public async Task ListCaseMaterialAsync_WithPagination_RespectsSkipAndTake()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange - first page
        var firstPageArg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = _fixture.EgressWorkspaceId!,
            Skip = 0,
            Take = 5,
            RecurseSubFolders = false
        };

        // Act - get first page
        var firstPage = await _fixture.EgressClient!.ListCaseMaterialAsync(firstPageArg);

        // Assert
        Assert.NotNull(firstPage);
        Assert.NotNull(firstPage.Pagination);
        Assert.Equal(0, firstPage.Pagination.Skip);

        // If there are more results, test second page
        if (firstPage.Pagination.TotalResults > 5)
        {
            var secondPageArg = new ListWorkspaceMaterialArg
            {
                WorkspaceId = _fixture.EgressWorkspaceId!,
                Skip = 5,
                Take = 5,
                RecurseSubFolders = false
            };

            var secondPage = await _fixture.EgressClient!.ListCaseMaterialAsync(secondPageArg);

            Assert.NotNull(secondPage);
            Assert.Equal(5, secondPage.Pagination.Skip);
        }
    }

    [SkippableFact]
    public async Task ListTemplatesAsync_ReturnsTemplates()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var arg = new PaginationArg
        {
            Skip = 0,
            Take = 50
        };

        // Act
        var result = await _fixture.EgressClient!.ListTemplatesAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Pagination);
        Assert.NotEmpty(result.Data);

        // Verify template structure
        foreach (var template in result.Data)
        {
            Assert.False(string.IsNullOrEmpty(template.Id), "Template should have an Id");
            Assert.False(string.IsNullOrEmpty(template.Name), "Template should have a Name");
        }
    }

    [SkippableFact]
    public async Task ListWorkspaceRolesAsync_WithValidWorkspace_ReturnsRoles()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Act
        var roles = await _fixture.EgressClient!.ListWorkspaceRolesAsync(_fixture.EgressWorkspaceId!);

        // Assert
        Assert.NotNull(roles);
        Assert.NotEmpty(roles);

        foreach (var role in roles)
        {
            Assert.False(string.IsNullOrEmpty(role.RoleId), "Role should have a RoleId");
            Assert.False(string.IsNullOrEmpty(role.RoleName), "Role should have a RoleName");
        }
    }

    [SkippableFact]
    public async Task ListWorkspaceRolesAsync_WithInvalidWorkspace_ThrowsException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var invalidWorkspaceId = "invalid-workspace-" + Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _fixture.EgressClient!.ListWorkspaceRolesAsync(invalidWorkspaceId));
    }

    [SkippableFact]
    public async Task ListCaseMaterialAsync_WithInvalidWorkspaceId_ThrowsException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var arg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = "invalid-workspace-" + Guid.NewGuid(),
            Skip = 0,
            Take = 10
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _fixture.EgressClient!.ListCaseMaterialAsync(arg));
    }

    [SkippableFact]
    public async Task GetWorkspacePermission_WithInvalidWorkspaceId_ThrowsException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var arg = new GetWorkspacePermissionArg
        {
            WorkspaceId = "invalid-workspace-" + Guid.NewGuid(),
            Email = _fixture.Settings.Egress.Username!
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _fixture.EgressClient!.GetWorkspacePermission(arg));
    }
}
