using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Functions.Transfer;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions.Transfer
{
    public class GetFilesForTransferTests
    {
        private readonly Mock<IFileTransferClient> _transferClientMock;
        private readonly Mock<ILogger<GetFilesForTransfer>> _loggerMock;
        private readonly Mock<IRequestValidator> _requestValidatorMock;
        private readonly GetFilesForTransfer _function;
        private readonly Guid _correlationId;
        private readonly Fixture _fixture;

        public GetFilesForTransferTests()
        {
            _transferClientMock = new Mock<IFileTransferClient>();
            _loggerMock = new Mock<ILogger<GetFilesForTransfer>>();
            _requestValidatorMock = new Mock<IRequestValidator>();

            _function = new GetFilesForTransfer(_transferClientMock.Object, _loggerMock.Object, _requestValidatorMock.Object);

            _fixture = new Fixture();
            _correlationId = _fixture.Create<Guid>();
        }

        [Fact]
        public async Task Run_ValidRequest_ReturnsOkObjectResult_WithDeserializedContent()
        {
            // Arrange
            var httpRequestMock = new Mock<HttpRequest>();

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

            var validRequest = new ValidatableRequest<GetFilesForTransferRequest>
            {
                IsValid = true,
                Value = new GetFilesForTransferRequest
                {
                    CaseId = _fixture.Create<int>(),
                    TransferDirection = TransferDirection.EgressToNetApp,
                    TransferType = TransferType.Copy,
                    DestinationPath = _fixture.Create<string>(),
                    WorkspaceId = _fixture.Create<string>(),
                    SourcePaths = new List<SourcePath>
                    {
                        new SourcePath { Path = "/source/path", FileId = "file1", IsFolder = false }
                    }
                }
            };

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<GetFilesForTransferRequest, GetFilesForTransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(validRequest);

            var expectedResponse = new FilesForTransferResult
            {
                CaseId = validRequest.Value.CaseId,
                WorkspaceId = validRequest.Value.WorkspaceId,
                TransferDirection = validRequest.Value.TransferDirection.ToString(),
                DestinationPath = validRequest.Value.DestinationPath,
                Files = new List<FileTransferInfo>
                {
                    new FileTransferInfo
                    {
                        SourcePath = "/source/path"
                    }
                },
                ValidationErrors = null,
                IsInvalid = false
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse);

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            };

            _transferClientMock
                .Setup(c => c.ListFilesForTransferAsync(It.IsAny<ListFilesForTransferRequest>(), _correlationId))
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _function.Run(httpRequestMock.Object, functionContext);

            // Assert
            var response = Assert.IsType<ContentResult>(result);

            var actualJson = response?.Content ?? string.Empty;
            var actualResult = JsonSerializer.Deserialize<FilesForTransferResult>(actualJson);

            Assert.NotNull(actualResult);
            Assert.Equal(expectedResponse.CaseId, actualResult.CaseId);
            Assert.Equal(expectedResponse.WorkspaceId, actualResult.WorkspaceId);
            Assert.Equal(expectedResponse.TransferDirection, actualResult.TransferDirection);
            Assert.Equal(expectedResponse.DestinationPath, actualResult.DestinationPath);
            Assert.False(actualResult.IsInvalid);
            Assert.Null(actualResult.ValidationErrors);
            Assert.NotEmpty(actualResult.Files);

            _transferClientMock.Verify(c => c.ListFilesForTransferAsync(It.IsAny<ListFilesForTransferRequest>(), _correlationId), Times.Once);
            _requestValidatorMock.Verify(v => v.GetJsonBody<GetFilesForTransferRequest, GetFilesForTransferRequestValidator>(httpRequestMock.Object), Times.Once);
        }

        [Fact]
        public async Task Run_InvalidRequest_ReturnsBadRequestObjectResult_WithValidationErrors()
        {
            // Arrange
            var httpRequestMock = new Mock<HttpRequest>();

            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

            var invalidRequest = new ValidatableRequest<GetFilesForTransferRequest>
            {
                IsValid = false,
                ValidationErrors = new List<string> { "CaseId is required", "SourcePaths cannot be empty" },
                Value = null!
            };

            _requestValidatorMock
                .Setup(v => v.GetJsonBody<GetFilesForTransferRequest, GetFilesForTransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(invalidRequest);

            // Act
            var result = await _function.Run(httpRequestMock.Object, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
            Assert.Contains("CaseId is required", errors);
            Assert.Contains("SourcePaths cannot be empty", errors);

            _requestValidatorMock.Verify(v => v.GetJsonBody<GetFilesForTransferRequest, GetFilesForTransferRequestValidator>(httpRequestMock.Object), Times.Once);

            _transferClientMock.Verify(c => c.ListFilesForTransferAsync(It.IsAny<ListFilesForTransferRequest>(), It.IsAny<Guid>()), Times.Never);
        }

    }
}
