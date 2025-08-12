using CPS.ComplexCases.API.Functions.Transfer;
using CPS.ComplexCases.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Tests.Unit.Functions.Transfer;

public class ClearTransferTests
{
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<ILogger<ClearTransfer>> _loggerMock;
    private readonly ClearTransfer _function;

    public ClearTransferTests()
    {
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _loggerMock = new Mock<ILogger<ClearTransfer>>();
        _function = new ClearTransfer(_caseMetadataServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Run_CallsClearActiveTransferIdAsync_WithCorrectTransferId_AndReturnsOkResult()
    {
        // Arrange
        var transferId = Guid.NewGuid();

        var httpRequestMock = new Mock<HttpRequest>();
        var functionContextMock = new Mock<FunctionContext>();

        // Act
        var result = await _function.Run(httpRequestMock.Object, functionContextMock.Object, transferId);

        // Assert
        _caseMetadataServiceMock.Verify(svc => svc.ClearActiveTransferIdAsync(transferId), Times.Once);

        Assert.IsType<OkResult>(result);
    }
}
