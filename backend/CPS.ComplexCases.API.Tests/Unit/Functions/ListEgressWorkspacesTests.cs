using System.Text.Json;
using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class ListEgressWorkspacesTests
    {
        private readonly Mock<ILogger<ListEgressWorkspaces>> _loggerMock;
        private readonly Mock<IEgressClient> _egressClientMock;
        private readonly Mock<IEgressArgFactory> _egressArgFactoryMock;
        private readonly Mock<ICaseEnrichmentService> _caseEnrichmentServiceMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly Fixture _fixture;
        private readonly ListEgressWorkspaces _function;

        public ListEgressWorkspacesTests()
        {
            _loggerMock = new Mock<ILogger<ListEgressWorkspaces>>();
            _egressClientMock = new Mock<IEgressClient>();
            _egressArgFactoryMock = new Mock<IEgressArgFactory>();
            _caseEnrichmentServiceMock = new Mock<ICaseEnrichmentService>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();
            _fixture = new Fixture();
            _function = new ListEgressWorkspaces(
                _loggerMock.Object,
                _egressClientMock.Object,
                _egressArgFactoryMock.Object,
                _caseEnrichmentServiceMock.Object,
                _initializationHandlerMock.Object);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithEnrichedResponse()
        {
            // Arrange
            var correlationId = _fixture.Create<Guid>();
            var username = _fixture.Create<string>();
            var cmsAuthValues = _fixture.Create<string>();

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
                .Setup(s => s.EnrichEgressWorkspacesWithMetadataAsync(egressResponse, cmsAuthValues, correlationId))
                .ReturnsAsync(enrichedResponse);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, cmsAuthValues, username, _fixture.Create<string>());
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams, correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedResponse, okResult.Value);

            _egressArgFactoryMock.Verify(f => f.CreateListEgressWorkspacesArg(operationName, skip, take), Times.Once);
            _egressClientMock.Verify(c => c.ListWorkspacesAsync(egressArg, username), Times.Once);
            _caseEnrichmentServiceMock.Verify(s => s.EnrichEgressWorkspacesWithMetadataAsync(egressResponse, cmsAuthValues, correlationId), Times.Once);
        }

        [Fact]
        public async Task Run_SerializesConnectedCaseDetails_ForConnectedAndUnconnectedWorkspaces()
        {
            // Arrange
            var correlationId = _fixture.Create<Guid>();
            var username = _fixture.Create<string>();
            var cmsAuthValues = _fixture.Create<string>();

            var egressResponse = _fixture.Create<ListWorkspacesDto>();
            var enrichedResponse = new ListWorkspacesResponse
            {
                Pagination = new PaginationResponse { Count = 2, Take = 2, Skip = 0, TotalResults = 2 },
                Data =
                [
                    new ListWorkspaceDataResponse
                    {
                        Id = "connected-workspace",
                        Name = "Connected",
                        CaseId = 123,
                        ConnectedCaseUrn = "01TS0000001",
                        ConnectedCaseName = "Operation Thunder"
                    },
                    new ListWorkspaceDataResponse
                    {
                        Id = "unconnected-workspace",
                        Name = "Unconnected"
                    }
                ]
            };

            _egressClientMock
                .Setup(c => c.ListWorkspacesAsync(It.IsAny<ListEgressWorkspacesArg>(), username))
                .ReturnsAsync(egressResponse);

            _caseEnrichmentServiceMock
                .Setup(s => s.EnrichEgressWorkspacesWithMetadataAsync(egressResponse, cmsAuthValues, correlationId))
                .ReturnsAsync(enrichedResponse);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, cmsAuthValues, username, _fixture.Create<string>());
            var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>(), correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            using var json = JsonDocument.Parse(JsonSerializer.Serialize(okResult.Value));
            var data = json.RootElement.GetProperty("Data");

            var connected = data[0];
            Assert.Equal(123, connected.GetProperty("caseId").GetInt32());
            Assert.Equal("01TS0000001", connected.GetProperty("connectedCaseUrn").GetString());
            Assert.Equal("Operation Thunder", connected.GetProperty("connectedCaseName").GetString());

            var unconnected = data[1];
            Assert.Equal(JsonValueKind.Null, unconnected.GetProperty("caseId").ValueKind);
            Assert.Equal(JsonValueKind.Null, unconnected.GetProperty("connectedCaseUrn").ValueKind);
            Assert.Equal(JsonValueKind.Null, unconnected.GetProperty("connectedCaseName").ValueKind);
        }
    }
}
