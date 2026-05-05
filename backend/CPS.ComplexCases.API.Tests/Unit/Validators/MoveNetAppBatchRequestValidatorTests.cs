using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class MoveNetAppBatchRequestValidatorTests
{
    private readonly MoveNetAppBatchRequestValidator _validator = new();

    private static MoveNetAppBatchDto ValidBatch(List<MoveNetAppBatchOperationDto>? ops = null) => new()
    {
        CaseId = 1,
        DestinationPrefix = "CaseRoot/Folder-B/",
        Operations = ops ?? [new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/file.txt" }]
    };

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenCaseIdIsNotPositive_ReturnsValidationError(int caseId)
    {
        var request = ValidBatch();
        request.CaseId = caseId;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.CaseId));
    }

    [Fact]
    public void Validate_WhenOperationsIsEmpty_ReturnsValidationError()
    {
        var request = ValidBatch([]);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Operations));
    }

    [Fact]
    public void Validate_WhenOperationsExceedsMaximum_ReturnsValidationError()
    {
        var ops = Enumerable.Range(1, MoveNetAppBatchRequestValidator.MaxOperations + 1)
            .Select(i => new MoveNetAppBatchOperationDto { Type = NetAppMoveOperationType.Material, SourcePath = $"CaseRoot/file{i}.txt" })
            .ToList();
        var request = ValidBatch(ops);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("more than"));
    }

    [Fact]
    public void Validate_WhenDestinationPrefixDoesNotEndWithSlash_ReturnsValidationError()
    {
        var request = ValidBatch();
        request.DestinationPrefix = "CaseRoot/Folder-B";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("end with '/'"));
    }

    [Fact]
    public void Validate_WhenDestinationPrefixContainsPathTraversal_ReturnsValidationError()
    {
        var request = ValidBatch();
        request.DestinationPrefix = "CaseRoot/../evil/";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("'..'"));
    }

    [Fact]
    public void Validate_WhenDestinationPrefixStartsWithSlash_ReturnsValidationError()
    {
        var request = ValidBatch();
        request.DestinationPrefix = "/CaseRoot/Folder-B/";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("start with '/'"));
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePaths_ReturnsValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/file.txt" },
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/file.txt" },
        };
        var request = ValidBatch(ops);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Duplicate"));
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePathsCaseInsensitive_ReturnsValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/File.txt" },
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/file.txt" },
        };
        var request = ValidBatch(ops);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Duplicate"));
    }

    [Fact]
    public void Validate_WhenFolderAndFileInsideFolderInSameBatch_ReturnsOverlapValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Folder, SourcePath = "CaseRoot/Reports/" },
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/Reports/file.txt" },
        };
        var request = ValidBatch(ops);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("overlapping"));
    }

    [Fact]
    public void Validate_WhenFolderMoveDestinationIsChildOfSource_ReturnsValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Folder, SourcePath = "CaseRoot/Folder-A/" },
        };
        var request = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = "CaseRoot/Folder-A/Sub/",
            Operations = ops
        };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("child"));
    }

    [Fact]
    public void Validate_WhenMaterialSourceAndDestinationAreSameKey_ReturnsValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/file.txt" },
        };
        var request = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = "CaseRoot/",
            Operations = ops
        };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("same"));
    }

    [Fact]
    public void Validate_WhenSourcePathContainsPathTraversal_ReturnsValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/../secret.txt" },
        };
        var request = ValidBatch(ops);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("'..'"));
    }

    [Fact]
    public void Validate_WhenFolderSourcePathDoesNotEndWithSlash_ReturnsValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Folder, SourcePath = "CaseRoot/Old-Folder" },
        };
        var request = ValidBatch(ops);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("end with a '/'"));
    }

    [Fact]
    public void Validate_WhenSourcePathStartsWithSlash_ReturnsValidationError()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "/CaseRoot/file.txt" },
        };
        var request = ValidBatch(ops);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("start with '/'"));
    }

    [Fact]
    public void Validate_WhenValidMixedBatch_IsValid()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/Folder-A/report.pdf" },
            new() { Type = NetAppMoveOperationType.Material, SourcePath = "CaseRoot/Folder-A/evidence.docx" },
            new() { Type = NetAppMoveOperationType.Folder, SourcePath = "CaseRoot/Old-Folder/" },
        };
        var request = new MoveNetAppBatchDto
        {
            CaseId = 12345,
            DestinationPrefix = "CaseRoot/Folder-B/",
            Operations = ops
        };
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValidSingleMaterial_IsValid()
    {
        var result = _validator.Validate(ValidBatch());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenFolderMoveDestinationIsNotChildOfSource_IsValid()
    {
        var ops = new List<MoveNetAppBatchOperationDto>
        {
            new() { Type = NetAppMoveOperationType.Folder, SourcePath = "CaseRoot/Folder-A/" },
        };
        var request = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = "CaseRoot/Folder-B/",
            Operations = ops
        };
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
