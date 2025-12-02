using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class ListNetAppFoldersTests
    {
        private readonly Mock<ILogger<ListNetAppFolders>> _loggerMock;
        private readonly Mock<INetAppClient> _netAppClientMock;
        private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
        private readonly Mock<ICaseEnrichmentService> _caseEnrichmentServiceMock;
        private readonly Mock<IOptions<NetAppOptions>> _optionsMock;
        private readonly Fixture _fixture;
        private readonly ListNetAppFolders _function;
        private readonly string _testBearerToken;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;

        public ListNetAppFoldersTests()
        {
            _loggerMock = new Mock<ILogger<ListNetAppFolders>>();
            _netAppClientMock = new Mock<INetAppClient>();
            _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
            _caseEnrichmentServiceMock = new Mock<ICaseEnrichmentService>();
            _optionsMock = new Mock<IOptions<NetAppOptions>>();
            _fixture = new Fixture();

            _testBearerToken = _fixture.Create<string>();
            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();

            _optionsMock.Setup(o => o.Value).Returns(new NetAppOptions
            {
                BucketName = "test-bucket",
                Url = "https://example.com",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                RegionName = "test-region"
            });

            _function = new ListNetAppFolders(
                _loggerMock.Object,
                _netAppClientMock.Object,
                _netAppArgFactoryMock.Object,
                _caseEnrichmentServiceMock.Object,
                _optionsMock.Object);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithEnrichedResponse_WhenResponseIsNotNull()
        {
            // Arrange
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = "opName",
                [InputParameters.ContinuationToken] = "token",
                [InputParameters.Take] = "50",
                [InputParameters.Path] = "/some/path"
            };

            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);

            var arg = _fixture.Create<ListFoldersInBucketArg>();
            var response = _fixture.Create<ListNetAppObjectsDto>();
            var enrichedResponse = _fixture.Create<ListNetAppObjectsResponse>();

            _netAppArgFactoryMock
                .Setup(f => f.CreateListFoldersInBucketArg(
                    _testBearerToken,
                    "test-bucket",
                    queryParams[InputParameters.OperationName],
                    queryParams[InputParameters.ContinuationToken],
                    50,
                    queryParams[InputParameters.Path]))
                .Returns(arg);

            _netAppClientMock
                .Setup(c => c.ListFoldersInBucketAsync(arg))
                .ReturnsAsync(response);

            _caseEnrichmentServiceMock
                .Setup(s => s.EnrichNetAppFoldersWithMetadataAsync(response))
                .ReturnsAsync(enrichedResponse);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedResponse, okResult.Value);

            _netAppArgFactoryMock.Verify(f => f.CreateListFoldersInBucketArg(
                _testBearerToken,
                "test-bucket",
                queryParams[InputParameters.OperationName],
                queryParams[InputParameters.ContinuationToken],
                50,
                queryParams[InputParameters.Path]), Times.Once);

            _netAppClientMock.Verify(c => c.ListFoldersInBucketAsync(arg), Times.Once);
            _caseEnrichmentServiceMock.Verify(s => s.EnrichNetAppFoldersWithMetadataAsync(response), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsBadRequest_WhenResponseIsNull()
        {
            // Arrange
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = "opName",
                [InputParameters.ContinuationToken] = "token",
                [InputParameters.Take] = "50",
                [InputParameters.Path] = "/some/path"
            };

            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);

            var arg = _fixture.Create<ListFoldersInBucketArg>();

            _netAppArgFactoryMock
                .Setup(f => f.CreateListFoldersInBucketArg(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .Returns(arg);

            _netAppClientMock
                .Setup(c => c.ListFoldersInBucketAsync(arg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _caseEnrichmentServiceMock.Verify(s => s.EnrichNetAppFoldersWithMetadataAsync(It.IsAny<ListNetAppObjectsDto>()), Times.Never);
        }
    }
}