using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Configuration;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Models.Requests;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class MaterialRenameTests
{
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<IOntapHttpClient> _ontapHttpClientMock;

    private readonly Fixture _fixture;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;
    private readonly string _testBearerToken;

    public MaterialRenameTests()
    {
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _ontapHttpClientMock = new Mock<IOntapHttpClient>();

        _fixture = new Fixture();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();
        _testBearerToken = _fixture.Create<string>();
    }

    [Fact]
    public async Task Run_WhenFeatureFlagIsDisabled_ReturnsNotFound()
    {
        var function = CreateFunction(materialRenameEnabled: false);
        var request = HttpRequestStubHelper.CreateHttpRequest();
        var context = CreateFunctionContext();

        var result = await function.Run(request, context);

        Assert.IsType<NotFoundResult>(result);
        _requestValidatorMock.Verify(
            v => v.GetJsonBody<MaterialRenameDto, MaterialRenameRequestValidator>(It.IsAny<HttpRequest>()),
            Times.Never);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenRequestIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        var validationErrors = new List<string> { "CurrentPath is required.", "NewPath is required." };
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: false, validationErrors);

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        var result = await function.Run(request, context);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(validationErrors, badRequest.Value);

        _initializationHandlerMock.Verify(i => i.Initialize(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int?>()), Times.Never);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenOntapClientReturnsNonOkResult_PassesThroughResult()
    {
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: true);

        var volumeUuid = _fixture.Create<Guid>();
        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync([
                new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    DisplayName = "SG",
                    BucketName = "bucket",
                    VolumeUuid = volumeUuid
                }
            ]);

        var notFoundResult = new NotFoundObjectResult("Material not found at path: old/path");
        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(_testBearerToken, volumeUuid, requestDto.CurrentPath, requestDto.NewPath))
            .ReturnsAsync(notFoundResult);

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        var result = await function.Run(request, context);

        Assert.Same(notFoundResult, result);
        _activityLogServiceMock.Verify(
            a => a.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<System.Text.Json.JsonDocument>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenRenameSucceeds_ReturnsOkObjectResult_AndLogsActivity()
    {
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: true);

        var volumeUuid = _fixture.Create<Guid>();
        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync([
                new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    DisplayName = "SG",
                    BucketName = "bucket",
                    VolumeUuid = volumeUuid
                }
            ]);

        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(_testBearerToken, volumeUuid, requestDto.CurrentPath, requestDto.NewPath))
            .ReturnsAsync(new OkResult());

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        var result = await function.Run(request, context);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Material renamed successfully.", okResult.Value);

        _initializationHandlerMock.Verify(
            i => i.Initialize(_testUsername, _testCorrelationId, requestDto.CaseId),
            Times.Once);

        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(_testBearerToken, volumeUuid, requestDto.CurrentPath, requestDto.NewPath),
            Times.Once);

        _activityLogServiceMock.Verify(
            a => a.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.MaterialRenamed,
                ActivityLog.Enums.ResourceType.Material,
                requestDto.CaseId,
                requestDto.CurrentPath,
                requestDto.NewPath,
                _testUsername,
                It.IsAny<System.Text.Json.JsonDocument>()),
            Times.Once);
    }

    private MaterialRename CreateFunction(bool materialRenameEnabled)
    {
        var featureFlags = Options.Create(new FeatureFlagConfig
        {
            MaterialRename = materialRenameEnabled
        });

        return new MaterialRename(
            _activityLogServiceMock.Object,
            _requestValidatorMock.Object,
            _initializationHandlerMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _ontapHttpClientMock.Object,
            featureFlags);
    }

    private void SetupRequestValidator(MaterialRenameDto dto, bool isValid, List<string>? validationErrors = null)
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MaterialRenameDto, MaterialRenameRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MaterialRenameDto>
            {
                Value = dto,
                IsValid = isValid,
                ValidationErrors = validationErrors ?? []
            });
    }

    private FunctionContext CreateFunctionContext() =>
        FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId,
            _testCmsAuthValues,
            _testUsername,
            _testBearerToken);

    private static MaterialRenameDto CreateValidRequestDto() =>
        new()
        {
            CaseId = 42,
            CurrentPath = "case/old-path",
            NewPath = "case/new-path"
        };
}
