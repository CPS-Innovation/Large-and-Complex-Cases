using System.Net;
using System.Text;
using System.Text.Json;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Functions.Transfer;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions.Transfer
{
    public class GetTransferStatusTests
    {
        private readonly Mock<IFileTransferClient> _transferClientMock;
        private readonly GetTransferStatus _function;
        private readonly Guid _correlationId = Guid.NewGuid();

        public GetTransferStatusTests()
        {
            _transferClientMock = new Mock<IFileTransferClient>();
            _function = new GetTransferStatus(Mock.Of<ILogger<GetTransferStatus>>(), _transferClientMock.Object);
        }

        [Fact]
        public async Task Run_TransferFound_ReturnsOkObjectResultWithEntity()
        {
            // Arrange
            var transferId = "transfer-123";
            var httpRequestMock = new Mock<HttpRequest>();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, "cmsAuthValues", "testUser");

            var transferEntity = new
            {
                Id = Guid.NewGuid(),
                CaseId = 456,
                DestinationPath = "/data/files",
                TotalFiles = 10,
                ProcessedFiles = 5,
                SuccessfulFiles = 4,
                FailedFiles = 1
            };

            var json = JsonSerializer.Serialize(transferEntity);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            _transferClientMock
                .Setup(c => c.GetFileTransferStatusAsync(transferId, _correlationId))
                .ReturnsAsync(response);

            // Act
            var result = await _function.Run(httpRequestMock.Object, functionContext, transferId);

            // Assert
            var okResult = Assert.IsType<ContentResult>(result);
            Assert.False(string.IsNullOrEmpty(okResult.Content));
            var returnedEntity = JsonSerializer.Deserialize<JsonElement>(okResult.Content!);

            Assert.Equal(transferEntity.Id.ToString(), returnedEntity.GetProperty("Id").GetString());
            Assert.Equal(transferEntity.CaseId, returnedEntity.GetProperty("CaseId").GetInt32());
            Assert.Equal(transferEntity.TotalFiles, returnedEntity.GetProperty("TotalFiles").GetInt32());


            _transferClientMock.Verify(c => c.GetFileTransferStatusAsync(transferId, _correlationId), Times.Once);
        }

        [Fact]
        public async Task Run_TransferNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var transferId = "transfer-404";
            var httpRequestMock = new Mock<HttpRequest>();
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, "cmsAuthValues", "testUser");

            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Transfer not found")
            };

            _transferClientMock
                .Setup(c => c.GetFileTransferStatusAsync(transferId, _correlationId))
                .ReturnsAsync(response);

            // Act
            var result = await _function.Run(httpRequestMock.Object, functionContext, transferId);

            // Assert
            var notFoundResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);

            _transferClientMock.Verify(c => c.GetFileTransferStatusAsync(transferId, _correlationId), Times.Once);
        }

    }
}