using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class GetAreasTests
    {
        private readonly Mock<ILogger<GetAreas>> _loggerMock;
        private readonly Mock<IDdeiClient> _ddeiClientMock;
        private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly Fixture _fixture;
        private readonly GetAreas _function;

        public GetAreasTests()
        {
            _loggerMock = new Mock<ILogger<GetAreas>>();
            _ddeiClientMock = new Mock<IDdeiClient>();
            _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();
            _fixture = new Fixture();
            _function = new GetAreas(_loggerMock.Object, _ddeiClientMock.Object, _ddeiArgFactoryMock.Object, _initializationHandlerMock.Object);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithExpectedAreas()
        {
            // Arrange
            var expectedAreasDto = _fixture.Create<AreasDto>();
            var correlationId = _fixture.Create<Guid>();
            var cmsAuthValues = _fixture.Create<string>();
            var baseArg = _fixture.Create<DdeiBaseArgDto>();

            _ddeiArgFactoryMock
                .Setup(f => f.CreateBaseArg(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(baseArg);

            _ddeiClientMock
                .Setup(c => c.GetAreasAsync(baseArg))
                .ReturnsAsync(expectedAreasDto);

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
            var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

            // Act
            var result = await _function.Run(httpRequest, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedAreasDto, okResult.Value);

            _ddeiClientMock.Verify(c => c.GetAreasAsync(baseArg), Times.Once);
        }
    }
}
