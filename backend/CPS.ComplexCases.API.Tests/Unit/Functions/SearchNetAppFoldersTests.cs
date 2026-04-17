using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Enums;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Models.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class SearchNetAppFoldersTests
{
    private readonly Mock<ILogger<SearchNetAppFolders>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly IValidator<SearchNetAppFoldersDto> _validator;
    private readonly SearchNetAppFolders _function;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;

    public SearchNetAppFoldersTests()
    {
        _loggerMock = new Mock<ILogger<SearchNetAppFolders>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _validator = new SearchNetAppFoldersRequestValidator();
        _fixture = new Fixture();

        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
            .ReturnsAsync([
                new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = _testBucketName,
                    DisplayName = "Test Security Group"
                }
            ]);

        _function = new SearchNetAppFolders(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _caseMetadataServiceMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _initializationHandlerMock.Object,
            _validator);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenCaseIdIsMissing()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.Query] = "searchTerm"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("CaseId must be provided", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenQueryIsMissing()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Query must be provided", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenMaxResultsIsZero()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm",
            [InputParameters.MaxResults] = "0"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("MaxResults must be between 1 and 1000", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenMaxResultsExceedsLimit()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm",
            [InputParameters.MaxResults] = "1001"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("MaxResults must be between 1 and 1000", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenCaseMetadataIsNull()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync((CaseMetadata?)null);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _netAppClientMock.Verify(c => c.SearchObjectsInBucketAsync(It.IsAny<SearchArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenNetappFolderPathIsNull()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = null });

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _netAppClientMock.Verify(c => c.SearchObjectsInBucketAsync(It.IsAny<SearchArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenNetappFolderPathIsEmpty()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = string.Empty });

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _netAppClientMock.Verify(c => c.SearchObjectsInBucketAsync(It.IsAny<SearchArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_ReturnsOkObjectResult_WithSearchResults_WhenRequestIsValid()
    {
        // Arrange
        var folderPath = "/case/123";
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm",
            [InputParameters.MaxResults] = "50"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        var searchArg = _fixture.Create<SearchArg>();
        var searchResponse = _fixture.Create<SearchResultsDto>();

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = folderPath });

        _netAppArgFactoryMock
            .Setup(f => f.CreateSearchArg(_testBearerToken, _testBucketName, folderPath, "searchTerm", 50, SearchModes.Prefix))
            .Returns(searchArg);

        _netAppClientMock
            .Setup(c => c.SearchObjectsInBucketAsync(searchArg))
            .ReturnsAsync(searchResponse);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(searchResponse, okResult.Value);

        _netAppArgFactoryMock.Verify(
            f => f.CreateSearchArg(_testBearerToken, _testBucketName, folderPath, "searchTerm", 50, SearchModes.Prefix),
            Times.Once);
        _netAppClientMock.Verify(c => c.SearchObjectsInBucketAsync(searchArg), Times.Once);
    }

    [Fact]
    public async Task Run_UsesDefaultPrefixMode_WhenModeIsNotSupplied()
    {
        // Arrange
        var folderPath = "/case/123";
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        var searchArg = _fixture.Create<SearchArg>();

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = folderPath });

        _netAppArgFactoryMock
            .Setup(f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<SearchModes>()))
            .Returns(searchArg);

        _netAppClientMock
            .Setup(c => c.SearchObjectsInBucketAsync(searchArg))
            .ReturnsAsync(_fixture.Create<SearchResultsDto>());

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(
            f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), SearchModes.Prefix),
            Times.Once);
    }

    [Fact]
    public async Task Run_PassesSubstringMode_WhenModeIsSpecified()
    {
        // Arrange
        var folderPath = "/case/123";
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm",
            [InputParameters.Mode] = "Substring"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        var searchArg = _fixture.Create<SearchArg>();

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = folderPath });

        _netAppArgFactoryMock
            .Setup(f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<SearchModes>()))
            .Returns(searchArg);

        _netAppClientMock
            .Setup(c => c.SearchObjectsInBucketAsync(searchArg))
            .ReturnsAsync(_fixture.Create<SearchResultsDto>());

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(
            f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), SearchModes.Substring),
            Times.Once);
    }

    [Fact]
    public async Task Run_UsesDefaultMaxResults_WhenMaxResultsIsNotSupplied()
    {
        // Arrange
        var folderPath = "/case/123";
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        var searchArg = _fixture.Create<SearchArg>();

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = folderPath });

        _netAppArgFactoryMock
            .Setup(f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<SearchModes>()))
            .Returns(searchArg);

        _netAppClientMock
            .Setup(c => c.SearchObjectsInBucketAsync(searchArg))
            .ReturnsAsync(_fixture.Create<SearchResultsDto>());

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(
            f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), 1000, It.IsAny<SearchModes>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_PassesBucketNameFromSecurityGroup_ToArgFactory()
    {
        // Arrange
        var folderPath = "/case/123";
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        var searchArg = _fixture.Create<SearchArg>();

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = folderPath });

        _netAppArgFactoryMock
            .Setup(f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<SearchModes>()))
            .Returns(searchArg);

        _netAppClientMock
            .Setup(c => c.SearchObjectsInBucketAsync(searchArg))
            .ReturnsAsync(_fixture.Create<SearchResultsDto>());

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(
            f => f.CreateSearchArg(_testBearerToken, _testBucketName, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<SearchModes>()),
            Times.Once);
    }

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("some/../traversal")]
    [InlineData("..")]
    public async Task Run_ReturnsBadRequest_WhenQueryContainsDoubleDotPathTraversal(string query)
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = query
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Path cannot contain '..'", badRequest.Value?.ToString());
        _netAppClientMock.Verify(c => c.SearchObjectsInBucketAsync(It.IsAny<SearchArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_FallsBackToPrefixMode_WhenModeIsUnrecognised()
    {
        // Arrange — an unrecognised mode string cannot be parsed as SearchModes so the
        // function defaults to Prefix rather than returning a 400.
        var folderPath = "/case/123";
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.CaseId] = "123",
            [InputParameters.Query] = "searchTerm",
            [InputParameters.Mode] = "invalidMode"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        var searchArg = _fixture.Create<SearchArg>();

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(123))
            .ReturnsAsync(new CaseMetadata { CaseId = 123, NetappFolderPath = folderPath });

        _netAppArgFactoryMock
            .Setup(f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<SearchModes>()))
            .Returns(searchArg);

        _netAppClientMock
            .Setup(c => c.SearchObjectsInBucketAsync(searchArg))
            .ReturnsAsync(_fixture.Create<SearchResultsDto>());

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _netAppArgFactoryMock.Verify(
            f => f.CreateSearchArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), SearchModes.Prefix),
            Times.Once);
    }
}
