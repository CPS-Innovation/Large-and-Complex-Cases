using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Models.Domain.Enums;
namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class GetFilesForTransferRequestValidatorTests
{
    private readonly GetFilesForTransferRequestValidator _validator;

    public GetFilesForTransferRequestValidatorTests()
    {
        _validator = new GetFilesForTransferRequestValidator();
    }

    [Fact]
    public void Validate_WhenCaseIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 0,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "/source/path" } },
            DestinationPath = "/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "Case ID is required.");
    }

    [Fact]
    public void Validate_WhenTransferDirectionIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = (TransferDirection)999, // Invalid enum value
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "/source/path" } },
            DestinationPath = "/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.TransferDirection) && e.ErrorMessage == "TransferDirection must be either EgressToNetApp or NetAppToEgress.");
    }

    [Fact]
    public void Validate_WhenSourcePathsIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath>(),
            DestinationPath = "/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.SourcePaths) && e.ErrorMessage == "At least one source path is required.");
    }

    [Fact]
    public void Validate_WhenSourcePathContainsEmptyPath_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath>
            {
                new SourcePath { Path = "/valid/path" },
                new SourcePath { Path = string.Empty }
            },
            DestinationPath = "/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.SourcePaths) && e.ErrorMessage == "Source paths cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WhenSourcePathContainsWhitespacePath_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath>
            {
                new SourcePath { Path = "/valid/path" },
                new SourcePath { Path = "   " }
            },
            DestinationPath = "/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.SourcePaths) && e.ErrorMessage == "Source paths cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WhenSourcePathContainsNullPath_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath>
            {
                new SourcePath { Path = "/valid/path" },
                new SourcePath { Path = string.Empty }
            },
            DestinationPath = "/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.SourcePaths) && e.ErrorMessage == "Source paths cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WhenDestinationPathIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "/source/path" } },
            DestinationPath = string.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.DestinationPath) && e.ErrorMessage == "Destination path is required.");
    }

    [Fact]
    public void Validate_WhenDestinationPathIsNull_ReturnsValidationError()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "/source/path" } },
            DestinationPath = string.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.DestinationPath) && e.ErrorMessage == "Destination path is required.");
    }

    [Fact]
    public void Validate_WhenAllPropertiesAreValidWithEgressToNetApp_ReturnsValidationSuccess()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 1,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath>
            {
                new SourcePath { Path = "/source/path1" },
                new SourcePath { Path = "/source/path2" }
            },
            DestinationPath = "/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenAllPropertiesAreValidWithNetAppToEgress_ReturnsValidationSuccess()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 123,
            TransferDirection = TransferDirection.NetAppToEgress,
            SourcePaths = new List<SourcePath>
            {
                new SourcePath { Path = "/another/source/path" }
            },
            DestinationPath = "/another/destination/path"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenMultipleValidationErrorsExist_ReturnsAllErrors()
    {
        // Arrange
        var request = new GetFilesForTransferRequest
        {
            CaseId = 0,
            TransferDirection = (TransferDirection)999,
            SourcePaths = new List<SourcePath>(),
            DestinationPath = string.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.CaseId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.TransferDirection));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.SourcePaths));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.DestinationPath));
    }
}