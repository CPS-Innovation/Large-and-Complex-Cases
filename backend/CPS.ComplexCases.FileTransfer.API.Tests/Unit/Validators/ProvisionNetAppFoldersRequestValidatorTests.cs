using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class ProvisionNetAppFoldersRequestValidatorTests
{
    private readonly ProvisionNetAppFoldersRequestValidator _validator = new();

    private static ProvisionNetAppFoldersRequest CreateRequest(string destination, string template) => new()
    {
        CaseId = 1,
        Urn = "URN",
        TemplateName = template,
        DestinationFolderPath = destination,
        BucketName = "bucket",
        BearerToken = "token",
        CaseName = "Case-1",
        OperationName = "Op"
    };

    [Theory]
    [InlineData("Case-1/", "_templates/Standard/")]
    [InlineData("Area1/TeamA/Case-1/", "Area1/TeamA/_templates/Standard/")]
    public void Validate_WhenDestinationAndTemplateAreValid_HasNoErrors(string destination, string template)
    {
        var result = _validator.TestValidate(CreateRequest(destination, template));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("_templates/Case-1/")]
    [InlineData("Area1/_templates/Case-1/")]
    [InlineData("Area1/_Templates/Standard/Case-1/")]
    public void Validate_WhenDestinationIsInsideTemplatesFolder_HasError(string destination)
    {
        var parent = destination[..destination.TrimEnd('/').LastIndexOf('/')] + "/";
        var result = _validator.TestValidate(CreateRequest(destination, $"{parent}_templates/Standard/"));

        result.ShouldHaveValidationErrorFor(x => x.DestinationFolderPath)
            .WithErrorMessage("Case folders cannot be created inside a _templates folder.");
    }

    [Theory]
    [InlineData("Area1/TeamA/Case-1/", "Area1/TeamB/_templates/Standard/")]
    [InlineData("Area1/TeamA/Case-1/", "_templates/Standard/")]
    [InlineData("Case-1/", "Area1/_templates/Standard/")]
    [InlineData("Case-1/", "_templates/")]
    public void Validate_WhenTemplateIsNotUnderDestinationParent_HasError(string destination, string template)
    {
        var result = _validator.TestValidate(CreateRequest(destination, template));

        result.ShouldHaveValidationErrorFor(x => x.TemplateName)
            .WithErrorMessage("Template must be in the _templates/ folder directly under the destination's parent folder.");
    }

    [Theory]
    [InlineData("Case-1")]
    [InlineData("Area1/../Case-1/")]
    [InlineData("")]
    public void Validate_WhenDestinationIsMalformed_HasError(string destination)
    {
        var result = _validator.TestValidate(CreateRequest(destination, "_templates/Standard/"));

        result.ShouldHaveValidationErrorFor(x => x.DestinationFolderPath);
    }

    [Theory]
    [InlineData("_templates/Standard")]
    [InlineData("_templates/../Standard/")]
    public void Validate_WhenTemplateIsMalformed_HasError(string template)
    {
        var result = _validator.TestValidate(CreateRequest("Case-1/", template));

        result.ShouldHaveValidationErrorFor(x => x.TemplateName);
    }

    [Theory]
    [InlineData("", "TemplateName is required.")]
    [InlineData("_templates/Standard", "TemplateName must end with '/'.")]
    [InlineData("_templates/../Standard/", "TemplateName cannot contain '..' to navigate up directories.")]
    public void Validate_WhenDestinationIsEmpty_StillValidatesTemplateName(string template, string expectedMessage)
    {
        var result = _validator.TestValidate(CreateRequest("", template));

        result.ShouldHaveValidationErrorFor(x => x.TemplateName)
            .WithErrorMessage(expectedMessage);
    }
}
