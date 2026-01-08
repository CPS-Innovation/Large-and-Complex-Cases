using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class ListEgressTemplatesTests
    {
        private readonly Mock<ILogger<ListEgressTemplates>> _loggerMock;
        private readonly Mock<IEgressClient> _egressClientMock;
        private readonly Mock<IEgressArgFactory> _egressArgFactoryMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly Fixture _fixture;
        private readonly ListEgressTemplates _function;
        private readonly Guid _correlationId;

        public ListEgressTemplatesTests()
        {
            _loggerMock = new Mock<ILogger<ListEgressTemplates>>();
            _egressClientMock = new Mock<IEgressClient>();
            _egressArgFactoryMock = new Mock<IEgressArgFactory>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();
            _fixture = new Fixture();
            _function = new ListEgressTemplates(
                _loggerMock.Object,
                _egressClientMock.Object,
                _egressArgFactoryMock.Object,
                _initializationHandlerMock.Object);
            _correlationId = _fixture.Create<Guid>();
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithExpectedResponse_WhenCalledWithQueryParameters()
        {
            // Arrange
            var username = _fixture.Create<string>();
            var cmsAuthValues = _fixture.Create<string>();
            var bearerToken = _fixture.Create<string>();
            var correlationId = _fixture.Create<Guid>();
            var skip = _fixture.Create<int>();
            var take = _fixture.Create<int>();

            var listTemplatesResponse = _fixture.Create<ListTemplatesDto>();
            var paginationArg = _fixture.Create<PaginationArg>();

            _egressArgFactoryMock
                .Setup(f => f.CreatePaginationArg(skip, take))
                .Returns(paginationArg);

            _egressClientMock
                .Setup(c => c.ListTemplatesAsync(paginationArg))
                .ReturnsAsync(listTemplatesResponse);

            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.Skip] = skip.ToString(),
                [InputParameters.Take] = take.ToString()
            };

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, cmsAuthValues, username, bearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams, _correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(listTemplatesResponse, okResult.Value);

            _egressArgFactoryMock.Verify(f => f.CreatePaginationArg(skip, take), Times.Once);
            _egressClientMock.Verify(c => c.ListTemplatesAsync(paginationArg), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithDefaultPagination_WhenQueryParametersNotProvided()
        {
            // Arrange
            var username = _fixture.Create<string>();
            var cmsAuthValues = _fixture.Create<string>();
            var bearerToken = _fixture.Create<string>();
            var correlationId = _fixture.Create<Guid>();
            var defaultSkip = 0;
            var defaultTake = 100;

            var listTemplatesResponse = _fixture.Create<ListTemplatesDto>();
            var paginationArg = _fixture.Create<PaginationArg>();

            _egressArgFactoryMock
                .Setup(f => f.CreatePaginationArg(defaultSkip, defaultTake))
                .Returns(paginationArg);

            _egressClientMock
                .Setup(c => c.ListTemplatesAsync(paginationArg))
                .ReturnsAsync(listTemplatesResponse);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, cmsAuthValues, username, bearerToken);
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters([], _correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(listTemplatesResponse, okResult.Value);

            _egressArgFactoryMock.Verify(f => f.CreatePaginationArg(defaultSkip, defaultTake), Times.Once);
            _egressClientMock.Verify(c => c.ListTemplatesAsync(paginationArg), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithDefaultPagination_WhenQueryParametersAreInvalid()
        {
            // Arrange
            var username = _fixture.Create<string>();
            var cmsAuthValues = _fixture.Create<string>();
            var bearerToken = _fixture.Create<string>();
            var correlationId = _fixture.Create<Guid>();
            var defaultSkip = 0;
            var defaultTake = 100;

            var listTemplatesResponse = _fixture.Create<ListTemplatesDto>();
            var paginationArg = _fixture.Create<PaginationArg>();

            _egressArgFactoryMock
                .Setup(f => f.CreatePaginationArg(defaultSkip, defaultTake))
                .Returns(paginationArg);

            _egressClientMock
                .Setup(c => c.ListTemplatesAsync(paginationArg))
                .ReturnsAsync(listTemplatesResponse);

            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.Skip] = "invalid",
                [InputParameters.Take] = "invalid"
            };
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams, correlationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, cmsAuthValues, username, bearerToken);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(listTemplatesResponse, okResult.Value);

            _egressArgFactoryMock.Verify(f => f.CreatePaginationArg(defaultSkip, defaultTake), Times.Once);
            _egressClientMock.Verify(c => c.ListTemplatesAsync(paginationArg), Times.Once);
        }
    }
}