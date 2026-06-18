using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.NetApp.Models.Requests;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class MaterialRenameRequestValidatorTests
{
    private readonly MaterialRenameRequestValidator _validator = new();

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
    public void Should_Not_Have_Error_When_CaseId_Is_Positive()
    {
        var request = CreateValidRequest();
        request.CaseId = 1;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.CaseId);
    }

    [Fact]
    public void Should_Have_Error_When_CurrentPath_Is_Empty()
    {
        var request = CreateValidRequest();
        request.CurrentPath = "";

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPath)
            .WithErrorMessage("Current material path is required.");
    }

    [Fact]
    public void Should_Have_Error_When_CurrentPath_Is_Null()
    {
        var request = CreateValidRequest();
        request.CurrentPath = null!;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPath)
            .WithErrorMessage("Current material path is required.");
    }

    [Fact]
    public void Should_Have_Error_When_CurrentPath_Is_Whitespace()
    {
        var request = CreateValidRequest();
        request.CurrentPath = "   ";

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPath)
            .WithErrorMessage("Current material path is required.");
    }

    [Fact]
    public void Should_Have_Error_When_CurrentPath_Exceeds_Maximum_Length()
    {
        var request = CreateValidRequest();
        request.CurrentPath = new string('a', 261);

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPath)
            .WithErrorMessage("Current material path cannot exceed 260 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_CurrentPath_Is_At_Maximum_Length()
    {
        var request = CreateValidRequest();
        request.CurrentPath = new string('a', 260);

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPath);
    }

    [Fact]
    public void Should_Have_Error_When_NewPath_Is_Empty()
    {
        var request = CreateValidRequest();
        request.NewPath = "";

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPath)
            .WithErrorMessage("New material path is required.");
    }

    [Fact]
    public void Should_Have_Error_When_NewPath_Is_Null()
    {
        var request = CreateValidRequest();
        request.NewPath = null!;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPath)
            .WithErrorMessage("New material path is required.");
    }

    [Fact]
    public void Should_Have_Error_When_NewPath_Is_Whitespace()
    {
        var request = CreateValidRequest();
        request.NewPath = "   ";

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPath)
            .WithErrorMessage("New material path is required.");
    }

    [Fact]
    public void Should_Have_Error_When_NewPath_Exceeds_Maximum_Length()
    {
        var request = CreateValidRequest();
        request.NewPath = new string('b', 261);

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPath)
            .WithErrorMessage("New material path cannot exceed 260 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_NewPath_Is_At_Maximum_Length()
    {
        var request = CreateValidRequest();
        request.NewPath = new string('b', 260);

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPath);
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Request_Is_Invalid()
    {
        var request = new MaterialRenameDto
        {
            CaseId = 0,
            CurrentPath = "",
            NewPath = ""
        };

        var result = _validator.TestValidate(request);

        Assert.True(result.Errors.Count >= 3);
        result.ShouldHaveValidationErrorFor(x => x.CaseId);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPath);
        result.ShouldHaveValidationErrorFor(x => x.NewPath);
    }

    private static MaterialRenameDto CreateValidRequest() =>
        new()
        {
            CaseId = 42,
            CurrentPath = "case/current-path.pdf",
            NewPath = "case/new-path.pdf"
        };
}
