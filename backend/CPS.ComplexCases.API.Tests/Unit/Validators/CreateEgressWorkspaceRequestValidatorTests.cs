using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Validators.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class CreateEgressWorkspaceRequestValidatorTests
{
    private readonly CreateEgressWorkspaceRequestValidator _validator;

    public CreateEgressWorkspaceRequestValidatorTests()
    {
        _validator = new CreateEgressWorkspaceRequestValidator();
    }

    [Fact]
    public void Validate_WhenCaseIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 0,
            TemplateId = "TestTemplate"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenTemplateIdIsOmitted_ReturnsNoValidationError(string? templateId)
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 1,
            TemplateId = templateId
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ReturnsNoValidationError()
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 1,
            TemplateId = "TestTemplate"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenCaseIdIsEmptyAndTemplateIdIsOmitted_ReturnsOnlyCaseIdError()
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 0
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
    }
}
