using AutoFixture;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class CreateNetAppConnectionTests
    {
        private readonly Mock<ILogger<CreateNetAppConnection>> _loggerMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<INetAppClient> _netAppClientMock;
        private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IRequestValidator> _requestValidatorMock;
        private readonly Mock<IUserBucketAccessService> _userBucketAccessServiceMock;
        private readonly Mock<IInitializationHandler> _initializationHandlerMock;
        private readonly CreateNetAppConnection _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;
        private readonly string _testBucketName = "test-bucket";

        public CreateNetAppConnectionTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<CreateNetAppConnection>>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _netAppClientMock = new Mock<INetAppClient>();
            _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _requestValidatorMock = new Mock<IRequestValidator>();
            _userBucketAccessServiceMock = new Mock<IUserBucketAccessService>();
            _initializationHandlerMock = new Mock<IInitializationHandler>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();

            _userBucketAccessServiceMock
                .Setup(s => s.ResolveBucketAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = _testBucketName,
                    VolumeUuid = _fixture.Create<Guid>(),
                    DisplayName = "Test Security Group"
                });

            _function = new CreateNetAppConnection(
                _loggerMock.Object,
                _caseMetadataServiceMock.Object,
                _netAppClientMock.Object,
                _netAppArgFactoryMock.Object,
                _activityLogServiceMock.Object,
                _requestValidatorMock.Object,
                _userBucketAccessServiceMock.Object,
                _initializationHandlerMock.Object
                );
        }

        [Fact]
        public async Task Run_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var validationErrors = _fixture.CreateMany<string>(2).ToList();
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = false,
                    ValidationErrors = validationErrors,
                    Value = netAppConnectionRequest
                });

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
            Assert.Equal(validationErrors, errors);
        }

        [Fact]
        public async Task Run_NetAppClientReturnsNull_ReturnsUnauthorized()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(
                    _testBearerToken,
                    _testBucketName,
                    netAppConnectionRequest.OperationName,
                    null,
                    1,
                    null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Run_ValidRequestWithPermission_CreatesConnectionAndReturnsOk()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();
            var netAppResponse = _fixture.Create<ListNetAppObjectsDto>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync(netAppResponse);

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(Enumerable.Empty<CaseMetadata>());

            _caseMetadataServiceMock
                .Setup(x => x.CreateNetAppConnectionAsync(netAppConnectionRequest))
                .Returns(Task.CompletedTask);

            _activityLogServiceMock
                .Setup(x => x.CreateActivityLogAsync(
                    ActivityLog.Enums.ActionType.ConnectionToNetApp,
                    ActivityLog.Enums.ResourceType.StorageConnection,
                    netAppConnectionRequest.CaseId,
                    netAppConnectionRequest.NetAppFolderPath,
                    netAppConnectionRequest.NetAppFolderPath,
                    _testUsername, null))
                .Returns(Task.CompletedTask);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<OkResult>(result);

            _caseMetadataServiceMock.Verify(x => x.CreateNetAppConnectionAsync(netAppConnectionRequest), Times.Once);
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.ConnectionToNetApp,
                ActivityLog.Enums.ResourceType.StorageConnection,
                netAppConnectionRequest.CaseId,
                netAppConnectionRequest.NetAppFolderPath,
                netAppConnectionRequest.NetAppFolderPath,
                _testUsername, null), Times.Once);
        }

        [Fact]
        public async Task Run_WhenFolderPathAlreadyConnected_ReturnsConflict()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();
            var netAppResponse = _fixture.Create<ListNetAppObjectsDto>();
            var existingCaseId = _fixture.Create<int>();
            var folderPath = netAppConnectionRequest.NetAppFolderPath;
            var legacyPath = $"{_testBucketName}:{folderPath}";

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync(netAppResponse);

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForNetAppFolderPathsAsync(It.Is<IEnumerable<string>>(paths =>
                    paths.Contains(folderPath) && paths.Contains(legacyPath))))
                .ReturnsAsync([
                    new CaseMetadata
                    {
                        CaseId = existingCaseId,
                        NetappFolderPath = folderPath
                    }
                ]);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(
                $"Folder path '{folderPath}' is already connected to another case.",
                conflict.Value);

            _caseMetadataServiceMock.Verify(x => x.GetCaseMetadataForNetAppFolderPathsAsync(It.Is<IEnumerable<string>>(paths =>
                paths.Contains(folderPath) && paths.Contains(legacyPath))), Times.Once);
            _caseMetadataServiceMock.Verify(x => x.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains(folderPath) &&
                        v.ToString()!.Contains(existingCaseId.ToString()) &&
                        v.ToString()!.Contains(netAppConnectionRequest.CaseId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Run_WhenLegacyBucketPathAlreadyConnected_ReturnsConflict()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();
            var netAppResponse = _fixture.Create<ListNetAppObjectsDto>();
            var existingCaseId = _fixture.Create<int>();
            var folderPath = netAppConnectionRequest.NetAppFolderPath;
            var legacyPath = $"{_testBucketName}:{folderPath}";

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync(netAppResponse);

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForNetAppFolderPathsAsync(It.Is<IEnumerable<string>>(paths =>
                    paths.Contains(folderPath) && paths.Contains(legacyPath))))
                .ReturnsAsync([
                    new CaseMetadata
                    {
                        CaseId = existingCaseId,
                        NetappFolderPath = legacyPath
                    }
                ]);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(
                $"Folder path '{folderPath}' is already connected to another case.",
                conflict.Value);

            _caseMetadataServiceMock.Verify(x => x.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
        }

        [Fact]
        public async Task Run_NetAppArgFactoryCalledWithCorrectParameters()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _netAppArgFactoryMock.Verify(x => x.CreateListFoldersInBucketArg(
                _testBearerToken,
                _testBucketName,
                netAppConnectionRequest.OperationName,
                null,
                1,
                null), Times.Once);
        }

        [Fact]
        public async Task Run_NetAppClientCalledWithCorrectArg()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _netAppClientMock.Verify(x => x.ListFoldersInBucketAsync(netAppArg), Times.Once);
        }

        [Fact]
        public async Task Run_OnlyCallsCaseMetadataAndActivityLogWhenUserHasPermission()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _caseMetadataServiceMock.Verify(x => x.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
            _activityLogServiceMock.Verify(x => x.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(), null), Times.Never);
        }

        [Fact]
        public async Task Run_WhenActivityLogThrows_StillReturnsOk()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();
            var netAppResponse = _fixture.Create<ListNetAppObjectsDto>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync(netAppResponse);

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(Enumerable.Empty<CaseMetadata>());

            _caseMetadataServiceMock
                .Setup(x => x.CreateNetAppConnectionAsync(netAppConnectionRequest))
                .Returns(Task.CompletedTask);

            _activityLogServiceMock
                .Setup(x => x.CreateActivityLogAsync(
                    It.IsAny<ActivityLog.Enums.ActionType>(),
                    It.IsAny<ActivityLog.Enums.ResourceType>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    null))
                .ThrowsAsync(new Exception("Activity log unavailable"));

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert — connection creation succeeded; logging failure must not surface as an error
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Run_WhenNoBucketNameSupplied_PersistsResolvedBucket()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            netAppConnectionRequest.BucketName = null;

            ArrangeSuccessfulConnect(netAppConnectionRequest);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<OkResult>(result);
            _caseMetadataServiceMock.Verify(
                x => x.CreateNetAppConnectionAsync(It.Is<CreateNetAppConnectionDto>(dto => dto.BucketName == _testBucketName)),
                Times.Once);
        }

        [Fact]
        public async Task Run_WhenBucketNameSupplied_ValidatesAgainstEntitlementAndPersistsIt()
        {
            // Arrange
            var requestedBucket = "manchester-bucket";
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            netAppConnectionRequest.BucketName = requestedBucket;

            _userBucketAccessServiceMock
                .Setup(s => s.ResolveBucketAsync(_testBearerToken, null, requestedBucket))
                .ReturnsAsync(new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = requestedBucket,
                    VolumeUuid = _fixture.Create<Guid>(),
                    DisplayName = "Manchester"
                });

            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, requestedBucket, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync(_fixture.Create<ListNetAppObjectsDto>());

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(Enumerable.Empty<CaseMetadata>());

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            Assert.IsType<OkResult>(result);
            _userBucketAccessServiceMock.Verify(s => s.ResolveBucketAsync(_testBearerToken, null, requestedBucket), Times.Once);
            _caseMetadataServiceMock.Verify(
                x => x.CreateNetAppConnectionAsync(It.Is<CreateNetAppConnectionDto>(dto => dto.BucketName == requestedBucket)),
                Times.Once);
        }

        [Fact]
        public async Task Run_WhenUserNotEntitledToRequestedBucket_ThrowsMissingSecurityGroupException()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            netAppConnectionRequest.BucketName = "not-entitled-bucket";

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _userBucketAccessServiceMock
                .Setup(s => s.ResolveBucketAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ThrowsAsync(new MissingSecurityGroupException("User is not entitled to bucket 'not-entitled-bucket'."));

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act & Assert — the exception middleware maps this to a 403
            await Assert.ThrowsAsync<MissingSecurityGroupException>(() => _function.Run(request, functionContext));

            _caseMetadataServiceMock.Verify(x => x.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
        }

        private void ArrangeSuccessfulConnect(CreateNetAppConnectionDto netAppConnectionRequest)
        {
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync(_fixture.Create<ListNetAppObjectsDto>());

            _caseMetadataServiceMock
                .Setup(x => x.GetCaseMetadataForNetAppFolderPathsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(Enumerable.Empty<CaseMetadata>());
        }
    }
}