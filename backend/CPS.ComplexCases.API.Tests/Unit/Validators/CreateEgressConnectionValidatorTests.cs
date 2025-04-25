using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;
using FluentAssertions;

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
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
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
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.EgressWorkspaceId) && e.ErrorMessage == "EgressWorkspaceId is required.");
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
        result.IsValid.Should().BeTrue();
    }
}