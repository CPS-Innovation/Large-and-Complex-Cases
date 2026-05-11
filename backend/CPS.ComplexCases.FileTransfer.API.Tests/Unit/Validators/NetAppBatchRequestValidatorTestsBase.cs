using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public abstract class NetAppBatchRequestValidatorTestsBase<TRequest, TOperation, TValidator>
    where TRequest : class, INetAppBatchRequest<TOperation>
    where TOperation : class, INetAppBatchOperationRequest
    where TValidator : NetAppBatchRequestValidatorBase<TRequest, TOperation>, new()
{
    private readonly TValidator _validator = new();

    protected abstract TRequest CreateValidRequest(List<TOperation>? ops = null);
    protected abstract TOperation CreateOperation(string type, string sourcePath);
    protected abstract TRequest CreateRequest(int caseId, string destinationPrefix, string bearerToken, string bucketName, List<TOperation> operations);

    [Fact]
    public void Validate_WhenValidSingleMaterial_IsValid()
    {
        var result = _validator.TestValidate(CreateValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenCaseIdIsNotPositive_ReturnsValidationError(int caseId)
    {
        var request = CreateValidRequest();
        request.CaseId = caseId;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CaseId);
    }

    [Fact]
    public void Validate_WhenOperationsIsEmpty_ReturnsValidationError()
    {
        var result = _validator.TestValidate(CreateValidRequest([]));
        result.ShouldHaveValidationErrorFor(x => x.Operations);
    }

    [Fact]
    public void Validate_WhenOperationsExceedsMaximum_ReturnsValidationError()
    {
        var ops = Enumerable.Range(1, NetAppBatchCopyValidationRules.MaxOperations + 1)
            .Select(i => CreateOperation("Material", $"CaseRoot/file{i}.txt"))
            .ToList();
        var result = _validator.TestValidate(CreateValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage($"A batch may not contain more than {NetAppBatchCopyValidationRules.MaxOperations} operations.");
    }

    [Fact]
    public void Validate_WhenDestinationPrefixDoesNotEndWithSlash_ReturnsValidationError()
    {
        var request = CreateValidRequest();
        request.DestinationPrefix = "CaseRoot/Folder-B";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DestinationPrefix)
            .WithErrorMessage("DestinationPrefix must end with '/'.");
    }

    [Fact]
    public void Validate_WhenDestinationPrefixContainsPathTraversal_ReturnsValidationError()
    {
        var request = CreateValidRequest();
        request.DestinationPrefix = "CaseRoot/../evil/";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DestinationPrefix)
            .WithErrorMessage("DestinationPrefix must not contain path traversal sequences ('..').");
    }

    [Fact]
    public void Validate_WhenDestinationPrefixStartsWithSlash_ReturnsValidationError()
    {
        var request = CreateValidRequest();
        request.DestinationPrefix = "/CaseRoot/Folder-B/";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DestinationPrefix)
            .WithErrorMessage("DestinationPrefix must not start with '/'.");
    }

    [Fact]
    public void Validate_WhenBearerTokenIsMissing_ReturnsValidationError()
    {
        var request = CreateValidRequest();
        request.BearerToken = string.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BearerToken);
    }

    [Fact]
    public void Validate_WhenBucketNameIsMissing_ReturnsValidationError()
    {
        var request = CreateValidRequest();
        request.BucketName = string.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BucketName);
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePaths_ReturnsValidationError()
    {
        var ops = new List<TOperation>
        {
            CreateOperation("Material", "CaseRoot/file.txt"),
            CreateOperation("Material", "CaseRoot/file.txt"),
        };
        var result = _validator.TestValidate(CreateValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Duplicate sourcePath values are not permitted in a single batch.");
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePathsCaseInsensitive_ReturnsValidationError()
    {
        var ops = new List<TOperation>
        {
            CreateOperation("Material", "CaseRoot/File.txt"),
            CreateOperation("Material", "CaseRoot/file.txt"),
        };
        var result = _validator.TestValidate(CreateValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Duplicate sourcePath values are not permitted in a single batch.");
    }

    [Fact]
    public void Validate_WhenFolderAndFileInsideFolderInSameBatch_ReturnsOverlapValidationError()
    {
        var ops = new List<TOperation>
        {
            CreateOperation("Folder", "CaseRoot/Reports/"),
            CreateOperation("Material", "CaseRoot/Reports/file.txt"),
        };
        var result = _validator.TestValidate(CreateValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Operations contain overlapping paths. A folder and a file inside that folder cannot both be in the same batch.");
    }

    [Fact]
    public void Validate_WhenSourcePathContainsPathTraversal_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation("Material", "CaseRoot/../secret.txt") };
        var result = _validator.TestValidate(CreateValidRequest(ops));
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("'..'"));
    }

    [Fact]
    public void Validate_WhenSourcePathStartsWithSlash_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation("Material", "/CaseRoot/file.txt") };
        var result = _validator.TestValidate(CreateValidRequest(ops));
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("start with '/'"));
    }

    [Fact]
    public void Validate_WhenFolderSourcePathDoesNotEndWithSlash_ReturnsValidationError()
    {
        var ops = new List<TOperation> { CreateOperation("Folder", "CaseRoot/Old-Folder") };
        var result = _validator.TestValidate(CreateValidRequest(ops));
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("end with a '/'"));
    }

    [Fact]
    public void Validate_WhenFolderDestinationIsChildOfSource_ReturnsValidationError()
    {
        var request = CreateRequest(1, "CaseRoot/Folder-A/Sub/", "token", "bucket",
            [CreateOperation("Folder", "CaseRoot/Folder-A/")]);
        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("child"));
    }

    [Fact]
    public void Validate_WhenMaterialSourceAndDestinationAreSameKey_ReturnsValidationError()
    {
        var request = CreateRequest(1, "CaseRoot/", "token", "bucket",
            [CreateOperation("Material", "CaseRoot/file.txt")]);
        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("same"));
    }

    [Fact]
    public void Validate_WhenValidMixedBatch_IsValid()
    {
        var ops = new List<TOperation>
        {
            CreateOperation("Material", "CaseRoot/Folder-A/report.pdf"),
            CreateOperation("Material", "CaseRoot/Folder-A/evidence.docx"),
            CreateOperation("Folder", "CaseRoot/Old-Folder/"),
        };
        var request = CreateRequest(12345, "CaseRoot/Folder-B/", "token", "bucket", ops);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenFolderDestinationIsNotChildOfSource_IsValid()
    {
        var request = CreateRequest(1, "CaseRoot/Folder-B/", "token", "bucket",
            [CreateOperation("Folder", "CaseRoot/Folder-A/")]);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
