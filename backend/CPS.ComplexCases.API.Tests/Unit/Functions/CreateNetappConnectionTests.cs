using AutoFixture;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class CreateNetAppConnectionTests
    {
        private readonly Mock<ILogger<CreateNetAppConnection>> _loggerMock;
        private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
        private readonly Mock<INetAppClient> _netAppClientMock;
        private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
        private readonly Mock<IOptions<NetAppOptions>> _optionsMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<IRequestValidator> _requestValidatorMock;
        private readonly CreateNetAppConnection _function;
        private readonly Fixture _fixture;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly NetAppOptions _netAppOptions;

        public CreateNetAppConnectionTests()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<CreateNetAppConnection>>();
            _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
            _netAppClientMock = new Mock<INetAppClient>();
            _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
            _optionsMock = new Mock<IOptions<NetAppOptions>>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _requestValidatorMock = new Mock<IRequestValidator>();

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _netAppOptions = _fixture.Create<NetAppOptions>();

            _optionsMock.Setup(x => x.Value).Returns(_netAppOptions);

            _function = new CreateNetAppConnection(
                _loggerMock.Object,
                _caseMetadataServiceMock.Object,
                _netAppClientMock.Object,
                _netAppArgFactoryMock.Object,
                _optionsMock.Object,
                _activityLogServiceMock.Object,
                _requestValidatorMock.Object);
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
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

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
                    _netAppOptions.BucketName,
                    netAppConnectionRequest.OperationName,
                    null,
                    1,
                    null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

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
                .Setup(x => x.CreateListFoldersInBucketArg(_netAppOptions.BucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync(netAppResponse);

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
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

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
                .Setup(x => x.CreateListFoldersInBucketArg(_netAppOptions.BucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            await _function.Run(request, functionContext);

            // Assert
            _netAppArgFactoryMock.Verify(x => x.CreateListFoldersInBucketArg(
                _netAppOptions.BucketName,
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
                .Setup(x => x.CreateListFoldersInBucketArg(_netAppOptions.BucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

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
                .Setup(x => x.CreateListFoldersInBucketArg(_netAppOptions.BucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

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
        public async Task Run_UsesCorrectNetAppOptionsFromConfiguration()
        {
            // Arrange
            var netAppConnectionRequest = _fixture.Create<CreateNetAppConnectionDto>();
            var netAppArg = _fixture.Create<ListFoldersInBucketArg>();
            var expectedBucketName = _fixture.Create<string>();
            var customNetAppOptions = new NetAppOptions
            {
                BucketName = expectedBucketName,
                Url = _fixture.Create<string>(),
                AccessKey = _fixture.Create<string>(),
                SecretKey = _fixture.Create<string>(),
                RegionName = _fixture.Create<string>()
            };

            var customOptionsMock = new Mock<IOptions<NetAppOptions>>();
            customOptionsMock.Setup(x => x.Value).Returns(customNetAppOptions);

            var customFunction = new CreateNetAppConnection(
                _loggerMock.Object,
                _caseMetadataServiceMock.Object,
                _netAppClientMock.Object,
                _netAppArgFactoryMock.Object,
                customOptionsMock.Object,
                _activityLogServiceMock.Object,
                _requestValidatorMock.Object);

            _requestValidatorMock
                .Setup(x => x.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new ValidatableRequest<CreateNetAppConnectionDto>
                {
                    IsValid = true,
                    Value = netAppConnectionRequest
                });

            _netAppArgFactoryMock
                .Setup(x => x.CreateListFoldersInBucketArg(expectedBucketName, netAppConnectionRequest.OperationName, null, 1, null))
                .Returns(netAppArg);

            _netAppClientMock
                .Setup(x => x.ListFoldersInBucketAsync(netAppArg))
                .ReturnsAsync((ListNetAppObjectsDto?)null);

            var request = HttpRequestStubHelper.CreateHttpRequestFor(netAppConnectionRequest);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername);

            // Act
            await customFunction.Run(request, functionContext);

            // Assert
            _netAppArgFactoryMock.Verify(x => x.CreateListFoldersInBucketArg(
                expectedBucketName,
                netAppConnectionRequest.OperationName,
                null,
                1,
                null
            ), Times.Once);
        }
    }
}