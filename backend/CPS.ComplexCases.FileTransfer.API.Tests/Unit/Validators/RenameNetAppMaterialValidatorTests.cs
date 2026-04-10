using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class RenameNetAppMaterialValidatorTests
{
    private readonly RenameNetAppMaterialValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Error_For_Valid_Request()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CaseId_Is_Zero()
    {
        var request = CreateValidRequest();
        request.CaseId = 0;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CaseId)
            .WithErrorMessage("CaseId must be greater than 0.");
    }

    [Fact]
    public void Should_Have_Error_When_CaseId_Is_Negative()
    {
        var request = CreateValidRequest();
        request.CaseId = -1;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CaseId)
            .WithErrorMessage("CaseId must be greater than 0.");
    }

    [Fact]
    public void Should_Have_Error_When_SourcePath_Is_Empty()
    {
        var request = CreateValidRequest();
        request.SourcePath = "";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SourcePath)
            .WithErrorMessage("SourcePath is required.");
    }

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
    public void Should_Have_Error_When_Source_And_Destination_Are_The_Same()
    {
        var request = CreateValidRequest();
        request.DestinationPath = request.SourcePath;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Source and destination paths must be different.");
    }

    [Fact]
    public void Should_Have_Error_When_SourcePath_Contains_Path_Traversal()
    {
        var request = CreateValidRequest();
        request.SourcePath = "materials/case1/../../../etc/passwd";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SourcePath)
            .WithErrorMessage("SourcePath must not contain path traversal sequences ('..').");
    }

    [Fact]
    public void Should_Have_Error_When_DestinationPath_Contains_Path_Traversal()
    {
        var request = CreateValidRequest();
        request.DestinationPath = "materials/case1/../../../etc/passwd";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DestinationPath)
            .WithErrorMessage("DestinationPath must not contain path traversal sequences ('..').");
    }

    [Fact]
    public void Should_Have_Error_When_SourcePath_Ends_With_Slash()
    {
        var request = CreateValidRequest();
        request.SourcePath = "materials/case42/";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SourcePath)
            .WithErrorMessage("SourcePath must not end with '/' (must be a file path, not a folder).");
    }

    [Fact]
    public void Should_Have_Error_When_DestinationPath_Ends_With_Slash()
    {
        var request = CreateValidRequest();
        request.DestinationPath = "materials/case42/";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DestinationPath)
            .WithErrorMessage("DestinationPath must not end with '/' (must be a file path, not a folder).");
    }

    [Fact]
    public void Should_Have_Error_When_BearerToken_Is_Empty()
    {
        var request = CreateValidRequest();
        request.BearerToken = "";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.BearerToken)
            .WithErrorMessage("BearerToken is required.");
    }

    [Fact]
    public void Should_Have_Error_When_BucketName_Is_Empty()
    {
        var request = CreateValidRequest();
        request.BucketName = "";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.BucketName)
            .WithErrorMessage("BucketName is required.");
    }

    private static RenameNetAppMaterialRequest CreateValidRequest() =>
        new()
        {
            CaseId = 42,
            SourcePath = "materials/case42/document.pdf",
            DestinationPath = "materials/case42/renamed-document.pdf",
            BearerToken = "test-bearer-token",
            BucketName = "flexgroup4",
            Username = "user@example.com"
        };
}
