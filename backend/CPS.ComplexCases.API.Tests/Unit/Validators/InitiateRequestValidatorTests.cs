using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class InitiateTransferRequestValidatorTests
{
    private readonly InitiateTransferRequestValidator _validator;

    public InitiateTransferRequestValidatorTests()
    {
        _validator = new InitiateTransferRequestValidator();
    }

    [Fact]
    public void Validate_WhenDestinationPathIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = string.Empty,
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.DestinationPath) && e.ErrorMessage == "DestinationPath is required.");
    }

    [Fact]
    public void Validate_WhenTransferTypeIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = (TransferType)999, // Invalid enum value
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.TransferType) && e.ErrorMessage == "TransferType must be either Copy or Move.");
    }

    [Fact]
    public void Validate_WhenTransferDirectionIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Copy,
            TransferDirection = (TransferDirection)999, // Invalid enum value
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
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
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.SourcePaths) && e.ErrorMessage == "At least one SourcePath is required.");
    }

    [Fact]
    public void Validate_WhenCaseIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 0,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.CaseId) && e.ErrorMessage == "CaseId is required.");
    }

    [Fact]
    public void Validate_WhenWorkspaceIdIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = string.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.WorkspaceId) && e.ErrorMessage == "WorkspaceId is required.");
    }


    [Fact]
    public void Validate_WhenNetAppToEgressWithMoveTransferType_ReturnsValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Move,
            TransferDirection = TransferDirection.NetAppToEgress,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(request.TransferType) && e.ErrorMessage == "When TransferDirection is NetAppToEgress, TransferType must be Copy.");
    }

    [Fact]
    public void Validate_WhenNetAppToEgressWithCopyTransferType_ReturnsNoValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.NetAppToEgress,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenEgressToNetAppWithCopyTransferType_ReturnsNoValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenEgressToNetAppWithMoveTransferType_ReturnsNoValidationError()
    {
        // Arrange
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Move,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
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
        var request = new InitiateTransferRequest
        {
            DestinationPath = "test/destination",
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.EgressToNetApp,
            SourcePaths = new List<SourcePath> { new SourcePath { Path = "test/path" } },
            CaseId = 1,
            WorkspaceId = "TestWorkspace"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}