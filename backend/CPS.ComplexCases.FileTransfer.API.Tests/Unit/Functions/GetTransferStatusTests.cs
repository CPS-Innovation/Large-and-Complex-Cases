using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class GetTransferStatusTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<GetTransferStatus>> _loggerMock;
    private readonly Mock<HttpRequest> _httpRequestMock;
    private readonly Mock<IHeaderDictionary> _headersMock;
    private readonly GetTransferStatus _function;
    private readonly string _transferId;
    private readonly string _correlationId;

    public GetTransferStatusTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _loggerMock = new Mock<ILogger<GetTransferStatus>>();
        _httpRequestMock = new Mock<HttpRequest>();
        _headersMock = new Mock<IHeaderDictionary>();

        _transferId = _fixture.Create<Guid>().ToString();
        _correlationId = _fixture.Create<Guid>().ToString();

        _httpRequestMock.Setup(r => r.Headers).Returns(_headersMock.Object);
        _headersMock.Setup(h => h[HttpHeaderKeys.CorrelationId])
            .Returns(new StringValues(_correlationId));

        _headersMock.Setup(h => h.TryGetValue(HttpHeaderKeys.CorrelationId, out It.Ref<StringValues>.IsAny))
            .Callback((string key, out StringValues value) =>
            {
                value = new StringValues(_correlationId);
            })
            .Returns(true);

        _function = new GetTransferStatus(_loggerMock.Object);
    }

    [Fact]
    public async Task Run_WithValidTransferId_ReturnsOkResultWithTransferState()
    {
        // Arrange
        var transferEntity = CreateValidTransferEntity();

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };

        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var returnedEntity = Assert.IsType<TransferEntity>(okResult.Value);
        Assert.Equal(transferEntity.Id, returnedEntity.Id);
        Assert.Equal(transferEntity.Status, returnedEntity.Status);
        Assert.Equal(transferEntity.DestinationPath, returnedEntity.DestinationPath);
        Assert.Equal(transferEntity.SourcePaths, returnedEntity.SourcePaths);
        Assert.Equal(transferEntity.CaseId, returnedEntity.CaseId);
        Assert.Equal(transferEntity.TransferType, returnedEntity.TransferType);
        Assert.Equal(transferEntity.Direction, returnedEntity.Direction);
        Assert.Equal(transferEntity.TotalFiles, returnedEntity.TotalFiles);
        Assert.Equal(transferEntity.IsRetry, returnedEntity.IsRetry);
        Assert.Equal(transferEntity.CreatedAt, returnedEntity.CreatedAt);
        Assert.Equal(transferEntity.UpdatedAt, returnedEntity.UpdatedAt);
    }

    [Fact]
    public async Task Run_WhenEntityClientThrowsException_ExceptionIsNotCaught()
    {
        // Arrange
        var expected = new InvalidOperationException("Failure");

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (_, _) => throw expected
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId));

        Assert.Equal(expected, ex);
    }

    [Fact]
    public async Task Run_WithDifferentTransferStatuses_ReturnsCorrectStatus()
    {
        var statuses = new[]
        {
            TransferStatus.Initiated,
            TransferStatus.InProgress,
            TransferStatus.Completed,
            TransferStatus.Failed
        };

        foreach (var status in statuses)
        {
            var entity = CreateValidTransferEntity();
            entity.Status = status;

            var stub = new DurableEntityClientStub("FileTransferEntities")
            {
                OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, entity))
            };
            var durableTaskClientStub = new DurableTaskClientStub(stub);

            // Act
            var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TransferEntity>(okResult.Value);
            Assert.Equal(status, returned.Status);
        }
    }

    [Fact]
    public async Task Run_ExtractsCorrelationIdFromHeaders()
    {
        var transferEntity = CreateValidTransferEntity();

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        _httpRequestMock.Verify(r => r.Headers, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Run_EntityNotFound_ReturnsNotFoundObjectResult()
    {
        // Arrange
        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (_, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(null)
        };

        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFound.Value);
    }

    private TransferEntity CreateValidTransferEntity()
    {
        return new TransferEntity
        {
            Id = Guid.Parse(_transferId),
            Status = _fixture.Create<TransferStatus>(),
            DestinationPath = _fixture.Create<string>(),
            SourcePaths = _fixture.CreateMany<TransferSourcePath>(2).ToList(),
            CaseId = _fixture.Create<int>(),
            TransferType = _fixture.Create<TransferType>(),
            Direction = _fixture.Create<TransferDirection>(),
            TotalFiles = _fixture.Create<int>(),
            IsRetry = _fixture.Create<bool>(),
            CreatedAt = _fixture.Create<DateTime>(),
            UpdatedAt = _fixture.Create<DateTime>()
        };
    }
}
