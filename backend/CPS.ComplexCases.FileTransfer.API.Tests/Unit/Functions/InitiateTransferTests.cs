using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Responses;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using CPS.ComplexCases.FileTransfer.API.Validators;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions
{
    public class InitiateTransferTests
    {
        private readonly Mock<ILogger<InitiateTransfer>> _loggerMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<IRequestValidator> _requestValidatorMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly DurableEntityClientStub _entityClientStub;
        private readonly DurableTaskClientStub _durableClientStub;
        private readonly InitiateTransfer _function;
        private readonly Guid _testCorrelationId;
        private readonly Fixture _fixture;

        public InitiateTransferTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<InitiateTransfer>>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _requestValidatorMock = new Mock<IRequestValidator>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();

            _entityClientStub = new DurableEntityClientStub("TestEntityClient");
            _durableClientStub = new DurableTaskClientStub(_entityClientStub);
            _testCorrelationId = _fixture.Create<Guid>();

            _function = new InitiateTransfer(
                _loggerMock.Object,
                _caseMetadataServiceMock.Object,
                _requestValidatorMock.Object,
                _initializationHandlerMock.Object);
        }

        [Fact]
        public async Task Run_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var validationErrors = _fixture.CreateMany<string>(2).ToList();
            var transferRequest = _fixture.Build<TransferRequest>()
                .With(x => x.Metadata, _fixture.Create<TransferMetadata>())
                .Create();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<TransferRequest, TransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<TransferRequest>
                {
                    IsValid = false,
                    ValidationErrors = validationErrors,
                    Value = transferRequest
                });

            var request = CreateHttpRequestFor(transferRequest);

            // Act
            var result = await _function.Run(request, _durableClientStub);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
            Assert.Equal(validationErrors, errors);
        }

        [Fact]
        public async Task Run_RequestMissingMetadata_ReturnsBadRequest()
        {
            // Arrange
            var transferRequest = _fixture.Build<TransferRequest>()
                .With(x => x.Metadata, (TransferMetadata)null!)
                .Create();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<TransferRequest, TransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<TransferRequest>
                {
                    IsValid = true,
                    Value = transferRequest
                });

            var request = CreateHttpRequestFor(transferRequest);

            // Act
            var result = await _function.Run(request, _durableClientStub);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Metadata is required.", badRequestResult.Value);
        }
        [Fact]
        public async Task Run_ActiveTransferInProgress_ReturnsAcceptedWithStatus()
        {
            // Arrange
            var transferId = Guid.NewGuid();
            var caseId = _fixture.Create<int>();
            var transferRequest = _fixture.Build<TransferRequest>()
                .With(x => x.Metadata, _fixture.Build<TransferMetadata>().With(m => m.CaseId, caseId).Create())
                .Create();

            var validResult = new ValidatableRequest<TransferRequest>
            {
                IsValid = true,
                Value = transferRequest
            };

            var caseMetadata = new CaseMetadata
            {
                ActiveTransferId = transferId,
                CaseId = caseId
            };

            var entityState = new TransferEntity
            {
                Status = TransferStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                DestinationPath = transferRequest.DestinationPath,
                BearerToken = "fakeBearerToken"
            };

            _entityClientStub.OnGetEntityAsync = (entityId, ct) =>
            {
                var metadata = new EntityMetadata<TransferEntity>(entityId, entityState);
                return Task.FromResult<EntityMetadata<TransferEntity>?>(metadata);
            };

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<TransferRequest, TransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(validResult);

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForCaseIdAsync(caseId))
                .ReturnsAsync(caseMetadata);

            var request = CreateHttpRequestFor(transferRequest);

            // Act
            var result = await _function.Run(request, _durableClientStub);

            // Assert
            var accepted = Assert.IsType<AcceptedResult>(result);
            var response = Assert.IsType<TransferResponse>(accepted.Value);
            Assert.Equal(transferId, response.Id);
            Assert.Equal(TransferStatus.InProgress, response.Status);
            Assert.Equal(entityState.CreatedAt, response.CreatedAt);
            Assert.Equal($"/api/v1/filetransfer/{transferId}/status", accepted.Location);
        }
        [Fact]
        public async Task Run_NoActiveTransfer_SchedulesNewOrchestrationAndReturnsAccepted()
        {
            // Arrange
            var caseId = _fixture.Create<int>();

            var transferRequest = _fixture.Build<TransferRequest>()
                .With(x => x.Metadata, _fixture.Build<TransferMetadata>().With(m => m.CaseId, caseId).Create())
                .Create();

            var validResult = new ValidatableRequest<TransferRequest>
            {
                IsValid = true,
                Value = transferRequest
            };

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<TransferRequest, TransferRequestValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(validResult);

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForCaseIdAsync(caseId))
                .ReturnsAsync((CaseMetadata?)null);

            _caseMetadataServiceMock
                .Setup(x => x.UpdateActiveTransferIdAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            var durableClientMock = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };

            string? actualInstanceId = null;

            durableClientMock
                .Setup(client => client.ScheduleNewOrchestrationInstanceAsync(
                    It.IsAny<TaskName>(),
                    It.IsAny<object>(),
                    It.IsAny<StartOrchestrationOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<TaskName, object, StartOrchestrationOptions, CancellationToken>((name, input, options, ct) =>
                {
                    actualInstanceId = options.InstanceId;

                    var payload = Assert.IsType<TransferPayload>(input);
                    Assert.Equal(caseId, payload.CaseId);
                    Assert.Equal(transferRequest.TransferType, payload.TransferType);
                    Assert.Equal(transferRequest.DestinationPath, payload.DestinationPath);
                    Assert.Equal(transferRequest.SourcePaths, payload.SourcePaths);
                    Assert.Equal(transferRequest.Metadata.UserName, payload.UserName);
                    Assert.Equal(transferRequest.Metadata.WorkspaceId, payload.WorkspaceId);
                    Assert.Equal(transferRequest.TransferDirection, payload.TransferDirection);

                    Assert.True(Guid.TryParse(options.InstanceId, out _));
                })
                .ReturnsAsync(() => actualInstanceId!);

            var request = CreateHttpRequestFor(transferRequest);

            // Act
            var result = await _function.Run(request, durableClientMock.Object);

            // Assert
            var accepted = Assert.IsType<AcceptedResult>(result);
            var response = Assert.IsType<TransferResponse>(accepted.Value);

            var expectedGuid = Guid.Parse(actualInstanceId!);
            Assert.Equal(expectedGuid, response.Id);
            Assert.Equal(TransferStatus.Initiated, response.Status);
            Assert.True((DateTime.UtcNow - response.CreatedAt).TotalSeconds < 10);
            Assert.Equal($"/api/v1/filetransfer/{expectedGuid}/status", accepted.Location);

            _caseMetadataServiceMock.Verify(x => x.UpdateActiveTransferIdAsync(caseId, expectedGuid), Times.Once);
        }
        private HttpRequest CreateHttpRequestFor<T>(T obj)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(obj);
            request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
            request.ContentType = "application/json";
            request.Headers[HttpHeaderKeys.CorrelationId] = _testCorrelationId.ToString();
            return request;
        }
    }
}
