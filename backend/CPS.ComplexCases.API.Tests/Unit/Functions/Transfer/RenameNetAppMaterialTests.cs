using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Functions.Transfer;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using Moq;
using CommonRenameRequest = CPS.ComplexCases.Common.Models.Requests.RenameNetAppMaterialRequest;

namespace CPS.ComplexCases.API.Tests.Unit.Functions.Transfer;

public class RenameNetAppMaterialTests
{
    private readonly Mock<ILogger<RenameNetAppMaterial>> _loggerMock;
    private readonly Mock<IFileTransferClient> _fileTransferClientMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly RenameNetAppMaterial _function;
    private readonly Fixture _fixture;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;
    private readonly string _testBearerToken;
    private const string BucketName = "test-bucket";

    public RenameNetAppMaterialTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<RenameNetAppMaterial>>();
        _fileTransferClientMock = new Mock<IFileTransferClient>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();

        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();
        _testBearerToken = _fixture.Create<string>();

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
            .ReturnsAsync([
                new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = BucketName,
                    DisplayName = "Test Security Group"
                }
            ]);

        _function = new RenameNetAppMaterial(
            _loggerMock.Object,
            _fileTransferClientMock.Object,
            _requestValidatorMock.Object,
            _securityGroupMetadataServiceMock.Object);
    }

    [Fact]
    public async Task Run_InvalidRequest_ReturnsBadRequest()
    {
        var validationErrors = new List<string> { "SourcePath is required." };
        var incomingRequest = CreateValidIncomingRequest();

        SetupRequestValidator(incomingRequest, isValid: false, errors: validationErrors);

        var result = await _function.Run(
            HttpRequestStubHelper.CreateHttpRequestFor(incomingRequest),
            CreateFunctionContext());

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(validationErrors, bad.Value);
        _fileTransferClientMock.Verify(
            c => c.RenameNetAppMaterialAsync(It.IsAny<CommonRenameRequest>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_ValidRequest_CallsTransferClientWithEnrichedRequest()
    {
        var incomingRequest = CreateValidIncomingRequest();
        SetupRequestValidator(incomingRequest, isValid: true);

        CommonRenameRequest? capturedRequest = null;

        _fileTransferClientMock
            .Setup(c => c.RenameNetAppMaterialAsync(It.IsAny<CommonRenameRequest>(), It.IsAny<Guid>()))
            .Callback<CommonRenameRequest, Guid>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("renamed", Encoding.UTF8, "application/json")
            });

        var result = await _function.Run(
            HttpRequestStubHelper.CreateHttpRequestFor(incomingRequest),
            CreateFunctionContext());

        Assert.IsType<ContentResult>(result);

        Assert.NotNull(capturedRequest);
        Assert.Equal(incomingRequest.CaseId, capturedRequest!.CaseId);
        Assert.Equal(incomingRequest.SourcePath, capturedRequest.SourcePath);
        Assert.Equal(incomingRequest.DestinationPath, capturedRequest.DestinationPath);
        Assert.Equal(_testBearerToken, capturedRequest.BearerToken);
        Assert.Equal(BucketName, capturedRequest.BucketName);
        Assert.Equal(_testUsername, capturedRequest.Username);
    }

    [Fact]
    public async Task Run_ValidRequest_PassesCorrelationIdToClient()
    {
        var incomingRequest = CreateValidIncomingRequest();
        SetupRequestValidator(incomingRequest, isValid: true);

        Guid? capturedCorrelationId = null;

        _fileTransferClientMock
            .Setup(c => c.RenameNetAppMaterialAsync(It.IsAny<CommonRenameRequest>(), It.IsAny<Guid>()))
            .Callback<CommonRenameRequest, Guid>((_, id) => capturedCorrelationId = id)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("renamed", Encoding.UTF8, "application/json")
            });

        await _function.Run(
            HttpRequestStubHelper.CreateHttpRequestFor(incomingRequest),
            CreateFunctionContext());

        Assert.Equal(_testCorrelationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Run_DownstreamReturns404_Proxies404()
    {
        var incomingRequest = CreateValidIncomingRequest();
        SetupRequestValidator(incomingRequest, isValid: true);

        _fileTransferClientMock
            .Setup(c => c.RenameNetAppMaterialAsync(It.IsAny<CommonRenameRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await _function.Run(
            HttpRequestStubHelper.CreateHttpRequestFor(incomingRequest),
            CreateFunctionContext());

        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(404, statusResult.StatusCode);
    }

    [Fact]
    public async Task Run_DownstreamReturns409_Proxies409()
    {
        var incomingRequest = CreateValidIncomingRequest();
        SetupRequestValidator(incomingRequest, isValid: true);

        _fileTransferClientMock
            .Setup(c => c.RenameNetAppMaterialAsync(It.IsAny<CommonRenameRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("file is locked", Encoding.UTF8, "application/json")
            });

        var result = await _function.Run(
            HttpRequestStubHelper.CreateHttpRequestFor(incomingRequest),
            CreateFunctionContext());

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal(409, contentResult.StatusCode);
    }

    private void SetupRequestValidator(
        RenameNetAppMaterialRequest request,
        bool isValid,
        List<string>? errors = null)
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<RenameNetAppMaterialRequest, RenameNetAppMaterialRequestValidator>(
                It.IsAny<Microsoft.AspNetCore.Http.HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<RenameNetAppMaterialRequest>
            {
                Value = request,
                IsValid = isValid,
                ValidationErrors = errors ?? []
            });
    }

    private FunctionContext CreateFunctionContext() =>
        FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

    private static RenameNetAppMaterialRequest CreateValidIncomingRequest() =>
        new()
        {
            CaseId = 42,
            SourcePath = "materials/case42/document.pdf",
            DestinationPath = "materials/case42/renamed-document.pdf"
        };
}
