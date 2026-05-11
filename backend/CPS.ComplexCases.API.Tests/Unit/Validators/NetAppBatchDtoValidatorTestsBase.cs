using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public abstract class NetAppBatchDtoValidatorTestsBase<TDto, TOperation, TValidator>
    where TDto : class, INetAppBatchDto<TOperation>
    where TOperation : class, INetAppBatchOperationDto
    where TValidator : NetAppBatchDtoValidatorBase<TDto, TOperation>, new()
{
    private readonly TValidator _validator = new();

    protected abstract TDto CreateValidBatch(List<TOperation>? ops = null);
    protected abstract TOperation CreateOperation(NetAppBatchOperationType type, string sourcePath);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenCaseIdIsNotPositive_ReturnsValidationError(int caseId)
    {
        var request = CreateValidBatch();
        request.CaseId = caseId;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.CaseId));
    }

    [Fact]
    public void Validate_WhenOperationsIsEmpty_ReturnsValidationError()
    {
        var request = CreateValidBatch([]);
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Operations));
    }

    [Fact]
    public void Validate_WhenOperationsExceedsMaximum_ReturnsValidationError()
    {
        var ops = Enumerable.Range(1, NetAppBatchCopyValidationRules.MaxOperations + 1)
            .Select(i => CreateOperation(NetAppBatchOperationType.Material, $"CaseRoot/file{i}.txt"))
            .ToList();
        var result = _validator.Validate(CreateValidBatch(ops));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("more than"));
    }

    [Fact]
    public void Validate_WhenDestinationPrefixDoesNotEndWithSlash_ReturnsValidationError()
    {
        var request = CreateValidBatch();
        request.DestinationPrefix = "CaseRoot/Folder-B";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("end with '/'"));
    }

    [Fact]
    public void Validate_WhenDestinationPrefixContainsPathTraversal_ReturnsValidationError()
    {
        var request = CreateValidBatch();
        request.DestinationPrefix = "CaseRoot/../evil/";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("'..'"));
    }

    [Fact]
    public void Validate_WhenDestinationPrefixStartsWithSlash_ReturnsValidationError()
    {
        var request = CreateValidBatch();
        request.DestinationPrefix = "/CaseRoot/Folder-B/";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("start with '/'"));
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePaths_ReturnsValidationError()
    {
        var ops = new List<TOperation>
        {
            CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/file.txt"),
            CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/file.txt"),
        };
        var result = _validator.Validate(CreateValidBatch(ops));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Duplicate"));
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePathsCaseInsensitive_ReturnsValidationError()
    {
        var ops = new List<TOperation>
        {
            CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/File.txt"),
            CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/file.txt"),
        };
        var result = _validator.Validate(CreateValidBatch(ops));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Duplicate"));
    }

    [Fact]
    public void Validate_WhenFolderAndFileInsideFolderInSameBatch_ReturnsOverlapValidationError()
    {
        var ops = new List<TOperation>
        {
            CreateOperation(NetAppBatchOperationType.Folder, "CaseRoot/Reports/"),
            CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/Reports/file.txt"),
        };
        var result = _validator.Validate(CreateValidBatch(ops));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("overlapping"));
    }

    [Fact]
    public void Validate_WhenFolderDestinationIsChildOfSource_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation(NetAppBatchOperationType.Folder, "CaseRoot/Folder-A/") };
        var request = CreateValidBatch(ops);
        request.DestinationPrefix = "CaseRoot/Folder-A/Sub/";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("child"));
    }

    [Fact]
    public void Validate_WhenMaterialSourceAndDestinationAreSameKey_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/file.txt") };
        var request = CreateValidBatch(ops);
        request.DestinationPrefix = "CaseRoot/";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("same"));
    }

    [Fact]
    public void Validate_WhenSourcePathContainsPathTraversal_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/../secret.txt") };
        var result = _validator.Validate(CreateValidBatch(ops));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("'..'"));
    }

    [Fact]
    public void Validate_WhenFolderSourcePathDoesNotEndWithSlash_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation(NetAppBatchOperationType.Folder, "CaseRoot/Old-Folder") };
        var result = _validator.Validate(CreateValidBatch(ops));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("end with a '/'"));
    }

    [Fact]
    public void Validate_WhenSourcePathStartsWithSlash_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation(NetAppBatchOperationType.Material, "/CaseRoot/file.txt") };
        var result = _validator.Validate(CreateValidBatch(ops));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("start with '/'"));
    }

    [Fact]
    public void Validate_WhenValidMixedBatch_IsValid()
    {
        var ops = new List<TOperation>
        {
            CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/Folder-A/report.pdf"),
            CreateOperation(NetAppBatchOperationType.Material, "CaseRoot/Folder-A/evidence.docx"),
            CreateOperation(NetAppBatchOperationType.Folder, "CaseRoot/Old-Folder/"),
        };
        var request = CreateValidBatch(ops);
        request.DestinationPrefix = "CaseRoot/Folder-B/";
        request.CaseId = 12345;
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValidSingleMaterial_IsValid()
    {
        var result = _validator.Validate(CreateValidBatch());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenFolderDestinationIsNotChildOfSource_IsValid()
    {
        var ops = new List<TOperation> { CreateOperation(NetAppBatchOperationType.Folder, "CaseRoot/Folder-A/") };
        var request = CreateValidBatch(ops);
        request.DestinationPrefix = "CaseRoot/Folder-B/";
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
