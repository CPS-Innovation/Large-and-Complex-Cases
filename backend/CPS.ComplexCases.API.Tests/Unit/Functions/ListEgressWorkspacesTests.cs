using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class ListEgressWorkspacesTests
    {
        private readonly Mock<ILogger<ListEgressWorkspaces>> _loggerMock;
        private readonly Mock<IEgressClient> _egressClientMock;
        private readonly Mock<IEgressArgFactory> _egressArgFactoryMock;
        private readonly Mock<ICaseEnrichmentService> _caseEnrichmentServiceMock;
        private readonly Fixture _fixture;
        private readonly ListEgressWorkspaces _function;

        public ListEgressWorkspacesTests()
        {
            _loggerMock = new Mock<ILogger<ListEgressWorkspaces>>();
            _egressClientMock = new Mock<IEgressClient>();
            _egressArgFactoryMock = new Mock<IEgressArgFactory>();
            _caseEnrichmentServiceMock = new Mock<ICaseEnrichmentService>();
            _fixture = new Fixture();
            _function = new ListEgressWorkspaces(
                _loggerMock.Object,
                _egressClientMock.Object,
                _egressArgFactoryMock.Object,
                _caseEnrichmentServiceMock.Object);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithEnrichedResponse()
        {
            // Arrange
            var correlationId = _fixture.Create<Guid>();
            var username = _fixture.Create<string>();

            var operationName = _fixture.Create<string>();
            var skip = _fixture.Create<int>();
            var take = _fixture.Create<int>();

            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.WorkspaceName] = operationName,
                [InputParameters.Skip] = skip.ToString(),
                [InputParameters.Take] = take.ToString()
            };

            var egressArg = _fixture.Create<ListEgressWorkspacesArg>();
            var egressResponse = _fixture.Create<ListWorkspacesDto>();
            var enrichedResponse = _fixture.Create<ListWorkspacesResponse>();

            _egressArgFactoryMock
                .Setup(f => f.CreateListEgressWorkspacesArg(operationName, skip, take))
                .Returns(egressArg);

            _egressClientMock
                .Setup(c => c.ListWorkspacesAsync(egressArg, username))
                .ReturnsAsync(egressResponse);

            _caseEnrichmentServiceMock
                .Setup(s => s.EnrichEgressWorkspacesWithMetadataAsync(egressResponse))
                .ReturnsAsync(enrichedResponse);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), username, _fixture.Create<string>());
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams, correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedResponse, okResult.Value);

            _egressArgFactoryMock.Verify(f => f.CreateListEgressWorkspacesArg(operationName, skip, take), Times.Once);
            _egressClientMock.Verify(c => c.ListWorkspacesAsync(egressArg, username), Times.Once);
            _caseEnrichmentServiceMock.Verify(s => s.EnrichEgressWorkspacesWithMetadataAsync(egressResponse), Times.Once);
        }
    }
}
