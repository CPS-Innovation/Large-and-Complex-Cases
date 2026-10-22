using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class ListNetAppTemplatesTests
{
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly ListNetAppTemplates _function;
    private readonly Fixture _fixture;
    private readonly string _bearerToken;
    private readonly string _bucketName;
    private readonly Guid _correlationId;
    private readonly string _username;
    private readonly string _cmsAuthValues;

    public ListNetAppTemplatesTests()
    {
        _fixture = new Fixture();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        var userBucketAccessServiceMock = new Mock<IUserBucketAccessService>();

        _bearerToken = _fixture.Create<string>();
        _bucketName = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();
        _username = _fixture.Create<string>();
        _cmsAuthValues = _fixture.Create<string>();

        userBucketAccessServiceMock
            .Setup(s => s.ResolveBucketAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(new SecurityGroup
            {
                Id = _fixture.Create<Guid>(),
                BucketName = _bucketName,
                VolumeUuid = _fixture.Create<Guid>(),
                DisplayName = "Test Security Group"
            });

        _netAppArgFactoryMock
            .Setup(f => f.CreateListFoldersInBucketArg(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>()))
            .Returns((string bearer, string bucket, string? operationName, string? continuationToken, int? maxKeys, string? prefix) =>
                new ListFoldersInBucketArg
                {
                    BearerToken = bearer,
                    BucketName = bucket,
                    ContinuationToken = continuationToken,
                    Prefix = prefix
                });

        _function = new ListNetAppTemplates(
            new Mock<ILogger<ListNetAppTemplates>>().Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            userBucketAccessServiceMock.Object,
            new Mock<ICaseMetadataService>().Object,
            new Mock<IInitializationHandler>().Object);
    }

    [Fact]
    public async Task Run_WhenTemplatesFolderHasTwoTemplates_ReturnsBothWithCountOfTwo()
    {
        SetupListing("Area1/TeamA/_templates/",
            "Area1/TeamA/_templates/Standard/", "Area1/TeamA/_templates/Complex/");

        var response = await RunAndGetResponse("Area1/TeamA/");

        Assert.Equal(2, response.Count);
        Assert.Equal("Area1/TeamA/", response.ParentFolderPath);
        Assert.Contains(response.Templates, t => t.Name == "Standard" && t.Path == "Area1/TeamA/_templates/Standard/");
        Assert.Contains(response.Templates, t => t.Name == "Complex" && t.Path == "Area1/TeamA/_templates/Complex/");
    }

    [Fact]
    public async Task Run_WithNoPath_ListsTemplatesAtBucketRoot()
    {
        SetupListing("_templates/", "_templates/Standard/");

        var response = await RunAndGetResponse(null);

        Assert.Equal(1, response.Count);
        Assert.Equal(string.Empty, response.ParentFolderPath);
        _netAppArgFactoryMock.Verify(f => f.CreateListFoldersInBucketArg(
            _bearerToken, _bucketName, null, It.IsAny<string?>(), It.IsAny<int?>(), "_templates/"), Times.Once);
    }

    [Fact]
    public async Task Run_WithPathWithoutTrailingSlash_NormalisesPath()
    {
        SetupListing("Area1/TeamA/_templates/", "Area1/TeamA/_templates/Standard/");

        var response = await RunAndGetResponse("Area1/TeamA");

        Assert.Equal(1, response.Count);
        Assert.Equal("Area1/TeamA/", response.ParentFolderPath);
    }

    [Fact]
    public async Task Run_WhenTemplatesFolderIsMissingOrEmpty_ReturnsEmptyListWithCountOfZero()
    {
        SetupListing("Area1/TeamA/_templates/");

        var response = await RunAndGetResponse("Area1/TeamA/");

        Assert.Equal(0, response.Count);
        Assert.Empty(response.Templates);
    }

    [Fact]
    public async Task Run_WhenListingReturnsNull_ReturnsEmptyListWithCountOfZero()
    {
        _netAppClientMock
            .Setup(c => c.ListFoldersInBucketAsync(It.IsAny<ListFoldersInBucketArg>()))
            .ReturnsAsync((ListNetAppObjectsDto?)null);

        var response = await RunAndGetResponse("Area1/TeamA/");

        Assert.Equal(0, response.Count);
        Assert.Empty(response.Templates);
    }

    [Fact]
    public async Task Run_WhenSingleTemplate_ReturnedPathIsAcceptedByProvisioning()
    {
        SetupListing("Area1/TeamA/_templates/", "Area1/TeamA/_templates/Standard/");

        var response = await RunAndGetResponse("Area1/TeamA/");

        Assert.Equal(1, response.Count);
        var template = Assert.Single(response.Templates);

        var provisionDto = new ProvisionNetAppFoldersDto
        {
            ParentFolderPath = response.ParentFolderPath,
            TemplateFolderPath = template.Path
        };
        var validation = new ProvisionNetAppFoldersRequestValidator().Validate(provisionDto);
        Assert.True(validation.IsValid);
    }

    [Fact]
    public async Task Run_WhenListingIsPaginated_FollowsContinuationToken()
    {
        _netAppClientMock
            .SetupSequence(c => c.ListFoldersInBucketAsync(It.IsAny<ListFoldersInBucketArg>()))
            .ReturnsAsync(MakeListResult(["_templates/A/"], nextToken: "page-2"))
            .ReturnsAsync(MakeListResult(["_templates/B/"], nextToken: null));

        var response = await RunAndGetResponse(null);

        Assert.Equal(2, response.Count);
        _netAppArgFactoryMock.Verify(f => f.CreateListFoldersInBucketArg(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), "page-2", It.IsAny<int?>(), "_templates/"), Times.Once);
    }

    [Theory]
    [InlineData("_templates/")]
    [InlineData("Area1/_templates/Standard/")]
    [InlineData("Area1/../")]
    public async Task Run_WhenPathIsInvalid_ReturnsBadRequest(string path)
    {
        var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(
            new Dictionary<string, string> { [InputParameters.Path] = path });
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        var result = await _function.Run(request, functionContext);

        Assert.IsType<BadRequestObjectResult>(result);
        _netAppClientMock.Verify(c => c.ListFoldersInBucketAsync(It.IsAny<ListFoldersInBucketArg>()), Times.Never);
    }

    private async Task<ListNetAppTemplatesResponse> RunAndGetResponse(string? path)
    {
        var query = new Dictionary<string, string>();
        if (path != null)
        {
            query[InputParameters.Path] = path;
        }

        var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(query);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        var result = await _function.Run(request, functionContext);

        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<ListNetAppTemplatesResponse>(ok.Value);
    }

    private void SetupListing(string templatesFolderPath, params string[] templatePaths)
    {
        _netAppClientMock
            .Setup(c => c.ListFoldersInBucketAsync(It.Is<ListFoldersInBucketArg>(a => a.Prefix == templatesFolderPath)))
            .ReturnsAsync(MakeListResult(templatePaths, nextToken: null));
    }

    private ListNetAppObjectsDto MakeListResult(string[] folderPaths, string? nextToken) => new()
    {
        Data = new ListNetAppDataDto
        {
            BucketName = _bucketName,
            FolderData = folderPaths.Select(p => new ListNetAppFolderDataDto { Path = p }),
            FileData = []
        },
        Pagination = new PaginationDto { NextContinuationToken = nextToken }
    };
}
