using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class TransferRequestValidatorTests
{
    private readonly TransferRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_DestinationPath_Is_Empty()
    {
        var request = CreateValidRequest();
        request.DestinationPath = "";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DestinationPath)
            .WithErrorMessage("DestinationPath is required.");
    }

    [Fact]
    public void Should_Have_Error_When_TransferDirection_Is_Invalid()
    {
        var request = CreateValidRequest();
        request.TransferDirection = (TransferDirection)999;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TransferDirection)
            .WithErrorMessage("TransferDirection must be either EgressToNetApp or NetAppToEgress.");
    }

    [Fact]
    public void Should_Have_Error_When_TransferType_Is_Invalid()
    {
        var request = CreateValidRequest();
        request.TransferType = (TransferType)999;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TransferType)
            .WithErrorMessage("TransferType must be either Copy or Move.");
    }

    [Fact]
    public void Should_Have_Error_When_SourcePaths_Is_Empty()
    {
        var request = CreateValidRequest();
        request.SourcePaths = new List<TransferSourcePath>();

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SourcePaths)
            .WithErrorMessage("At least one SourcePath is required.");
    }

    [Fact]
    public void Should_Not_Have_Error_For_Valid_Request()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static TransferRequest CreateValidRequest()
    {
        return new TransferRequest
        {
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.NetAppToEgress,
            DestinationPath = "/target/folder",
            SourcePaths = new List<TransferSourcePath>
            {
                new() { Path = "/source/file1.txt" }
            },
            Metadata = new TransferMetadata
            {
                CaseId = 1234,
                UserName = "user@example.com",
                WorkspaceId = "ws-5678",
                BearerToken = "fakeBearerToken",
                BucketName = "test-bucket"
            }
        };
    }
}
