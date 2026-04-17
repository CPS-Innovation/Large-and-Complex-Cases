using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class DeleteNetAppBatchRequestValidatorTests
{
    private readonly DeleteNetAppBatchRequestValidator _validator;

    public DeleteNetAppBatchRequestValidatorTests()
    {
        _validator = new DeleteNetAppBatchRequestValidator();
    }

    [Fact]
    public void Validate_WhenOperationsIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations = []
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.Operations) && e.ErrorMessage == "Operations cannot be empty.");
    }

    [Fact]
    public void Validate_WhenOperationsHasDuplicateSourcePaths_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "folder/file.txt" },
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "folder/file.txt" }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.Operations) && e.ErrorMessage == "Duplicate sourcePath values are not permitted in a single batch.");
    }

    [Fact]
    public void Validate_WhenOperationsHasCaseInsensitiveDuplicateSourcePaths_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "folder/file.txt" },
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "FOLDER/FILE.TXT" }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.Operations) && e.ErrorMessage == "Duplicate sourcePath values are not permitted in a single batch.");
    }

    [Fact]
    public void Validate_WhenOperationSourcePathIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = string.Empty }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == "Operations[0].SourcePath" && e.ErrorMessage == "SourcePath is required.");
    }

    [Fact]
    public void Validate_WhenOperationSourcePathContainsDoubleDots_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "folder/../file.txt" }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == "Operations[0].SourcePath" && e.ErrorMessage == "SourcePath cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Validate_WhenOperationSourcePathStartsWithSlash_ReturnsValidationError()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "/folder/file.txt" }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == "Operations[0].SourcePath" && e.ErrorMessage == "SourcePath cannot start with a '/'.");
    }

    [Fact]
    public void Validate_WhenOperationSourcePathHasMultipleViolations_ReturnsMultipleErrors()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "/../file.txt" }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.PropertyName == "Operations[0].SourcePath" && e.ErrorMessage == "SourcePath cannot contain '..' to navigate up directories.");
        Assert.Contains(result.Errors, e => e.PropertyName == "Operations[0].SourcePath" && e.ErrorMessage == "SourcePath cannot start with a '/'.");
    }

    [Fact]
    public void Validate_WhenMultipleOperationsHaveInvalidSourcePaths_ReturnsErrorsForEach()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "/folder/file.txt" },
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Folder, SourcePath = "folder/../subfolder" }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Operations[0].SourcePath" && e.ErrorMessage == "SourcePath cannot start with a '/'.");
        Assert.Contains(result.Errors, e => e.PropertyName == "Operations[1].SourcePath" && e.ErrorMessage == "SourcePath cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new DeleteNetAppBatchDto
        {
            CaseId = 1,
            Operations =
            [
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Material, SourcePath = "folder/file.txt" },
                new DeleteNetAppBatchOperationDto { Type = NetAppDeleteOperationType.Folder, SourcePath = "folder/subfolder" }
            ]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
