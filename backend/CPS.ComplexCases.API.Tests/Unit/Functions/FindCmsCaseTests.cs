using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class FindCmsCaseTests
    {
        private readonly Mock<ILogger<FindCmsCase>> _loggerMock;
        private readonly Mock<IDdeiClient> _ddeiClientMock;
        private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
        private readonly Mock<ICaseEnrichmentService> _caseEnrichmentServiceMock;
        private readonly FindCmsCase _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;

        public FindCmsCaseTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<FindCmsCase>>();
            _ddeiClientMock = new Mock<IDdeiClient>();
            _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
            _caseEnrichmentServiceMock = new Mock<ICaseEnrichmentService>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();

            _function = new FindCmsCase(
                _loggerMock.Object,
                _ddeiClientMock.Object,
                _ddeiArgFactoryMock.Object,
                _caseEnrichmentServiceMock.Object);
        }

        [Fact]
        public async Task Run_NoSearchParameters_ReturnsBadRequest()
        {
            // Arrange
            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(new Dictionary<string, string>());
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Search Parameters Invalid", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_OnlyAreaProvided_ReturnsBadRequest()
        {
            // Arrange
            var area = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.Area] = area
            };

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Search Parameters Invalid", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_UrnProvided_CallsListCasesByUrn()
        {
            // Arrange
            var urn = _fixture.Create<string>();
            var area = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.Urn] = urn,
                [InputParameters.Area] = area
            };

            var urnArg = _fixture.Create<DdeiUrnArgDto>();
            var caseDtos = _fixture.CreateMany<CaseDto>(3).ToList();
            var enrichedCases = _fixture.CreateMany<CaseWithMetadataResponse>(3).ToList();

            _ddeiArgFactoryMock
                .Setup(x => x.CreateUrnArg(_testCmsAuthValues, _testCorrelationId, urn))
                .Returns(urnArg);

            _ddeiClientMock
                .Setup(x => x.ListCasesByUrnAsync(urnArg))
                .ReturnsAsync(caseDtos);

            _caseEnrichmentServiceMock
                .Setup(x => x.EnrichCasesWithMetadataAsync(caseDtos))
                .ReturnsAsync(enrichedCases.AsEnumerable());

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedCases, okResult.Value);

            _ddeiArgFactoryMock.Verify(x => x.CreateUrnArg(_testCmsAuthValues, _testCorrelationId, urn), Times.Once);
            _ddeiClientMock.Verify(x => x.ListCasesByUrnAsync(urnArg), Times.Once);
            _caseEnrichmentServiceMock.Verify(x => x.EnrichCasesWithMetadataAsync(caseDtos), Times.Once);
        }

        [Fact]
        public async Task Run_OperationNameAndAreaProvided_CallsListCasesByOperationName()
        {
            // Arrange
            var operationName = _fixture.Create<string>();
            var area = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = operationName,
                [InputParameters.Area] = area
            };

            var operationNameArg = _fixture.Create<DdeiOperationNameArgDto>();
            var caseDtos = _fixture.CreateMany<CaseDto>(2).ToList();
            var enrichedCases = _fixture.CreateMany<CaseWithMetadataResponse>(2).ToList();

            _ddeiArgFactoryMock
                .Setup(x => x.CreateOperationNameArg(_testCmsAuthValues, _testCorrelationId, operationName, area))
                .Returns(operationNameArg);

            _ddeiClientMock
                .Setup(x => x.ListCasesByOperationNameAsync(operationNameArg))
                .ReturnsAsync(caseDtos);

            _caseEnrichmentServiceMock
                .Setup(x => x.EnrichCasesWithMetadataAsync(caseDtos))
                .ReturnsAsync(enrichedCases);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedCases, okResult.Value);

            _ddeiArgFactoryMock.Verify(x => x.CreateOperationNameArg(_testCmsAuthValues, _testCorrelationId, operationName, area), Times.Once);
            _ddeiClientMock.Verify(x => x.ListCasesByOperationNameAsync(operationNameArg), Times.Once);
            _caseEnrichmentServiceMock.Verify(x => x.EnrichCasesWithMetadataAsync(caseDtos), Times.Once);
        }

        [Fact]
        public async Task Run_DefendantNameAndAreaProvided_CallsListCasesByDefendantName()
        {
            // Arrange
            var defendantName = _fixture.Create<string>();
            var area = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.DefendantName] = defendantName,
                [InputParameters.Area] = area
            };

            var defendantArg = _fixture.Create<DdeiDefendantNameArgDto>();
            var caseDtos = _fixture.CreateMany<CaseDto>(4).ToList();
            var enrichedCases = _fixture.CreateMany<CaseWithMetadataResponse>(4).ToList();

            _ddeiArgFactoryMock
                .Setup(x => x.CreateDefendantArg(_testCmsAuthValues, _testCorrelationId, defendantName, area))
                .Returns(defendantArg);

            _ddeiClientMock
                .Setup(x => x.ListCasesByDefendantNameAsync(defendantArg))
                .ReturnsAsync(caseDtos);

            _caseEnrichmentServiceMock
                .Setup(x => x.EnrichCasesWithMetadataAsync(caseDtos))
                .ReturnsAsync(enrichedCases);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedCases, okResult.Value);

            _ddeiArgFactoryMock.Verify(x => x.CreateDefendantArg(_testCmsAuthValues, _testCorrelationId, defendantName, area), Times.Once);
            _ddeiClientMock.Verify(x => x.ListCasesByDefendantNameAsync(defendantArg), Times.Once);
            _caseEnrichmentServiceMock.Verify(x => x.EnrichCasesWithMetadataAsync(caseDtos), Times.Once);
        }

        [Fact]
        public async Task Run_OperationNameWithoutArea_ReturnsBadRequest()
        {
            // Arrange
            var operationName = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = operationName
            };

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Search Parameters Invalid", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_DefendantNameWithoutArea_ReturnsBadRequest()
        {
            // Arrange
            var defendantName = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.DefendantName] = defendantName
            };

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Search Parameters Invalid", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_UrnTakesPrecedenceOverOtherParameters()
        {
            var urn = _fixture.Create<string>();
            var operationName = _fixture.Create<string>();
            var defendantName = _fixture.Create<string>();
            var area = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.Urn] = urn,
                [InputParameters.OperationName] = operationName,
                [InputParameters.DefendantName] = defendantName,
                [InputParameters.Area] = area
            };

            var urnArg = _fixture.Create<DdeiUrnArgDto>();
            var caseDtos = _fixture.CreateMany<CaseDto>(1).ToList();
            var enrichedCases = _fixture.CreateMany<CaseWithMetadataResponse>(1).ToList();

            _ddeiArgFactoryMock
                .Setup(x => x.CreateUrnArg(_testCmsAuthValues, _testCorrelationId, urn))
                .Returns(urnArg);

            _ddeiClientMock
                .Setup(x => x.ListCasesByUrnAsync(urnArg))
                .ReturnsAsync(caseDtos);

            _caseEnrichmentServiceMock
                .Setup(x => x.EnrichCasesWithMetadataAsync(caseDtos))
                .ReturnsAsync(enrichedCases);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedCases, okResult.Value);

            // Verify only URN-related methods were called
            _ddeiArgFactoryMock.Verify(x => x.CreateUrnArg(_testCmsAuthValues, _testCorrelationId, urn), Times.Once);
            _ddeiClientMock.Verify(x => x.ListCasesByUrnAsync(urnArg), Times.Once);

            // Verify operation name and defendant methods were NOT called
            _ddeiArgFactoryMock.Verify(x => x.CreateOperationNameArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _ddeiArgFactoryMock.Verify(x => x.CreateDefendantArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _ddeiClientMock.Verify(x => x.ListCasesByOperationNameAsync(It.IsAny<DdeiOperationNameArgDto>()), Times.Never);
            _ddeiClientMock.Verify(x => x.ListCasesByDefendantNameAsync(It.IsAny<DdeiDefendantNameArgDto>()), Times.Never);
        }

        [Fact]
        public async Task Run_OperationNameTakesPrecedenceOverDefendantName()
        {
            // Arrange - Both operation name and defendant name provided with area
            var operationName = _fixture.Create<string>();
            var defendantName = _fixture.Create<string>();
            var area = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.OperationName] = operationName,
                [InputParameters.DefendantName] = defendantName,
                [InputParameters.Area] = area
            };

            var operationNameArg = _fixture.Create<DdeiOperationNameArgDto>();
            var caseDtos = _fixture.CreateMany<CaseDto>(2).ToList();
            var enrichedCases = _fixture.CreateMany<CaseWithMetadataResponse>(2).ToList();

            _ddeiArgFactoryMock
                .Setup(x => x.CreateOperationNameArg(_testCmsAuthValues, _testCorrelationId, operationName, area))
                .Returns(operationNameArg);

            _ddeiClientMock
                .Setup(x => x.ListCasesByOperationNameAsync(operationNameArg))
                .ReturnsAsync(caseDtos);

            _caseEnrichmentServiceMock
                .Setup(x => x.EnrichCasesWithMetadataAsync(caseDtos))
                .ReturnsAsync(enrichedCases);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(enrichedCases, okResult.Value);

            // Verify only operation name methods were called
            _ddeiArgFactoryMock.Verify(x => x.CreateOperationNameArg(_testCmsAuthValues, _testCorrelationId, operationName, area), Times.Once);
            _ddeiClientMock.Verify(x => x.ListCasesByOperationNameAsync(operationNameArg), Times.Once);

            // Verify defendant methods were NOT called
            _ddeiArgFactoryMock.Verify(x => x.CreateDefendantArg(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _ddeiClientMock.Verify(x => x.ListCasesByDefendantNameAsync(It.IsAny<DdeiDefendantNameArgDto>()), Times.Never);
        }

        [Fact]
        public async Task Run_EmptyCaseList_ReturnsOkWithEmptyEnrichedResult()
        {
            // Arrange
            var urn = _fixture.Create<string>();
            var area = _fixture.Create<string>();
            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.Urn] = urn,
                [InputParameters.Area] = area
            };

            var urnArg = _fixture.Create<DdeiUrnArgDto>();
            var emptyCaseDtos = new List<CaseDto>();
            var emptyEnrichedCases = new List<CaseWithMetadataResponse>();

            _ddeiArgFactoryMock
                .Setup(x => x.CreateUrnArg(_testCmsAuthValues, _testCorrelationId, urn))
                .Returns(urnArg);

            _ddeiClientMock
                .Setup(x => x.ListCasesByUrnAsync(urnArg))
                .ReturnsAsync(emptyCaseDtos);

            _caseEnrichmentServiceMock
                .Setup(x => x.EnrichCasesWithMetadataAsync(emptyCaseDtos))
                .ReturnsAsync(emptyEnrichedCases);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsAssignableFrom<IEnumerable<CaseWithMetadataResponse>>(okResult.Value);
            Assert.Empty(resultValue);

            _caseEnrichmentServiceMock.Verify(x => x.EnrichCasesWithMetadataAsync(emptyCaseDtos), Times.Once);
        }
    }
}