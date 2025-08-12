using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AutoFixture;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class GetCaseTests
    {
        private readonly Mock<ILogger<GetCase>> _loggerMock;
        private readonly Mock<ICaseMetadataService> _caseClientMock;
        private readonly Mock<IDdeiClient> _ddeiClientMock;
        private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
        private readonly Fixture _fixture;
        private readonly GetCase _function;

        public GetCaseTests()
        {
            _loggerMock = new Mock<ILogger<GetCase>>();
            _caseClientMock = new Mock<ICaseMetadataService>();
            _ddeiClientMock = new Mock<IDdeiClient>();
            _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
            _fixture = new Fixture();
            _function = new GetCase(
                _loggerMock.Object,
                _caseClientMock.Object,
                _ddeiClientMock.Object,
                _ddeiArgFactoryMock.Object);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithExpectedResponse_WhenCaseExists()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var correlationId = _fixture.Create<Guid>();
            var cmsAuthValues = _fixture.Create<string>();
            var caseMetadata = _fixture.Build<CaseMetadata>()
                                      .With(c => c.CaseId, caseId)
                                      .Create();
            var cmsResponse = _fixture.Create<CaseDto>();
            var caseArg = _fixture.Create<DdeiCaseIdArgDto>();

            _caseClientMock
                .Setup(c => c.GetCaseMetadataForCaseIdAsync(caseId))
                .ReturnsAsync(caseMetadata);

            _ddeiArgFactoryMock
                .Setup(f => f.CreateCaseArg(cmsAuthValues, correlationId, caseId))
                .Returns(caseArg);

            _ddeiClientMock
                .Setup(c => c.GetCaseAsync(caseArg))
                .ReturnsAsync(cmsResponse);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, cmsAuthValues, _fixture.Create<string>());
            var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext, caseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CaseWithMetadataResponse>(okResult.Value);

            Assert.Equal(caseMetadata.CaseId, response.CaseId);
            Assert.Equal(caseMetadata.EgressWorkspaceId, response.EgressWorkspaceId);
            Assert.Equal(caseMetadata.NetappFolderPath, response.NetappFolderPath);
            Assert.Equal(cmsResponse.Urn, response.Urn);
            Assert.Equal(cmsResponse.OperationName, response.OperationName);
            Assert.Equal(caseMetadata.ActiveTransferId, response.ActiveTransferId);

            _caseClientMock.Verify(c => c.GetCaseMetadataForCaseIdAsync(caseId), Times.Once);
            _ddeiArgFactoryMock.Verify(f => f.CreateCaseArg(cmsAuthValues, correlationId, caseId), Times.Once);
            _ddeiClientMock.Verify(c => c.GetCaseAsync(caseArg), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsNotFound_WhenCaseDoesNotExist()
        {
            // Arrange
            var caseId = _fixture.Create<int>();
            var correlationId = _fixture.Create<Guid>();

            _caseClientMock
                .Setup(c => c.GetCaseMetadataForCaseIdAsync(caseId))
                .ReturnsAsync((CaseMetadata?)null);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>());
            var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext, caseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Case with ID {caseId} not found.", notFoundResult.Value);

            _caseClientMock.Verify(c => c.GetCaseMetadataForCaseIdAsync(caseId), Times.Once);
            _ddeiArgFactoryMock.VerifyNoOtherCalls();
            _ddeiClientMock.VerifyNoOtherCalls();
        }
    }
}
