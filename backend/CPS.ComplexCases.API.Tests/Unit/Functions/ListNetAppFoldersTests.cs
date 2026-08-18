using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class ListNetAppFoldersTests
    {
        private readonly Mock<ILogger<ListNetAppFolders>> _loggerMock;
        private readonly Mock<INetAppClient> _netAppClientMock;
        private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
        private readonly Mock<ICaseEnrichmentService> _caseEnrichmentServiceMock;
        private readonly Mock<IUserBucketAccessService> _userBucketAccessServiceMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly Fixture _fixture;
        private readonly ListNetAppFolders _function;
        private readonly string _testBearerToken;
        private readonly string _testBucketName;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;

        public ListNetAppFoldersTests()
        {
            _loggerMock = new Mock<ILogger<ListNetAppFolders>>();
            _netAppClientMock = new Mock<INetAppClient>();
            _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
            _caseEnrichmentServiceMock = new Mock<ICaseEnrichmentService>();
            _userBucketAccessServiceMock = new Mock<IUserBucketAccessService>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();
            _fixture = new Fixture();

            _testBearerToken = _fixture.Create<string>();
            _testBucketName = _fixture.Create<string>();
            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();

            _userBucketAccessServiceMock
                .Setup(s => s.ResolveBucketAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = _testBucketName,
                    VolumeUuid = _fixture.Create<Guid>(),
                    DisplayName = "Test Security Group"
                });

            _function = new ListNetAppFolders(
                _loggerMock.Object,
                _netAppClientMock.Object,
                _netAppArgFactoryMock.Object,
                _caseEnrichmentServiceMock.Object,
                _userBucketAccessServiceMock.Object,
                _caseMetadataServiceMock.Object,
                _initializationHandlerMock.Object);
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
                    _testBucketName,
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
                _testBucketName,
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

        [Fact]
        public async Task Run_WhenCaseIdSupplied_ResolvesUsingPersistedBucket()
        {
            // Arrange
            var caseId = 12345;
            var persistedBucket = "manchester-bucket";

            _caseMetadataServiceMock
                .Setup(s => s.GetCaseMetadataForCaseIdAsync(caseId))
                .ReturnsAsync(new CaseMetadata { CaseId = caseId, NetappBucketName = persistedBucket });

            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = "opName",
                [InputParameters.Take] = "50",
                [InputParameters.Path] = "/some/path",
                [InputParameters.CaseId] = caseId.ToString()
            };

            ArrangeNetAppCall(queryParams);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams), functionContext);

            // Assert
            _userBucketAccessServiceMock.Verify(
                s => s.ResolveBucketAsync(_testBearerToken, persistedBucket, null), Times.Once);
        }

        [Fact]
        public async Task Run_WhenBucketNameSupplied_PassesItThroughForValidation()
        {
            // Arrange
            var requestedBucket = "manchester-bucket";

            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = "opName",
                [InputParameters.Take] = "50",
                [InputParameters.Path] = "/some/path",
                [InputParameters.BucketName] = requestedBucket
            };

            ArrangeNetAppCall(queryParams);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams), functionContext);

            // Assert
            _userBucketAccessServiceMock.Verify(
                s => s.ResolveBucketAsync(_testBearerToken, null, requestedBucket), Times.Once);
        }

        [Fact]
        public async Task Run_WhenUserNotEntitledToRequestedBucket_ThrowsMissingSecurityGroupException()
        {
            // Arrange
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = "opName",
                [InputParameters.Take] = "50",
                [InputParameters.Path] = "/some/path",
                [InputParameters.BucketName] = "not-entitled-bucket"
            };

            _userBucketAccessServiceMock
                .Setup(s => s.ResolveBucketAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ThrowsAsync(new MissingSecurityGroupException("User is not entitled to bucket 'not-entitled-bucket'."));

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act & Assert — the exception middleware maps this to a 403
            await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
                _function.Run(HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams), functionContext));

            _netAppClientMock.Verify(c => c.ListFoldersInBucketAsync(It.IsAny<ListFoldersInBucketArg>()), Times.Never);
        }

        private void ArrangeNetAppCall(Dictionary<string, string> queryParams)
        {
            var arg = _fixture.Create<ListFoldersInBucketArg>();
            var response = _fixture.Create<ListNetAppObjectsDto>();

            _netAppArgFactoryMock
                .Setup(f => f.CreateListFoldersInBucketArg(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(arg);

            _netAppClientMock
                .Setup(c => c.ListFoldersInBucketAsync(arg))
                .ReturnsAsync(response);

            _caseEnrichmentServiceMock
                .Setup(s => s.EnrichNetAppFoldersWithMetadataAsync(response))
                .ReturnsAsync(_fixture.Create<ListNetAppObjectsResponse>());
        }
    }
}
