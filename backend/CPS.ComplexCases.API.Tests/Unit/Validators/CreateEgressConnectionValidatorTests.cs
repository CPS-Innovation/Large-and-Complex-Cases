using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class CreateEgressConnectionValidatorTests
{
    private readonly CreateEgressConnectionValidator _validator;

    public CreateEgressConnectionValidatorTests()
    {
        _validator = new CreateEgressConnectionValidator();
    }

    [Fact]
    public void Validate_WhenCaseIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateEgressConnectionDto
        {
            CaseId = 0,
            EgressWorkspaceId = "TestWorkspace",
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
    }

    [Fact]
    public void Validate_WhenEgressWorkspaceIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateEgressConnectionDto
        {
            CaseId = 1,
            EgressWorkspaceId = string.Empty,
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.EgressWorkspaceId) && e.ErrorMessage == "EgressWorkspaceId is required.");
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ReturnsNoValidationError()
    {
        // Arrange
        var request = new CreateEgressConnectionDto
        {
            CaseId = 1,
            EgressWorkspaceId = "TestWorkspace",
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}