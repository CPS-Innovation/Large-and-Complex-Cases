using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class DeleteNetAppFileOrFolderRequestValidatorTests
{
    private readonly DeleteNetAppFileOrFolderRequestValidator _validator;

    public DeleteNetAppFileOrFolderRequestValidatorTests()
    {
        _validator = new DeleteNetAppFileOrFolderRequestValidator();
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppFileOrFolderDto
        {
            Path = string.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.Path) && e.ErrorMessage == "Path is required.");
    }

    [Fact]
    public void Validate_WhenPathContainsDoubleDots_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppFileOrFolderDto
        {
            Path = "folder/../file.txt"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.Path) && e.ErrorMessage == "Path cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Validate_WhenPathStartsWithSlash_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppFileOrFolderDto
        {
            Path = "/folder/file.txt"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.Path) && e.ErrorMessage == "Path cannot start with a '/'.");
    }

    [Fact]
    public void Validate_WhenPathIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new DeleteNetAppFileOrFolderDto
        {
            Path = "folder/subfolder/file.txt"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenPathHasMultipleViolations_ReturnsMultipleErrors()
    {
        // Arrange
        var request = new DeleteNetAppFileOrFolderDto
        {
            Path = "/../folder/file.txt"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Path) && e.ErrorMessage == "Path cannot contain '..' to navigate up directories.");
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Path) && e.ErrorMessage == "Path cannot start with a '/'.");
    }
}
