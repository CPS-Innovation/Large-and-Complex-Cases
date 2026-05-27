using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Handlers;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class DisconnectNetAppConnectionTests
    {
        private readonly Mock<IDisconnectConnectionHandler> _disconnectConnectionHandlerMock;
        private readonly DisconnectNetAppConnection _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;

        public DisconnectNetAppConnectionTests()
        {
            _fixture = new Fixture();
            _disconnectConnectionHandlerMock = new Mock<IDisconnectConnectionHandler>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();

            _function = new DisconnectNetAppConnection(_disconnectConnectionHandlerMock.Object);
        }

        [Fact]
        public async Task Run_DelegatesToHandlerWithNetAppConnectionType()
        {
            // Arrange
            var request = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            _disconnectConnectionHandlerMock
                .Setup(x => x.RunAsync(It.IsAny<HttpRequest>(), It.IsAny<FunctionContext>(), StorageConnectionType.NetApp))
                .ReturnsAsync(new OkResult());

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _disconnectConnectionHandlerMock.Verify(
                x => x.RunAsync(request, functionContext, StorageConnectionType.NetApp),
                Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsResultFromHandler()
        {
            // Arrange
            var expectedResult = new OkResult();
            var request = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            _disconnectConnectionHandlerMock
                .Setup(x => x.RunAsync(It.IsAny<HttpRequest>(), It.IsAny<FunctionContext>(), StorageConnectionType.NetApp))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public async Task Run_DoesNotDelegatesToEgressConnectionType()
        {
            // Arrange
            var request = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            _disconnectConnectionHandlerMock
                .Setup(x => x.RunAsync(It.IsAny<HttpRequest>(), It.IsAny<FunctionContext>(), It.IsAny<StorageConnectionType>()))
                .ReturnsAsync(new OkResult());

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _disconnectConnectionHandlerMock.Verify(
                x => x.RunAsync(It.IsAny<HttpRequest>(), It.IsAny<FunctionContext>(), StorageConnectionType.Egress),
                Times.Never);
        }
    }
}
