using System.Net;
using System.Text;
using AutoFixture;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Functions.Transfer;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using CPS.ComplexCases.Common.Models;

namespace CPS.ComplexCases.API.Tests.Unit.Functions.Transfer
{
    public class InitiateTransferTests
    {
        private readonly Mock<ILogger<InitiateTransfer>> _loggerMock;
        private readonly Mock<IFileTransferClient> _fileTransferClientMock;
        private readonly Mock<IRequestValidator> _requestValidatorMock;
        private readonly InitiateTransfer _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;

        public InitiateTransferTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<InitiateTransfer>>();
            _fileTransferClientMock = new Mock<IFileTransferClient>();
            _requestValidatorMock = new Mock<IRequestValidator>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();

            _function = new InitiateTransfer(
                _loggerMock.Object,
                _fileTransferClientMock.Object,
                _requestValidatorMock.Object
            );
        }

        [Fact]
        public async Task Run_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var validationErrors = new List<string> { "DestinationPath is required." };
            var initiateRequest = _fixture.Create<InitiateTransferRequest>();

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<InitiateTransferRequest, InitiateTransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<InitiateTransferRequest>
                {
                    IsValid = false,
                    ValidationErrors = validationErrors,
                    Value = initiateRequest
                });

            var request = HttpRequestStubHelper.CreateHttpRequestFor(initiateRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(validationErrors, badRequestResult.Value);
            _fileTransferClientMock.Verify(c => c.InitiateFileTransferAsync(It.IsAny<TransferRequest>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Run_ValidRequest_CallsTransferClient_AndReturnsExpectedResult()
        {
            // Arrange
            var initiateRequest = new InitiateTransferRequest
            {
                TransferType = TransferType.Copy,
                TransferDirection = TransferDirection.EgressToNetApp,
                DestinationPath = "/destination",
                CaseId = 123,
                WorkspaceId = "workspace-1",
                SourcePaths = new List<SourcePath>
                {
                    new SourcePath
                    {
                        Path = "/source/file1.txt",
                        FileId = "file1"
                    }
                }
            };

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<InitiateTransferRequest, InitiateTransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<InitiateTransferRequest>
                {
                    IsValid = true,
                    Value = initiateRequest
                });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Transfer initiated", Encoding.UTF8, "application/json")
            };

            _fileTransferClientMock
                .Setup(c => c.InitiateFileTransferAsync(It.IsAny<TransferRequest>(), It.IsAny<Guid>()))
                .ReturnsAsync(httpResponse);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(initiateRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            _fileTransferClientMock.Verify(c => c.InitiateFileTransferAsync(It.Is<TransferRequest>(r =>
                r.DestinationPath == initiateRequest.DestinationPath &&
                r.SourcePaths.Count == initiateRequest.SourcePaths.Count &&
                r.Metadata.CaseId == initiateRequest.CaseId &&
                r.Metadata.WorkspaceId == initiateRequest.WorkspaceId
            ), It.IsAny<Guid>()), Times.Once);

            Assert.IsType<ContentResult>(result);
        }
    }
}
