using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Dtos;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
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
        _httpRequestMock.Setup(r => r.HttpContext).Returns(new DefaultHttpContext());

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
    public async Task Run_WithValidTransferId_ReturnsOkResultWithTransferStatusDto()
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
        var returnedDto = Assert.IsType<TransferStatusDto>(okResult.Value);
        Assert.Equal(transferEntity.Status, returnedDto.Status);
        Assert.Equal(transferEntity.TransferType, returnedDto.TransferType);
        Assert.Equal(transferEntity.Direction, returnedDto.Direction);
        Assert.Equal(transferEntity.DestinationPath, returnedDto.DestinationPath);
        Assert.Equal(transferEntity.SourceRootFolderPath, returnedDto.SourceRootFolderPath);
        Assert.Equal(transferEntity.TotalFiles, returnedDto.TotalFiles);
        Assert.Equal(transferEntity.ProcessedFiles, returnedDto.ProcessedFiles);
        Assert.Equal(transferEntity.UserName, returnedDto.UserName);
    }

    [Fact]
    public async Task Run_WithValidTransferId_SetsETagResponseHeader()
    {
        // Arrange
        var transferEntity = CreateValidTransferEntity();
        var httpContext = new DefaultHttpContext();
        _httpRequestMock.Setup(r => r.HttpContext).Returns(httpContext);

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert
        Assert.True(httpContext.Response.Headers.ContainsKey("ETag"));
        Assert.Equal($"\"{transferEntity.UpdatedAt.Ticks}\"", httpContext.Response.Headers["ETag"].ToString());
    }

    [Fact]
    public async Task Run_WhenIfNoneMatchMatchesETag_Returns304()
    {
        // Arrange
        var transferEntity = CreateValidTransferEntity();
        var etag = $"\"{transferEntity.UpdatedAt.Ticks}\"";

        _headersMock.Setup(h => h["If-None-Match"]).Returns(new StringValues(etag));

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(304, statusResult.StatusCode);
    }

    [Fact]
    public async Task Run_WhenIfNoneMatchDiffersFromETag_ReturnsOkWithDto()
    {
        // Arrange
        var transferEntity = CreateValidTransferEntity();
        _headersMock.Setup(h => h["If-None-Match"]).Returns(new StringValues("\"stale-etag\""));

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
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
            var returned = Assert.IsType<TransferStatusDto>(okResult.Value);
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

    [Fact]
    public async Task Run_ResponseDtoExcludesSensitiveFields()
    {
        // Arrange — entity has BearerToken and large lists that should not appear in the DTO
        var transferEntity = CreateValidTransferEntity();
        transferEntity.Status = TransferStatus.Completed;
        transferEntity.BearerToken = "sensitive-bearer-token";
        transferEntity.SuccessfulItems.Add(new TransferItem
        {
            SourcePath = "/path/file.txt",
            Status = TransferItemStatus.Completed,
            IsRenamed = false,
            Size = 1024
        });

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert — result is a DTO, not the full entity
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<TransferStatusDto>(okResult.Value);
        Assert.IsNotType<TransferEntity>(okResult.Value);

        // Successful items only expose SourcePath and Size, not timing/chunking internals
        var item = Assert.Single(dto.SuccessfulItems);
        Assert.Equal("/path/file.txt", item.SourcePath);
        Assert.Equal(1024, item.Size);
        Assert.Equal(2, typeof(TransferSuccessfulItemDto).GetProperties().Length);
    }

    [Theory]
    [InlineData(TransferStatus.Initiated)]
    [InlineData(TransferStatus.InProgress)]
    public async Task Run_WhenTransferInFlight_ReturnsEmptySuccessfulItems(TransferStatus status)
    {
        // Arrange
        var transferEntity = CreateValidTransferEntity();
        transferEntity.Status = status;
        transferEntity.SuccessfulItems.Add(new TransferItem
        {
            SourcePath = "/path/file.txt",
            Status = TransferItemStatus.Completed,
            IsRenamed = false,
            Size = 1024
        });

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<TransferStatusDto>(okResult.Value);
        Assert.Empty(dto.SuccessfulItems);
    }

    [Theory]
    [InlineData(TransferStatus.Completed)]
    [InlineData(TransferStatus.PartiallyCompleted)]
    [InlineData(TransferStatus.Failed)]
    public async Task Run_WhenTransferFinished_ReturnsSuccessfulItemsWithSourcePathAndSize(TransferStatus status)
    {
        // Arrange
        var transferEntity = CreateValidTransferEntity();
        transferEntity.Status = status;
        transferEntity.SuccessfulItems.AddRange(new[]
        {
            new TransferItem
            {
                SourcePath = "/path/file-1.txt",
                Status = TransferItemStatus.Completed,
                IsRenamed = false,
                Size = 111
            },
            new TransferItem
            {
                SourcePath = "/path/file-2.txt",
                Status = TransferItemStatus.Completed,
                IsRenamed = true,
                Size = 222
            }
        });

        var stub = new DurableEntityClientStub("FileTransferEntities")
        {
            OnGetEntityAsync = (id, _) => Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(id, transferEntity))
        };
        var durableTaskClientStub = new DurableTaskClientStub(stub);

        // Act
        var result = await _function.Run(_httpRequestMock.Object, durableTaskClientStub, _transferId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<TransferStatusDto>(okResult.Value);
        Assert.Collection(dto.SuccessfulItems,
            first =>
            {
                Assert.Equal("/path/file-1.txt", first.SourcePath);
                Assert.Equal(111, first.Size);
            },
            second =>
            {
                Assert.Equal("/path/file-2.txt", second.SourcePath);
                Assert.Equal(222, second.Size);
            });
    }

    private TransferEntity CreateValidTransferEntity()
    {
        return new TransferEntity
        {
            Id = Guid.Parse(_transferId),
            Status = _fixture.Create<TransferStatus>(),
            DestinationPath = _fixture.Create<string>(),
            SourceRootFolderPath = _fixture.Create<string>(),
            SourcePaths = _fixture.CreateMany<TransferSourcePath>(2).ToList(),
            CaseId = _fixture.Create<int>(),
            TransferType = _fixture.Create<TransferType>(),
            Direction = _fixture.Create<TransferDirection>(),
            TotalFiles = _fixture.Create<int>(),
            IsRetry = _fixture.Create<bool>(),
            CreatedAt = _fixture.Create<DateTime>(),
            UpdatedAt = _fixture.Create<DateTime>(),
            BearerToken = _fixture.Create<string>()
        };
    }
}
