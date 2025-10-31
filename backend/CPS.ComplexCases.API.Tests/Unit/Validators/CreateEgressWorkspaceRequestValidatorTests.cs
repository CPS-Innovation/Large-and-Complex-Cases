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
            Name = "TestWorkspace",
            TemplateId = "TestTemplate",
            Description = "Test Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 1,
            Name = string.Empty,
            TemplateId = "TestTemplate",
            Description = "Test Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.Name) && e.ErrorMessage == "Name is required.");
    }

    [Fact]
    public void Validate_WhenTemplateIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 1,
            Name = "TestWorkspace",
            TemplateId = string.Empty,
            Description = "Test Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.TemplateId) && e.ErrorMessage == "TemplateId is required.");
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ReturnsNoValidationError()
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 1,
            Name = "TestWorkspace",
            TemplateId = "TestTemplate",
            Description = "Test Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenMultipleFieldsAreEmpty_ReturnsMultipleValidationErrors()
    {
        // Arrange
        var request = new CreateEgressWorkspaceRequest
        {
            CaseId = 0,
            Name = string.Empty,
            TemplateId = string.Empty,
            Description = "Test Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Name) && e.ErrorMessage == "Name is required.");
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.TemplateId) && e.ErrorMessage == "TemplateId is required.");
    }
}