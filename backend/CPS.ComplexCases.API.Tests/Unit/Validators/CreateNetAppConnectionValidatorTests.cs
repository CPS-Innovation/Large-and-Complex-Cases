using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;
using FluentAssertions;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class CreateNetAppConnectionValidatorTests
{
    private readonly CreateNetAppConnectionValidator _validator;

    public CreateNetAppConnectionValidatorTests()
    {
        _validator = new CreateNetAppConnectionValidator();
    }

    [Fact]
    public void Validate_WhenCaseIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateNetAppConnectionDto
        {
            CaseId = 0,
            OperationName = "TestBucket",
            NetAppFolderPath = "/path/to/folder"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
    }

    [Fact]
    public void Validate_WhenBucketNameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateNetAppConnectionDto
        {
            CaseId = 1,
            OperationName = string.Empty,
            NetAppFolderPath = "/path/to/folder"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.OperationName) && e.ErrorMessage == "BucketName is required.");
    }

    [Fact]
    public void Validate_WhenNetAppFolderPathIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateNetAppConnectionDto
        {
            CaseId = 1,
            OperationName = "TestBucket",
            NetAppFolderPath = string.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.NetAppFolderPath) && e.ErrorMessage == "NetAppFolderPath is required.");
    }

    [Fact]

    public void Validate_WhenAllPropertiesAreValid_ReturnsValidationSuccess()
    {
        // Arrange
        var request = new CreateNetAppConnectionDto
        {
            CaseId = 1,
            OperationName = "TestBucket",
            NetAppFolderPath = "/path/to/folder"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}